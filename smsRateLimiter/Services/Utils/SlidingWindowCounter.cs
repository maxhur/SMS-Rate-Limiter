using System.Collections.Concurrent;

namespace SmsRateLimiter.Services
{
    public class SlidingWindowCounter
    {
        private readonly object _lock = new();
        private readonly ConcurrentQueue<DateTime> _timestamps = new();
        private int _limit;

        public DateTime LastAccessed { get; private set; }

        public SlidingWindowCounter(int initialLimit)
        {
            _limit = initialLimit;
        }

        public void UpdateLimit(int newLimit)
        {
            lock (_lock)
            {
                _limit = newLimit;
            }
        }

        public bool TryIncrement()
        {
            lock (_lock)
            {
                LastAccessed = DateTime.UtcNow;
                var now = DateTime.UtcNow;

                while (_timestamps.TryPeek(out var t) && (now - t).TotalSeconds >= 1)
                {
                    _timestamps.TryDequeue(out _);
                }

                if (_timestamps.Count < _limit)
                {
                    _timestamps.Enqueue(now);
                    return true;
                }

                return false;
            }
        }

        public void Decrement()
        {
            lock (_lock)
            {
                _timestamps.TryDequeue(out _);
            }
        }
    }
}
