using UnityEngine;
using System.Collections;

namespace DsLib
{
    public class ScrEffectsTest : MonoBehaviour
    {

        public DsLib.Effects.Source effectsSource;
        public DsLib.Effects.SfxClip sfxClip;
        public DsLib.Effects.ShakeClip shakeClip;
        public DsLib.Effects.VibrationClip vibrationClip;

        // Use this for initialization
        void Start()
        {
            effectsSource.Initialize(this.transform);

            sfxClip.Play(effectsSource);
            shakeClip.Play(effectsSource);
            vibrationClip.Play(effectsSource);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

