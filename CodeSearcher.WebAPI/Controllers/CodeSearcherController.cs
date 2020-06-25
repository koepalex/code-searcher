using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using CodeSearcher.BusinessLogic;
using CodeSearcher.Interfaces;
using CodeSearcher.Interfaces.API.Model.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
            m_Logger.Info("[GET] - GetAllIndexes");
            var indexes = m_Manager.GetAllIndexes();
            return new ActionResult<IEnumerable<ICodeSearcherIndex>>(indexes);
        }

        /// <summary>
        /// Change the path where the Code Searcher Manager is storing/reading the meta information (Default: %APPDATA%\code-searcher)
        /// </summary>
        /// <param name="model">JSON object containting configuration parameter</param>
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
        public ActionResult SetManagementFolder([FromBody] ConfigureRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.ManagementInformationPath))
            {
                return BadRequest();
            }

            if(!Directory.Exists(model.ManagementInformationPath))
            {
                return BadRequest();
            }

            m_Manager.ManagementInformationPath = model.ManagementInformationPath;
            m_ManagementInformation = model.ManagementInformationPath;
            return Ok();
        }

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
