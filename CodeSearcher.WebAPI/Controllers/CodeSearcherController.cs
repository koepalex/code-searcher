using System.Collections.Generic;
using CodeSearcher.BusinessLogic;
using CodeSearcher.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CodeSearcher.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CodeSearcherController : ControllerBase
    {
        ICodeSearcherLogger m_Logger;
        public CodeSearcherController(ILogger<CodeSearcherController> logger)
        {
            m_Logger = new WebLogAdapter(logger);
        }
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<ICodeSearcherIndex>> Get()
        {
            m_Logger.Info("[GET] - GetAllIndexes");
            var manager = Factory.GetCodeSearcherManager(m_Logger);
            var indexes = manager.GetAllIndexes();
            return new ActionResult<IEnumerable<ICodeSearcherIndex>>(indexes);
        }

        //// GET api/values/5
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
