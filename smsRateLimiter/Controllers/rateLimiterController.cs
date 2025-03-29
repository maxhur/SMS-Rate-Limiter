using Microsoft.AspNetCore.Mvc;
using smsRateRegulator.Models;
using SmsRateLimiter.Services;

namespace smsRateRegulator.Controllers
{
    [ApiController]
    [Route("api/sms")]
    public class SmsRateController : ControllerBase
    {
        private readonly SmsRateService _rateService;

        public SmsRateController(SmsRateService rateService)
        {
            _rateService = rateService;
        }

        [HttpPost("isSendAllowed")]
        [ProducesResponseType(typeof(SmsRateResponse), StatusCodes.Status200OK)]
        public IActionResult CanSend([FromBody] SmsRateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.PhoneNumber))
                return BadRequest("Phone number is required.");

            var cleanedPhoneNumber = CleanPhoneNumber(request.PhoneNumber);
            var isAllowed = _rateService.IsSendAllowed(cleanedPhoneNumber);

            return Ok(new SmsRateResponse { CanSend = isAllowed });
        }

        private string CleanPhoneNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            input = input.Trim();
            bool hasPlus = input.StartsWith("+");
            var digits = new string(input.Where(char.IsDigit).ToArray());
            return hasPlus ? "+" + digits : digits;
        }
    }
}
