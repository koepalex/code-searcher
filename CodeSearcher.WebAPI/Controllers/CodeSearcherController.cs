using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Linq;
using CodeSearcher.BusinessLogic;
using CodeSearcher.Interfaces;
using CodeSearcher.Interfaces.API.Model.Requests;
using CodeSearcher.Interfaces.API.Model.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Hangfire;

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
        private static string m_ManagementInformation = null;

        /// <summary>
        /// Default constructor to create code-searcher Web API controller
        /// </summary>
        /// <param name="logger">Instance where all messages should be logged into</param>
        public CodeSearcherController(ILogger<CodeSearcherController> logger)
        {
            m_Logger = new WebLogAdapter(logger);
            m_Manager = Factory.Get().GetCodeSearcherManager(m_Logger);
            m_Manager.ManagementInformationPath = m_ManagementInformation ?? m_Manager.ManagementInformationPath;
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
        /// <returns>Enumeration of existing indexes, maybe empty enumeration</returns>
        [HttpGet("")]
        public ActionResult<IEnumerable<ICodeSearcherIndex>> GetAllIndexes()
        {
            m_Logger.Info("[GET] /api/CodeSearcher/ (GetAllIndexes)");
            var indexes = m_Manager.GetAllIndexes();
            return new ActionResult<IEnumerable<ICodeSearcherIndex>>(indexes);
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
            m_Logger.Info("[PUT] /api/CodeSearcher/configure");
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
            m_ManagementInformation = model.ManagementInformationPath;
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
            m_Logger.Info("[GET] /api/CodeSearcher/configure");
            return new ConfigureResponse
            {
                ManagementInformationPath = m_ManagementInformation
            };
        }

        /// <summary>
        /// Read all files in the given folder with given file extensions and add them to lucene indey
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
        /// <returns>JSON object contating indexing job id; requried to cancel the indexing job of get updates</returns>
        /// <response code="400">Path doesn't exist, or file extensions are missformed</response>
        [HttpPost("index")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<CreateIndexResponse> CreateNewIndex([FromBody] CreateIndexRequest model)
        {
            m_Logger.Info("[GET] /api/CodeSearcher/index");

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
            var jobId = BackgroundJob.Enqueue(() => CreateIndex(model));
            
            return new CreateIndexResponse
            { 
                IndexingJobId = jobId
            };
        }

        private int CreateIndex(CreateIndexRequest model)
        {
            var indexId = m_Manager.CreateIndex(model.SourcePath, model.FileExtensions);
            return indexId;
        }

        // status: JobStorage.Current.GetMonitoringApi().SucceededJobs()[0].Value.Result
        // delete: BackgroundJob.Delete(jobId) 

        //// GET api/values/5
        ////// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is null</response>            
        //[HttpPost]
        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[HttpGet("{id}")]
        //public ActionResult<string> Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/values
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
