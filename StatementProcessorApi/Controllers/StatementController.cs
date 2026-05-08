using Microsoft.AspNetCore.Mvc;
using StatementProcessorApi.Services;
using StatementProcessorModels;

namespace StatementProcessorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatementController(IAutomateService automateService, IReportProcess reportProcess, IStopService stopService) : ControllerBase
    {
        [HttpPost("create", Name = "CreateJob")]
        public async Task<IActionResult> CreateJob([FromBody] JobProcessParameters parameters)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);

            return Ok(await automateService.StartProcess(parameters));

        }


        [HttpPost("compresscomplete", Name = "CompressComplete")]
        public async Task<IActionResult> CompressComplete([FromBody] CompressCompleteParameters parameters)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);

            return Ok(await automateService.CompressionComplete(parameters));
        }

        [HttpPost("filecheck", Name = "FileCheck")]
        public async Task<IActionResult> FileCheck([FromBody] FileCheckParameters parameters)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var appUser = Request.HttpContext.User.Identity?.Name?.Split("\\").Last();

            return Ok(await automateService.ProcessIncoming(parameters, appUser));
        }

        [HttpPost("writesteplog", Name = "WriteStepLog")]
        public async Task<IActionResult> WriteStepLog([FromBody] WriteJobStepParameters parameters)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);

            return Ok(await automateService.WriteJob(parameters));
        }

        [HttpPost("rptprocess", Name = "RptProcess")]
        public async Task<IActionResult> RptProcess([FromBody] ReportProcessParameters parameters)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);

            return Ok(await reportProcess.GenerateReport(parameters));
        }

        [HttpPost("filesready", Name = "FilesReady")]
        public async Task<IActionResult> FilesReady([FromBody] GetJobDataParameters parameters)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);

            return Ok(await automateService.FilesToProcess(parameters));
        }

        [HttpPost("startcompression", Name = "StartCompression")]
        public async Task<IActionResult> StartCompression([FromBody] JobProcessParameters parameters)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);

            return Ok(await automateService.StartCompression(parameters));
        }

        [HttpPost("stop", Name = "StopProcess")]
        public async Task<IActionResult> StopProcess([FromBody] JobProcessParameters parameters)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);

            return Ok(await stopService.StopProcess(parameters));
        }

        [HttpPost("verifyFtp", Name = "VerifyFtp")]
        public async Task<IActionResult> VerifyFtp([FromBody] JobProcessParameters parameters)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);

            return Ok(await automateService.VerifyFtpConnection(parameters.AppUser));
        }







    }
}
