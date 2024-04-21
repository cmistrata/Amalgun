using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedDelayTimer : TimerBase
{
    [SerializeField]
    private float _timerPeriodSeconds = 3f;
    private float _curTimer = 0f;
    public FixedDelayTimer(float periodSeconds, bool automaticallyReset = true) : base(automaticallyReset) { }

    public override void Reset() => _curTimer = _timerPeriodSeconds;
    protected override bool HasTimerTrippedImpl(float timePassed)
    {
        return (_curTimer -= timePassed) < 0;
    }
}