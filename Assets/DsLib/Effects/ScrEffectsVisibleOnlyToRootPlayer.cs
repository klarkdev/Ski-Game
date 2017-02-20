using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DsLib
{
    public class ScrEffectsVisibleOnlyToRootPlayer : MonoBehaviour
    {
        ScrPlayer scrPlayer;

        public bool includeChildren = true;
        public bool invertVisibility = false;

        Renderer[] renderers;

        bool initialized = false;

        void Start()
        {
            Initialize();
        }

        void Initialize()
        {
            scrPlayer = transform.root.GetComponent<ScrPlayer>();

            if (scrPlayer == null)
            {
                Debug.LogError("Transform Root of the player object must have ScrPlayer component.");
                return;
            }

            renderers = new Renderer[0];

            if (!includeChildren)
            {
                renderers = new Renderer[1];
                renderers[0] = GetComponent<Renderer>();
            }
            else
                renderers = GetComponentsInChildren<Renderer>();

            Camera.onPreCull += SwitchRender;

            initialized = true;
        }

        void SwitchRender(Camera camera)
        {
            if (!initialized)
                Initialize();

            if (PlayerManager.PlayerCameraMatch(scrPlayer.playerId, camera))
            {
                foreach (Renderer renderer in renderers)
                {
                    if (!invertVisibility)
                        renderer.enabled = true;
                    else
                        renderer.enabled = false;
                }
            }
            else
            {
                foreach (Renderer renderer in renderers)
                {
                    if (!invertVisibility)
                        renderer.enabled = false;
                    else
                        renderer.enabled = true;
                }
            }

        }

        void OnDestroy()
        {
            Camera.onPreCull -= SwitchRender;
        }
    }

}
