namespace Plugin.Firebase.UnitTests.Auth;

/// <summary>
/// A <see cref="TimeProvider"/> whose clock only moves on <see cref="Advance"/>, which also fires due one-shot timers.
/// </summary>
internal sealed class ManualTimeProvider : TimeProvider
{
    private readonly List<ManualTimer> _timers = new();
    private DateTimeOffset _now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public int ActiveTimerCount => _timers.Count(x => x.DueAt is not null);

    public override DateTimeOffset GetUtcNow() => _now;

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var timer = new ManualTimer(this, callback, state);
        _timers.Add(timer);
        timer.Change(dueTime, period);
        return timer;
    }

    public void Advance(TimeSpan delta)
    {
        _now += delta;
        foreach(var timer in _timers.ToArray()) {
            timer.FireIfDue(_now);
        }
    }

    private sealed class ManualTimer : ITimer
    {
        private readonly ManualTimeProvider _owner;
        private readonly TimerCallback _callback;
        private readonly object? _state;

        public ManualTimer(ManualTimeProvider owner, TimerCallback callback, object? state)
        {
            _owner = owner;
            _callback = callback;
            _state = state;
        }

        public DateTimeOffset? DueAt { get; private set; }

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            if(period != Timeout.InfiniteTimeSpan) {
                throw new NotSupportedException("Only one-shot timers are supported.");
            }
            DueAt = dueTime == Timeout.InfiniteTimeSpan ? null : _owner._now + dueTime;
            return true;
        }

        public void FireIfDue(DateTimeOffset now)
        {
            if(DueAt is { } dueAt && dueAt <= now) {
                DueAt = null;
                _callback(_state);
            }
        }

        public void Dispose()
        {
            DueAt = null;
            _owner._timers.Remove(this);
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}