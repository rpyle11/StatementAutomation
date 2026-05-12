using Microsoft.AspNetCore.Mvc;
using StatementProcessorApi.Services;
using StatementProcessorModels;

namespace StatementProcessorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UiDataController(IUiDataService uiDataService) : ControllerBase
    {
        [HttpPost("jobdata", Name = "GetJobData")]
        public async Task<IActionResult> GetJobData([FromBody] GetJobDataParameters parameters)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);

            return Ok(await uiDataService.GetJobData(parameters));
        }

        [HttpPost("activejob", Name = "GetActiveJob")]
        public async Task<IActionResult> GetActiveJob([FromBody] JobProcessParameters parameters)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);

            return Ok(await uiDataService.GetActiveJob(parameters));
        }


    }
}
