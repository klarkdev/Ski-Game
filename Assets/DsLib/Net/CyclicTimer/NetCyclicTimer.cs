using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;

// Cyclic Timer 2.1 July 04 2016

namespace DsLib
{
    [Serializable]
    public class CyclicTimer
    {
        public enum State { Idle, Warmup, Iterating, Cooldown, Paused }
        public enum StartMode { OnIdleOnly, Forced, OnIdleOrWarmup, OnIdleOrCooldown, OnIdleOrWarmupOrCooldown }

        //[Header("Timer Settings")]
        public float warmupTime = 0f;
        public float cooldownTime = 0f;

        //[Header("Iteration Settings")]
        public int iterations = 1;
        public float iterationPadding = 0f;
        public bool loopIterations = false;
        public bool limitUpdate = false;

        //[Header("Current Status")]
        public float cycleTimeStarted;
        public float cycleTimeRemaining;
        public int iterationsRemaining;
        public bool iterationsInLoop = false;

        public State state;
        State statePrepause = State.Idle;

        // Events
        public event TimerEvent onWarmup;
        public void TriggerOnWarmup() { if (onWarmup != null) onWarmup(); }
        public event TimerEvent onIterate;
        public void TriggerOnIterate() { if (onIterate != null) onIterate(); }
        public event TimerEvent onCooldown;
        public void TriggerOnCooldown() { if (onCooldown != null) onCooldown(); }
        public event TimerEvent onFinish;
        public void TriggerOnFinish() { if (onFinish != null) onFinish(); }

        // Constructors
        public CyclicTimer(float warmupTime, float cooldownTime, int iterations, float iterationPadding, bool loopIterations, bool limitUpdate)
        {
            Configure(warmupTime, cooldownTime, iterations, iterationPadding, loopIterations, limitUpdate);
        }

        public void Configure(float warmupTime, float cooldownTime, int iterations, float iterationPadding, bool loopIterations, bool limitUpdate)
        {
            this.warmupTime = warmupTime;
            this.cooldownTime = cooldownTime;
            this.iterations = iterations;
            this.iterationPadding = iterationPadding;
            this.loopIterations = loopIterations;
            this.limitUpdate = limitUpdate;
        }

        // State Functions
        public void Start()
        {
            Reset();
            StartWarmup();
        }
        public void Start(StartMode mode)
        {
            switch (mode)
            {
                case StartMode.OnIdleOnly:
                    if (!(state == State.Idle))
                        return;
                    break;

                case StartMode.Forced:
                    break;

                case StartMode.OnIdleOrWarmup:
                    if (!(state == State.Idle || state == State.Warmup))
                        return;
                    break;

                case StartMode.OnIdleOrCooldown:
                    if (!(state == State.Idle || state == State.Cooldown))
                        return;
                    break;

                case StartMode.OnIdleOrWarmupOrCooldown:
                    if (!(state == State.Idle || state == State.Warmup || state == State.Cooldown))
                        return;
                    break;
            }

            Start();
        }
        public void Reset()
        {
            state = State.Idle;
            cycleTimeStarted = 0f;
            cycleTimeRemaining = 0f;
            iterationsRemaining = iterations;
            iterationsInLoop = loopIterations;
        }

        public void Update(float deltaTime)
        {
            if (!IsActive())
                return;

            cycleTimeRemaining -= deltaTime;

            if (cycleTimeRemaining <= 0)
            {
                float leftoverTime = -cycleTimeRemaining;

                // Trigger end of the state
                switch (state)
                {
                    case State.Warmup:
                        Iterate();
                        break;
                    case State.Iterating:
                        Iterate();
                        break;
                    case State.Cooldown:
                        Reset();
                        TriggerOnFinish();
                        break;
                }

                // Update with leftover time unless it would cause a stack overflow
                if (leftoverTime > 0f && !limitUpdate && !(loopIterations && iterationPadding <= 0))
                    Update(leftoverTime);
            }
        }
        public void Pause()
        {
            statePrepause = state;
            state = State.Paused;
        }
        public void UnPause()
        {
            if (state == State.Paused)
                state = statePrepause;
        }

        public void StartWarmup()
        {
            state = State.Warmup;
            cycleTimeStarted = warmupTime;
            cycleTimeRemaining = warmupTime;

            TriggerOnWarmup();
        }
        public void Iterate()
        {
            if (iterationsInLoop && iterationsRemaining <= 0)
                iterationsRemaining = iterations;

            if (iterationsRemaining > 0)
            {
                iterationsRemaining--;

                if (iterationsRemaining > 0 || iterationsInLoop || iterations == 1)
                {
                    state = State.Iterating;
                    cycleTimeStarted = iterationPadding;
                    cycleTimeRemaining = cycleTimeStarted;
                }

                TriggerOnIterate();
            }
            else
                StartCooldown();
        }
        public void ExitLoop()
        {
            iterationsInLoop = false;
        }
        public void StartCooldown()
        {
            state = State.Cooldown;
            cycleTimeStarted = cooldownTime;
            cycleTimeRemaining = cooldownTime;
            iterationsRemaining = 0;

            TriggerOnCooldown();
        }

        // Convenience State Queries
        public bool IsIdle()
        {
            return (state == State.Idle);
        }
        public bool IsWarmingUp()
        {
            return (state == State.Warmup);
        }
        public bool IsIterating()
        {
            return (state == State.Iterating);
        }
        public bool IsLoopingIteration()
        {
            return (loopIterations && state == State.Iterating);
        }
        public bool IsCoolingDown()
        {
            return (state == State.Cooldown);
        }
        public bool IsPaused()
        {
            return (state == State.Paused);
        }
        public bool IsActive()
        {
            return !(state == State.Idle || state == State.Paused);
        }

        // Cycle Time Queries
        public float GetCycleTimeRemaining()
        {
            return cycleTimeRemaining;
        }
        public float GetCyclePercentRemaining()
        {
            return cycleTimeRemaining / cycleTimeStarted;
        }
        public float GetCycleTimeElapsed()
        {
            return cycleTimeStarted - cycleTimeRemaining;
        }
        public float GetCyclePercentElapsed()
        {
            return 1f - cycleTimeRemaining / cycleTimeStarted;
        }
    }

    public delegate void TimerEvent();
}

