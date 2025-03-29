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

        /// <summary>
        /// Determines if a message can be sent from the specified phone number without exceeding rate limits.
        /// </summary>
        /// <param name="request">Contains the phone number and optional rate limits.</param>
        /// <returns>A boolean flag indicating whether the message can be sent.</returns>
        [HttpPost("isSendAllowed")]
        [ProducesResponseType(typeof(SmsRateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CanSend([FromBody] SmsRateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.PhoneNumber))
                return BadRequest("Phone number is required.");

            int perNumberLimit = request.PerNumberLimitPerSecond;
            int perAccountLimit = request.PerAccountLimitPerSecond;
            
            var cleanedPhoneNumber = CleanPhoneNumber(request.PhoneNumber);
            var isAllowed = _rateService.IsSendAllowed(cleanedPhoneNumber, perNumberLimit, perAccountLimit);

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
