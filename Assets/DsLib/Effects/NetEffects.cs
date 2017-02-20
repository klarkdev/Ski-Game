using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// Example Use
// Although the readOuts should be collected per position and then passed on, not per object requiring the readout as each inquiry will check distances to all sources.
// Audio is updated automatically.

// Camera
// void Start()
// {
//     DsLib.Effects.AddReceiver(transform, ReceiveReadOut);
// }
// 
// public void ReceiveReadOut(DsLib.Effects.ReadOut readOut)
// {
//     transform.localPosition = readOut.shake.posOffset;
//     transform.localEulerAngles = readOut.shake.rotOffset;
// }

namespace DsLib
{
    public static class Effects
    {
        static Transform effectsObject;
        static List<Receiver> receivers;

        static bool initialized = false;

        static void Initialize()
        {
            if (effectsObject == null)
                effectsObject = new GameObject("DsLib Effects", typeof(AudioListener), typeof(ScrEffectsUpdate)).transform;

            receivers = new List<Receiver>();

            initialized = true;
        }

        public static void AddReceiver(Transform receiverTransform, ReceiveReadOut receiveTarget)
        {
            if (!initialized)
                Initialize();

            receivers.Add(new Receiver(receiverTransform, receiveTarget));
        }

        public static void RemoveReceiver(Transform receiverTransform)
        {
            if (!initialized)
                Initialize();

            List<Receiver> flaggedReceivers = new List<Receiver>();

            foreach (Receiver receiver in receivers)
            {
                if (receiver.position == receiverTransform)
                {
                    flaggedReceivers.Add(receiver);
                }
            }

            foreach (Receiver receiver in flaggedReceivers)
                receivers.Remove(receiver);

            flaggedReceivers.Clear();
        }

        public static void Update()
        {
            if (!initialized)
                Initialize();

            // Update Sources
            if (onUpdateSources != null)
                onUpdateSources.Invoke();

            // Return if no registered Sources
            if (onPollSources == null)
                return;

            // Deliver ReadOuts
            foreach (Receiver receiver in receivers)
            {
                ReadOut cumulativeReadout = ReadOut.zero;

                // Calculates RollOff for each Receiver Position
                onPollSources(receiver.position, ref cumulativeReadout);

                receiver.receiveTarget(cumulativeReadout);
            }
        }

        public static SimpleEvent onUpdateSources;

        public static PollSource onPollSources;

        public delegate void SimpleEvent();
        public delegate void PollSource(Transform receiverTransform, ref ReadOut cumulativeReadOut);
        public delegate void ReceiveReadOut(ReadOut cumulativeReadOut);


        #region Classes

        class Receiver
        {
            public Transform position;
            public ReceiveReadOut receiveTarget;

            public Receiver(Transform position, ReceiveReadOut receiveTarget)
            {
                this.position = position;
                this.receiveTarget = receiveTarget;
            }
        }

        class ScrEffectsUpdate : MonoBehaviour
        {
            void LateUpdate()
            {
                DsLib.Effects.Update();
            }
        }

        [Serializable]
        public struct ReadOut
        {
            public Shake shake;
            public Vibration vibration;

            public ReadOut(Vector3 shakePos, Vector3 shakeRot, float vibrationMotorLeft, float vibrationMotorRight)
            {
                shake.posOffset = shakePos;
                shake.rotOffset = shakeRot;
                vibration.motorLeft = vibrationMotorLeft;
                vibration.motorRight = vibrationMotorRight;
            }

            public ReadOut(Shake shake, Vibration vibration)
            {
                this.shake = shake;
                this.vibration = vibration;
            }

            public static ReadOut zero = new ReadOut(Vector3.zero, Vector3.zero, 0f, 0f);
            public static ReadOut operator *(ReadOut r1, float f1)
            {
                return new ReadOut(r1.shake * f1, r1.vibration * f1);
            }
            public static ReadOut operator +(ReadOut r1, ReadOut r2)
            {
                return new ReadOut(
                    r1.shake + r2.shake,
                    r1.vibration + r2.vibration);
            }
        }

