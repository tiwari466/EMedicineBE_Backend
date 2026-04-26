using Microsoft.AspNetCore.Mvc;
using EMedicineBE.Dto;
namespace EMedicineBE.Controllers
{
    [Route("api/settings")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        // temp storage (replace with DB later)
        private static NotificationSettingsDto _settings = new NotificationSettingsDto
        {
            Email = true,
            Sms = false,
            OrderUpdates = true
        };

        // GET: /api/settings/notifications
        [HttpGet("notifications")]
        public IActionResult GetNotificationSettings()
        {
            return Ok(new
            {
                success = true,
                data = _settings
            });
        }

        // POST: /api/settings/notifications
        [HttpPost("notifications")]
        public IActionResult SaveNotificationSettings([FromBody] NotificationSettingsDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { success = false, message = "Invalid data" });
            }

            _settings = dto;

            return Ok(new
            {
                success = true,
                message = "Settings saved successfully"
            });
        }
    }
}
