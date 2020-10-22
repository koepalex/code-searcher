using System.IO;
using System.Linq;
using CodeSearcher.BusinessLogic;
using CodeSearcher.Interfaces;
using CodeSearcher.Interfaces.API.Model.Requests;
using CodeSearcher.Interfaces.API.Model.Response;
using CodeSearcher.Interfaces.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Hangfire;
using System;
using Microsoft.Extensions.Caching.Memory;
using CodeSearcher.WebAPI.Common;
using Hangfire.Server;

namespace CodeSearcher.WebAPI.Controllers
{
    /// <summary>
    /// Web API controller that provides access to code-searcher functionality
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CodeSearcherController : ControllerBase
    {
        private readonly ICodeSearcherLogger m_Logger;
        private readonly ICodeSearcherManager m_Manager;
        private readonly IMemoryCache m_MemoryCache;

        /// <summary>
        /// Default constructor to create code-searcher Web API controller
        /// </summary>
        /// <param name="logger">Instance where all messages should be logged into</param>
        /// <param name="memoryCache">Instance to cache data</param>
        public CodeSearcherController(ILogger<CodeSearcherController> logger, IMemoryCache memoryCache)
        {
            m_Logger = new WebLogAdapter(logger);
            m_Manager = Factory.Get().GetCodeSearcherManager(m_Logger);
            m_MemoryCache = memoryCache;

            string cachedManagementInformation;
            if (m_MemoryCache.TryGetValue<string>(CacheKeys.ManagementInformationKey, out cachedManagementInformation))
            {
                if (string.Compare(cachedManagementInformation, m_Manager.ManagementInformationPath, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    m_Manager.ManagementInformationPath = cachedManagementInformation;
                }
            }
        }
        
        /// <summary>
        /// Read Read all existing indexes
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/CodeSearcher
        ///     {
        ///     }
        ///     
        /// </remarks>
        /// <returns>Enumeration of existing indexes, maybe empty enumeration <see cref="GetIndexesResponse"/></returns>
        [HttpGet("")]
        public ActionResult<GetIndexesResponse> GetAllIndexes()
        {
            m_Logger.Info($"[GET] {APIRoutes.CodeSearcherRoute} (GetAllIndexes)");
            var indexes = m_Manager.GetAllIndexes();
            return new GetIndexesResponse
            {
                Indexes = indexes.ToArray()
            };
        }

        /// <summary>
        /// Change the current configuration of Code searcher Manager
        /// Currently supported:
        /// * Path where the Code Searcher Manager is storing/reading the meta information (Default: %APPDATA%\code-searcher)
        /// </summary>
        /// <param name="model">JSON object containting configuration parameter, <see cref="ConfigureRequest"/></param>
        /// <returns>StatusCodes only</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT /api/CodeSearcher/configure
        ///     {
        ///         "managementInformationPath" : "__PATH__"
        ///     }
        ///     
        /// </remarks>
        /// <response code="200">Path successfully changed</response>
        /// <response code="400">Path doesn't exist</response>
        [HttpPut("configure")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult SetConfiguration([FromBody] ConfigureRequest model)
        {
            m_Logger.Info($"[PUT] {APIRoutes.ConfigurationRoute}");
            if (string.IsNullOrWhiteSpace(model.ManagementInformationPath))
            {
                m_Logger.Debug("Required parameter ManagementInformationPath is null, empty or whitespace");
                return BadRequest();
            }

            if(!Directory.Exists(model.ManagementInformationPath))
            {
                m_Logger.Debug($"Required parameter ManagementInformationPath point to path that doesn't exist: {model.ManagementInformationPath}");
                return BadRequest();
            }

            m_Manager.ManagementInformationPath = model.ManagementInformationPath;

            m_MemoryCache.Set(CacheKeys.ManagementInformationKey, model.ManagementInformationPath);
            return Ok();
        }

        /// <summary>
        /// Read the current configuration from Code Searcher Manager
        /// Currently supported: 
        /// * Path where the Code Searcher Manager is storing/reading the meta information (Default: %APPDATA%\code-searcher)
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/CodeSearcher/configure
        ///     {
        ///     }
        ///     
        /// </remarks>
        /// <returns>JSON object containting configuration parameter</returns>
        [HttpGet("configure")]
        public ActionResult<ConfigureResponse> GetConfiguration()
        {
            m_Logger.Info($"[GET] {APIRoutes.ConfigurationRoute}");

            var result = new ConfigureResponse();
            string cachedManagementInformation;
            if (m_MemoryCache.TryGetValue<string>(CacheKeys.ManagementInformationKey, out cachedManagementInformation))
            {
                result.ManagementInformationPath = cachedManagementInformation;
            }
            else
            {
                result.ManagementInformationPath = m_Manager.ManagementInformationPath;
            }

            return result;
        }

        /// <summary>
        /// Read all files in the given folder with given file extensions and add them to lucene index
        /// </summary>
        /// <param name="model">JSON object containting requried parameter <see cref="CreateIndexRequest"/></param>
        /// <remarks>
        /// Sample request:
        ///     
        ///     POST  /api/CodeSearcher/index
        ///     {
        ///         "SourcePath" : "__PATH__",
        ///         "FileExtensions" : [".cs", ".csproj", ".xml"]
        ///     }
        ///     
        /// </remarks>
        /// <returns>JSON object contating indexing job id; required to cancel the indexing job of get updates</returns>
        /// <response code="400">Path doesn't exist, or file extensions are missformed</response>
        [HttpPost("index")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<CreateIndexResponse> CreateNewIndex([FromBody] CreateIndexRequest model)
        {
            m_Logger.Info($"[POST] {APIRoutes.CreateIndexRoute}");

            #region Request Model Checks
            if (string.IsNullOrWhiteSpace(model.SourcePath))
            {
                m_Logger.Debug("Required parameter SourcePath is null, empty or whitespace");
                return BadRequest();
            }

            if (!Directory.Exists(model.SourcePath))
            {
                m_Logger.Debug($"Required parameter SourcePath point to path that doesn't exist: {model.SourcePath}");
                return BadRequest();
            }

            var extensionList = model.FileExtensions.ToList();
            if (extensionList.Count == 0)
            {
                m_Logger.Debug("Required parameter FileExtensions contains empty list");
                return BadRequest();
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var ext in extensionList)
            {
                if (!ext.StartsWith("."))
                {
                    m_Logger.Debug($"Required parameter FileExtensions contains entry which don't start with '.': {ext}");
                    return BadRequest();

                }

                foreach (var invalid in invalidChars)
                {
                    if (ext.Contains(invalid))
                    {
                        m_Logger.Debug($"Required parameter FileExtensions contains entry with invalid filename chars: {ext}");
                        return BadRequest();

                    }

                }
            }
            #endregion

            m_Logger.Info($"SourcePath: {model.SourcePath}");
            
            var jobId = BackgroundJob.Enqueue(() => CreateIndex(model, null));
            m_MemoryCache.Set(jobId, -1);
            m_MemoryCache.Set($"{jobId}_IsRunning", true);

            return new CreateIndexResponse
            { 
                IndexingJobId = jobId
            };
        }

        /// <summary>
        /// Read the status of indexing job
        /// </summary>
        /// <param name="model">JSON object containting required parameter <see cref="CreateIndexStatusRequest"/></param>
        /// <remarks>
        /// Sample request:
        ///     
        ///     GET  /api/CodeSearcher/index/search
        ///     {
        ///         "JobId" : "__ID_OF_INDEXING_JOB__"
        ///     }
        ///     
        /// </remarks>
        /// <returns>JSON object contating status of indexing job id and maybe the id of created index</returns>
        /// <response code="400">Empty job id isn't valid</response>
        [HttpGet("index/status")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<CreateIndexStatusResponse> GetIndexingStatus([FromBody] CreateIndexStatusRequest model)
        {
            m_Logger.Info($"[GET] {APIRoutes.CreateIndexStatusRoute}");

            #region Request Model Checks
            if (string.IsNullOrWhiteSpace(model.JobId))
            {
                m_Logger.Debug("Required parameter JobId is null, empty or whitespace");
                return BadRequest();
            }
            #endregion

            bool exists = true;

            if(!m_MemoryCache.TryGetValue<int>(model.JobId, out var indexId))
            {
                m_Logger.Debug("Can't read JobId from memory cache");
                exists = false;
            }

            if (!m_MemoryCache.TryGetValue<bool>($"{model.JobId}_IsRunning", out var isRunning))
            {
                m_Logger.Debug("Can't read running status from memory cache");
                exists = false; 
            }

            return new CreateIndexStatusResponse
            {
                IndexingFinished = !isRunning,
                IndexId = indexId,
                Exists = exists
            };
        }

        /// <summary>
        /// Background job to index folder
        /// </summary>
        /// <param name="model">request model for indexing task</param>
        /// <param name="context">will be substitued by hangfire with context object</param>
        [ApiExplorerSettings(IgnoreApi = true)] // Ignore in OpenAPI definition
        [NonAction] // Ignore for ASP.net Core Controller mapping
        [AutomaticRetry(Attempts = BackgroundJobsConstants.NumberOfRetries)]  //configure hangfire to retry on failure
        public void CreateIndex(CreateIndexRequest model, PerformContext context)
        {
            if (context == null)
            {
                m_Logger.Error($"hangfire.io didn't inject PerformContext instance, didn't save indexId into memory cache ");
                throw new ArgumentNullException(nameof(context));
            }

            string cachedManagementInformation;
            if (m_MemoryCache.TryGetValue<string>(CacheKeys.ManagementInformationKey, out cachedManagementInformation))
            {
                m_Manager.ManagementInformationPath = cachedManagementInformation;
            }

            var indexId = m_Manager.CreateIndex(model.SourcePath, model.FileExtensions);

            m_MemoryCache.Set(context.BackgroundJob.Id, indexId);
            m_MemoryCache.Set($"{context.BackgroundJob.Id}_IsRunning", false);
        }

        /// <summary>
        /// Delete existing lucene index
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     
        ///     DELETE  /api/CodeSearcher/index
        ///     {
        ///         "IndexID" : __ID__
        ///     }
        ///     
        /// </remarks>
        /// <param name="model"></param>
        /// <returns>JSON object indicating if delete operation was successfull</returns>
        [HttpDelete("index")]
        public ActionResult<DeleteIndexResponse> DeleteExistingIndex(DeleteIndexRequest model)
        {
            m_Logger.Info($"[DELETE] {APIRoutes.CreateIndexRoute}");

            bool success = true;
            try
            {
                m_Manager.DeleteIndex(model.IndexID);
            }
            catch (NotSupportedException e)
            {
                m_Logger.Error(e.Message);
                success = false;
            }

            return new DeleteIndexResponse
            {
                Succeeded = success
            };
        }

        /// <summary>
        /// Looking for word in existing index
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/CodeSearcher/search
        ///     {
        ///         "IndexID" : __ID__,
        ///         "SearchWord": "__WORD__"
        ///     }
        ///     
        /// </remarks>
        /// <returns>JSON object contating indexing job id; requried to cancel the indexing job of get updates</returns>
        /// <response code="200">Search was successfull</response>
        /// <response code="400">word to search is null, whitespace or empty</response>
        [HttpPost("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<SearchIndexResponse> SearchExistingIndex(SearchIndexRequest model)
        {
            m_Logger.Info($"[POST] {APIRoutes.SearchInIndexRoute}");

            if(string.IsNullOrWhiteSpace(model.SearchWord))
            {
                m_Logger.Debug("Required parameter SearchWord is null, empty or whitespace");
                return BadRequest();
            }

            if(m_Manager.GetIndexById(model.IndexID) == null)
            {
                m_Logger.Debug("Required parameter IndexID point to non existing index");
                return BadRequest();
            }
            m_Logger.Info($"looking in index {model.IndexID} for {model.SearchWord}");
            var searchResults = m_Manager.SearchInIndex(model.IndexID, model.SearchWord).ToArray();
            
            //indexing using index starting by zero, need to add one for line number
            foreach(var searchResult in searchResults)
            {
                foreach(var finding in searchResult.Findings)
                {
                    finding.LineNumber += 1;
                }
            }

            return new SearchIndexResponse
            {
                Results = searchResults.ToArray()
            };
        }
    }
}