        [Serializable]
        public class Source
        {
            [HideInInspector]
            public Transform parent;
            public RollOff rollOff;

            public List<Clip> clips;

            float loudestListener = 0f;

            public void Initialize(Transform parent)
            {
                clips = new List<Clip>();

                DsLib.Effects.onUpdateSources += Update;
                DsLib.Effects.onPollSources += AnswerPoll;

                this.parent = parent;
            }

            void Update()
            {
                loudestListener = 0f;

                foreach (Clip clip in clips)
                    clip.Update();
            }

            public void AnswerPoll (Transform listener, ref ReadOut cumulativeReadout)
            {
                // Return when listener not in range
                Vector3 positionDelta = (parent.position - listener.position);
                if (positionDelta.sqrMagnitude > rollOff.maxDistance * rollOff.maxDistance)
                    return;

                // Get distance rolloff
                float multiplier = rollOff.GetMultiplier(parent, listener);
                if (multiplier > loudestListener) loudestListener = multiplier;

                ReadOut readoutCurrent = ReadOut.zero;

                foreach (Clip clip in clips)
                    clip.AddToReadOut(ref readoutCurrent, loudestListener);

                // add vibration and shake * multiplier
                cumulativeReadout += readoutCurrent * multiplier;
            }

            public void Detach()
            {
                DsLib.Effects.onUpdateSources -= Update;
                DsLib.Effects.onPollSources -= AnswerPoll;
            }
        }

        [Serializable]
        public class RollOff
        {
            public Mode mode;
            public float minDistance = 1f;
            public float maxDistance = 15f;

            public RollOff(float minDistance, float maxDistance, Mode mode)
            {
                this.minDistance = minDistance;
                this.maxDistance = maxDistance;
                this.mode = mode;
            }

            public float GetMultiplier (Transform source, Transform listener)
            {
                switch (mode)
                {
                    case Mode.Linear:
                        return Mathf.InverseLerp(maxDistance, minDistance, (source.position - listener.position).magnitude);
                    case Mode.Hermite:
                        return DsLib.Math.HermiteNormalized(Mathf.InverseLerp(maxDistance, minDistance, (source.position - listener.position).magnitude));
                    case Mode.Global:
                        return 1f;
                    case Mode.RootTransform:
                        if (source.root == listener.root)
                            return 1f;
                        else
                            return 0f;
                    default:
                        return 0f;
                }
            }

            public enum Mode { Linear, Hermite, Global, RootTransform }
        }

        #region Clips
        public interface Clip
        {
            void Play(Effects.Source source);
            void Stop(Effects.Source source);

            void Pause();
            void UnPause();

            void Update();
            void AddToReadOut(ref ReadOut readOut, float loudestListener);
        }

        [Serializable]
        public class SfxClip : Clip
        {
            public AudioClip sound;
            AudioSource audioSource;
            public float volume = 1f;
            public float pitch = 1f;
            public bool loop = false;
            bool initialized = false;

            void Initialize(Effects.Source source)
            {
                if (initialized)
                    return;

                audioSource = source.parent.gameObject.AddComponent<AudioSource>();
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.spatialBlend = 0f;

                initialized = true;
            }

            public void Play(Effects.Source source)
            {
                Initialize(source);

                audioSource.clip = sound;
                audioSource.Stop();
                audioSource.pitch = pitch;
                audioSource.loop = loop;
                audioSource.clip = sound;
                audioSource.volume = 0f;
                audioSource.Play();

                if (!source.clips.Contains(this))
                    source.clips.Add(this);
            }

            public void Stop(Effects.Source source)
            {
                audioSource.Stop();
                source.clips.Remove(this);
            }

            public void Pause()
            {
                audioSource.Pause();
            }

            public void UnPause()
            {
                audioSource.UnPause();
            }

            public void Update()
            {

            }

