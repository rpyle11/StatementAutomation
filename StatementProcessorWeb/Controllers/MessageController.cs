using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StatementProcessorModels;
using StatementProcessorWeb.Hub;

namespace StatementProcessorWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController(IHubContext<NotificationHub> hub) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] MessageParameters message)
        {

            await hub.Clients.All.SendAsync("ServerMessage", message.Message);

            return Ok(true);
        }
    }
}
