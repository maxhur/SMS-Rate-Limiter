using System.Collections.Concurrent;

namespace SmsRateLimiter.Services
{
  public class SmsRateService
  {
    private readonly int _maxPerNumber;
    private readonly int _maxPerAccount;

    private readonly ConcurrentDictionary<string, int> _perNumberCounter = new();
    private int _accountCounter = 0;

    // Timer to reset the counters every second
    private readonly Timer _resetTimer;


    public SmsRateService(IConfiguration config)
    {
      var limits = config.GetSection("RateLimit");
      _maxPerNumber = limits.GetValue<int>("MaxPerPhoneNumberPerSecond");
      _maxPerAccount = limits.GetValue<int>("MaxPerAccountPerSecond");

      // Initialize the timer to call ResetCounters every second
      _resetTimer = new Timer(ResetCounters, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

    }

    // This method resets the counters
    private void ResetCounters(object? state)
    {
      _perNumberCounter.Clear();
      Interlocked.Exchange(ref _accountCounter, 0);
    }

    // This method checks if sending is allowed based on the rate limits
    public bool IsSendAllowed(string phoneNumber, int maxPerPhoneNumberPerSecond, int maxPerAccountPerSecond)
    {
      // Check if the rate limits are exceeded
      if (maxPerPhoneNumberPerSecond <= 0 || maxPerAccountPerSecond <= 0)
      {
        // set system Default
        maxPerPhoneNumberPerSecond = _maxPerNumber;
        maxPerAccountPerSecond = _maxPerAccount;
      }

      // Update the counters atomically
      {
        var numberCount = _perNumberCounter.AddOrUpdate(phoneNumber, 1, (key, prev) => prev + 1);
        var accountCount = Interlocked.Increment(ref _accountCounter);

        if (numberCount > maxPerPhoneNumberPerSecond || accountCount > maxPerAccountPerSecond)
        {
          _perNumberCounter.AddOrUpdate(phoneNumber, 0, (key, prev) => Math.Max(0, prev - 1));
          Interlocked.Decrement(ref _accountCounter);
          return false;
        }

        return true;
      }
    }

    // Dispose the timer when the service is disposed
    public void Dispose()
    {
      _resetTimer?.Dispose();
    }

  }
}
