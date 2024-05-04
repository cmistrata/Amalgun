public abstract class TimerBase {
    private bool _automaticallyReset;
    public TimerBase(bool automaticallyReset = true) {
        _automaticallyReset = automaticallyReset;
        Reset();
    }

    public bool HasTimerTripped(float timePassed) {
        bool tripped = HasTimerTrippedImpl(timePassed);
        if (tripped && _automaticallyReset) Reset();
        return tripped;
    }
    public abstract void Reset();
    protected abstract bool HasTimerTrippedImpl(float timePassed);
}