            public void AddToReadOut(ref ReadOut readOut, float loudestListener)
            {
                audioSource.volume = volume * loudestListener;
            }
        }

        [Serializable]
        public class ShakeClip : Clip
        {
            [Header("Timing")]
            public float fadeInDuration;
            public float peakDuration;
            public float fadeOutDuration;
            public bool loop;
            DsLib.CyclicTimer fadeTimer;

            [Header("Intensity")]
            public float roughness;
            public Vector3 positionMagnitude;
            public Vector3 rotationMagnitude;

            // private temp values
            float noisePositionOffsetCurrent = 0;
            float noiseRotationOffsetCurrent = 0;
            bool shakeCurrentUpdated = false;
            Shake shakeCurrent;

            public void Play(Effects.Source source)
            {
                if (fadeTimer == null)
                    fadeTimer = new DsLib.CyclicTimer(fadeInDuration, fadeOutDuration, 1, peakDuration, loop, false);
                else
                    fadeTimer.Configure(fadeInDuration, fadeOutDuration, 1, peakDuration, loop, false);

                noisePositionOffsetCurrent = UnityEngine.Random.Range(0f, 100f);
                noiseRotationOffsetCurrent = UnityEngine.Random.Range(0f, 100f);

                if (!source.clips.Contains(this))
                    source.clips.Add(this);

                fadeTimer.Start(DsLib.CyclicTimer.StartMode.Forced);
            }

            public void Stop(Effects.Source source)
            {
                fadeTimer.Reset();
                source.clips.Remove(this);
            }

            public void Pause()
            {
                fadeTimer.Pause();
            }

            public void UnPause()
            {
                fadeTimer.UnPause();
            }

            public void Update()
            {
                fadeTimer.Update(Time.deltaTime);

                if (!fadeTimer.IsActive())
                    return;

                if (fadeTimer.IsWarmingUp())
                {
                    float offsetIncrease = Time.deltaTime * roughness * DsLib.Math.SinerpNormalized(fadeTimer.GetCyclePercentElapsed());
                    noisePositionOffsetCurrent += offsetIncrease;
                    noiseRotationOffsetCurrent += offsetIncrease;
                }
                if (fadeTimer.IsIterating())
                {
                    float offsetIncrease = Time.deltaTime * roughness;
                    noisePositionOffsetCurrent += offsetIncrease;
                    noiseRotationOffsetCurrent += offsetIncrease;
                }
                if (fadeTimer.IsCoolingDown())
                {
                    float offsetIncrease = Time.deltaTime * roughness * DsLib.Math.CoserpNormalized(fadeTimer.GetCyclePercentRemaining());
                    noisePositionOffsetCurrent += offsetIncrease;
                    noiseRotationOffsetCurrent += offsetIncrease;
                }

                shakeCurrentUpdated = false;
            }

            public void AddToReadOut(ref ReadOut readOut, float loudestListener)
            {
                if (shakeCurrentUpdated)
                {
                    readOut.shake += shakeCurrent;
                    return;
                }


                shakeCurrent = Shake.zero;

                if (positionMagnitude != Vector3.zero)
                {
                    shakeCurrent.posOffset.x = (Mathf.PerlinNoise(noisePositionOffsetCurrent, 0) * 2f - 1f) * positionMagnitude.x;
                    shakeCurrent.posOffset.y = (Mathf.PerlinNoise(0, noisePositionOffsetCurrent) * 2f - 1f) * positionMagnitude.y;
                    shakeCurrent.posOffset.z = (Mathf.PerlinNoise(noisePositionOffsetCurrent, noisePositionOffsetCurrent) * 2f - 1f) * positionMagnitude.z;
                }

                if (rotationMagnitude != Vector3.zero)
                {
                    shakeCurrent.rotOffset.x = (Mathf.PerlinNoise(noiseRotationOffsetCurrent, 0) * 2f - 1f) * rotationMagnitude.x;
                    shakeCurrent.rotOffset.y = (Mathf.PerlinNoise(0, noiseRotationOffsetCurrent) * 2f - 1f) * rotationMagnitude.y;
                    shakeCurrent.rotOffset.z = (Mathf.PerlinNoise(noiseRotationOffsetCurrent, noiseRotationOffsetCurrent) * 2f - 1f) * rotationMagnitude.z;
                }

                if (fadeTimer.IsWarmingUp())
                    shakeCurrent *= DsLib.Math.SinerpNormalized(fadeTimer.GetCyclePercentElapsed());
                if (fadeTimer.IsCoolingDown())
                    shakeCurrent *= DsLib.Math.CoserpNormalized(fadeTimer.GetCyclePercentRemaining());

                shakeCurrentUpdated = true;

                readOut.shake += shakeCurrent;
            }
        }
        [Serializable]
        public struct Shake
        {
            public Vector3 posOffset;
            public Vector3 rotOffset;

