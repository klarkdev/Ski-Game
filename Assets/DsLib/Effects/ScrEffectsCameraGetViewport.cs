using UnityEngine;
using System.Collections;

namespace DsLib
{
    [RequireComponent(typeof(Camera))]
    public class ScrEffectsCameraGetViewport : MonoBehaviour
    {
        ScrPlayer scrPlayer;

        public ViewportEntry viewport;

        bool initialized = false;

        void Start()
        {
            if (!initialized)
                Initialize();
        }

        void Initialize()
        {
            scrPlayer = transform.root.GetComponent<ScrPlayer>();

            viewport.camera = GetComponent<Camera>();

            if (scrPlayer == null)
            {
                Debug.LogError("Transform Root of the player object must have ScrPlayer component.");
                return;
            }

            DsLib.PlayerManager.AddViewportEntry(scrPlayer.playerId, viewport);
            initialized = true;
        }

        void OnEnable()
        {
            if (!initialized)
                Initialize();
        }

        void OnDisable()
        {
            if (initialized)
            {
                DsLib.PlayerManager.RemoveViewportEntry(scrPlayer.playerId, viewport);
                initialized = false;
            }
                
        }

        void OnDestroy()
        {
            if (initialized)
            {
                DsLib.PlayerManager.RemoveViewportEntry(scrPlayer.playerId, viewport);
                initialized = false;
            }
                
        }
    }
}
