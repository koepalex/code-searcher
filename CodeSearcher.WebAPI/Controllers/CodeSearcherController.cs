using System.Collections.Generic;
using CodeSearcher.BusinessLogic;
using CodeSearcher.Interfaces;
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

        /// <summary>
        /// Default constructor to create code-searcher Web API controller
        /// </summary>
        /// <param name="logger">Instance where all messages should be logged into</param>
        public CodeSearcherController(ILogger<CodeSearcherController> logger)
        {
            m_Logger = new WebLogAdapter(logger);
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
            var manager = Factory.Get().GetCodeSearcherManager(m_Logger);
            var indexes = manager.GetAllIndexes();
            return new ActionResult<IEnumerable<ICodeSearcherIndex>>(indexes);
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
