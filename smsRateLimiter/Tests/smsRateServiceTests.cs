using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using smsRateRegulator.Controllers;
using smsRateRegulator.Models;
using SmsRateLimiter.Services;
using Xunit;

namespace smsRateRegulator.Tests
{
  public class SmsRateControllerTests
  {
    private SmsRateService CreateService()
    {
      var config = new ConfigurationBuilder()
          .AddInMemoryCollection(new Dictionary<string, string?>
          {
                    { "RateLimit:MaxPerPhoneNumberPerSecond", "2" },
                    { "RateLimit:MaxPerAccountPerSecond", "5" }
          })
          .Build();

      return new SmsRateService(config);
    }

    private SmsRateController CreateController(SmsRateService service)
    {
      return new SmsRateController(service);
    }

    [Fact]
    public void CanSend_Returns_Ok_When_Allowed()
    {
      var service = CreateService();
      var controller = CreateController(service);

      var request = new SmsRateRequest
      {
        PhoneNumber = "(123) 456-7890",
        PerNumberLimitPerSecond = 2,
        PerAccountLimitPerSecond = 5
      };

      var result = controller.CanSend(request) as OkObjectResult;
      Assert.NotNull(result);
      Assert.IsType<SmsRateResponse>(result.Value);
      var response = result.Value as SmsRateResponse;
      Assert.True(response.CanSend);
    }

    [Fact]
    public void CanSend_Returns_BadRequest_When_PhoneNumber_Is_Missing()
    {
      var service = CreateService();
      var controller = CreateController(service);

      var request = new SmsRateRequest
      {
        PhoneNumber = null,
        PerNumberLimitPerSecond = 2,
        PerAccountLimitPerSecond = 5
      };

      var result = controller.CanSend(request);
      Assert.IsType<BadRequestObjectResult>(result);
    }


    [Fact]
    public void CanSend_Returns_False_When_Limit_Exceeded()
    {
      var service = CreateService();
      var controller = CreateController(service);

      var request = new SmsRateRequest
      {
        PhoneNumber = "1234567890",
        PerNumberLimitPerSecond = 1,
        PerAccountLimitPerSecond = 5
      };


      // First call: allowed
      var result1 = controller.CanSend(request) as OkObjectResult;
      Assert.NotNull(result1);
      Assert.IsType<SmsRateResponse>(result1.Value);

      var response1 = (SmsRateResponse)result1.Value!;
      Assert.True(response1.CanSend);

      // Second call: should be blocked
      var result2 = controller.CanSend(request) as OkObjectResult;
      Assert.NotNull(result2);
      Assert.IsType<SmsRateResponse>(result2.Value);

      var response2 = (SmsRateResponse)result2.Value!;
      Assert.False(response2.CanSend);
    }
  }
}
