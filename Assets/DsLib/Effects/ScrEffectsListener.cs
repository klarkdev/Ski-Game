using UnityEngine;
using System.Collections;

namespace DsLib
{
    public class ScrEffectsListener : MonoBehaviour
    {
        public Effects.Source personalEffects;
        public Effects.Source broadcastEffects;

        public Effects.ReceiveReadOut onReceivedReadOut;

        void Start()
        {
            Initialize();
            personalEffects.Initialize(transform);
        }

        void Initialize()
        {
            Effects.AddReceiver(transform, GetReadOut);

            personalEffects.rollOff = new Effects.RollOff(0f, 0f, Effects.RollOff.Mode.RootTransform);
        }

        void GetReadOut(DsLib.Effects.ReadOut readOut)
        {
            if (onReceivedReadOut != null)
                onReceivedReadOut.Invoke(readOut);
        }

        void OnDestroy()
        {
            personalEffects.Detach();
            broadcastEffects.Detach();
            Effects.RemoveReceiver(transform);
        }
    }
}
