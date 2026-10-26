namespace KavineshQuartz.Jobs
{
    public class JobStatusService
    {
        private DateTime? _lastRun;
        private DateTime? _nextRun = DateTime.UtcNow.AddMinutes(1);

        private readonly object _lock = new();

        public void MarkSuccess()
        {
            lock (_lock)
            {
                _lastRun = DateTime.UtcNow;
                _nextRun = _lastRun.Value.AddMinutes(1);
            }
        }

        public (DateTime? LastRun, DateTime? NextRun) GetStatus()
        {
            lock (_lock)
            {
                return (_lastRun, _nextRun);
            }
        }
    }
}