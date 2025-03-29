using System.Collections.Concurrent;

namespace SmsRateLimiter.Services
{
  public class SmsRateService
  {
    private readonly ConcurrentDictionary<string, SlidingWindowCounter> _perNumberCounters = new();
    private readonly ConcurrentDictionary<string, DateTime> _lastUsed = new();
    private readonly SlidingWindowCounter _accountCounter;
    private readonly TimeSpan _inactivityThreshold = TimeSpan.FromMinutes(5);


    private readonly int _defaultPerNumberLimit;
    private readonly int _defaultPerAccountLimit;

    private readonly ConcurrentDictionary<string, SlidingWindowCounter> _perNumberCounter = new();


    public SmsRateService(IConfiguration config)
    {
      var limits = config.GetSection("RateLimit");
      _defaultPerNumberLimit = limits.GetValue<int>("MaxPerPhoneNumberPerSecond");
      _defaultPerAccountLimit = limits.GetValue<int>("MaxPerAccountPerSecond");

      _accountCounter = new SlidingWindowCounter(_defaultPerAccountLimit);
    }

    // This method checks if sending is allowed based on the rate limits
    public bool IsSendAllowed(string phoneNumber, int perNumberLimit, int PerAccountLimit)
    {
      if (perNumberLimit <= 0)
      {
        perNumberLimit = _defaultPerNumberLimit;
      }

      if (PerAccountLimit <= 0)
      {
        PerAccountLimit = _defaultPerAccountLimit;
      }

      var numberCount = _perNumberCounter.GetOrAdd(phoneNumber, _ => new SlidingWindowCounter(perNumberLimit));
      numberCount.UpdateLimit(perNumberLimit);
      _accountCounter.UpdateLimit(PerAccountLimit);

      if (!numberCount.TryIncrement())
      {
        return false;
      }

      if (!_accountCounter.TryIncrement())
      {
        numberCount.Decrement();
        return false;
      }


      return true;
    }


    public void CleanupInactiveNumbers()
    {
      var now = DateTime.UtcNow;

      foreach (var when in _lastUsed)
      {
        if ((now - when.Value) > _inactivityThreshold)
        {
          _perNumberCounters.TryRemove(when.Key, out _);
          _lastUsed.TryRemove(when.Key, out _);
        }
      }
    }


  }
}