            public Shake(Vector3 positionOffset, Vector3 rotationOffset)
            {
                this.posOffset = positionOffset;
                this.rotOffset = rotationOffset;
            }

            public static Shake zero = new Shake(Vector3.zero, Vector3.zero);
            public static Shake operator +(Shake s1, Shake s2)
            {
                return new Shake(s1.posOffset + s2.posOffset, s1.rotOffset + s2.rotOffset);
            }
            public static Shake operator *(Shake s1, float f)
            {
                return new Shake(s1.posOffset * f, s1.rotOffset * f);
            }
        }

        [Serializable]
        public class VibrationClip : Clip
        {
            public AnimationCurve vibrationLeft;
            public AnimationCurve vibrationRight;

            public float duration = 1f;
            public float intensity = 1f;
            public bool loop = false;

            // private temp values
            bool paused = false;
            float durationRemaining;
            bool vibrationCurrentUpdated = false;
            Vibration vibrationCurrent;

            public void Play(Effects.Source source)
            {
                durationRemaining = duration;
                if (!source.clips.Contains(this))
                    source.clips.Add(this);
            }

            public void Stop(Effects.Source source)
            {
                durationRemaining = 0f;
                source.clips.Remove(this);
            }

            public void Pause()
            {
                paused = true;
            }

            public void UnPause()
            {
                paused = false;
            }

            public void Update()
            {
                vibrationCurrentUpdated = false;

                if (paused)
                    return;

                if (durationRemaining > 0f)
                    durationRemaining -= Time.deltaTime;

                if (loop && durationRemaining <= 0f)
                    durationRemaining = duration;
            }

            public void AddToReadOut(ref ReadOut readOut, float loudestListener)
            {
                if (paused || durationRemaining <= 0)
                    return;

                if (vibrationCurrentUpdated)
                {
                    readOut.vibration += vibrationCurrent;
                    return;
                }

                float timePercent = 1 - (durationRemaining / duration);
                vibrationCurrent = new Vibration(
                    vibrationLeft.Evaluate(timePercent) * intensity,
                    vibrationRight.Evaluate(timePercent) * intensity);

                vibrationCurrentUpdated = true;

                readOut.vibration += vibrationCurrent;

            }


        }
        [Serializable]
        public struct Vibration
        {
            public float motorLeft;
            public float motorRight;

            public Vibration(float motorLeft, float motorRight)
            {
                this.motorLeft = motorLeft;
                this.motorRight = motorRight;
            }

            public static Vibration zero = new Vibration(0f, 0f);
            public static Vibration operator +(Vibration v1, Vibration v2)
            {
                return new Vibration(
                    Mathf.Max(v1.motorLeft, v2.motorLeft),
                    Mathf.Max(v1.motorRight, v2.motorRight));
            }
            public static Vibration operator *(Vibration v1, float multiplier)
            {
                return new Vibration(v1.motorLeft * multiplier, v1.motorRight * multiplier);
            }
        }
        #endregion Clips

        #endregion Classes
    }
}

