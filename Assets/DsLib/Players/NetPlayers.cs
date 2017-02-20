using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DsLib
{
    public static class PlayerManager
    {
        public static List<Player> players;

        static bool initialized = false;

        static void Initialize()
        {
            GameObject playersObject = new GameObject("DsLib Players", typeof(Camera));

            players = new List<Player>();

            players.Add(new Player(PlayerId.PlayerOne));
            players.Add(new Player(PlayerId.PlayerTwo));
            players.Add(new Player(PlayerId.PlayerThree));
            players.Add(new Player(PlayerId.PlayerFour));

            Camera backgroundCam = playersObject.GetComponent<Camera>();
            backgroundCam.clearFlags = CameraClearFlags.Color;
            backgroundCam.backgroundColor = Color.black;
            backgroundCam.depth = -1000;
            backgroundCam.cullingMask = 0 << 0;

            initialized = true;
        }

        public static Player GetPlayer(PlayerId playerId)
        {
            if (!initialized)
                Initialize();

            switch (playerId)
            {
                case PlayerId.PlayerOne:
                    return players[0];
                case PlayerId.PlayerTwo:
                    return players[1];
                case PlayerId.PlayerThree:
                    return players[2];
                case PlayerId.PlayerFour:
                    return players[3];

                default:
                    return null;
            }
        }

        public static void AddViewportEntry(PlayerId playerId, ViewportEntry viewportEntry)
        {
            if (!initialized)
                Initialize();

            Player currentPlayer = GetPlayer(playerId);

            if (!currentPlayer.viewports.Contains(viewportEntry))
                currentPlayer.viewports.Add(viewportEntry);

            UpdateViewports();
        }

        public static void RemoveViewportEntry(PlayerId playerId, ViewportEntry viewportEntry)
        {
            if (!initialized)
                Initialize();

            Player currentPlayer = GetPlayer(playerId);

            if (currentPlayer.viewports.Contains(viewportEntry))
                currentPlayer.viewports.Remove(viewportEntry);

            UpdateViewports();
        }

        public static bool PlayerCameraMatch(PlayerId playerId, Camera camera)
        {
            foreach (Player viewport in players)
            {
                if (viewport.id == playerId)
                {
                    foreach (ViewportEntry viewportCamera in viewport.viewports)
                    {
                        if (viewportCamera.camera == camera)
                            return true;
                    }

                    break;
                }
            }

            return false;
        }

        static void UpdateViewports()
        {
            List<Player> activePlayers = new List<Player>();

            foreach (Player player in players)
            {
                if (player.viewports.Count > 0)
                    activePlayers.Add(player);
            }

            switch (activePlayers.Count)
            {
                case 0: break;

                case 1:
                    foreach (ViewportEntry viewport in activePlayers[0].viewports)
                    {
                        viewport.camera.rect = new Rect(0f, 0f, 1f, 1f);
                        viewport.camera.fieldOfView = viewport.fovFullScreen;
                    }
                    break;

                case 2:
                    foreach (ViewportEntry viewport in activePlayers[0].viewports)
                    {
                        viewport.camera.rect = new Rect(0f, 1f / 6f, 0.5f, 2f / 3f);
                        viewport.camera.fieldOfView = viewport.fovHalfScreen;
                    }
                    foreach (ViewportEntry viewport in activePlayers[1].viewports)
                    {
                        viewport.camera.rect = new Rect(0.5f, 1f / 6f, 0.5f, 2f / 3f);
                        viewport.camera.fieldOfView = viewport.fovHalfScreen;
                    }
                    break;

                case 3:
                    foreach (ViewportEntry viewport in activePlayers[0].viewports)
                    {
                        viewport.camera.rect = new Rect(0f, 1f / 6f, 0.5f, 2f / 3f);
                        viewport.camera.fieldOfView = viewport.fovHalfScreen;
                    }
                    foreach (ViewportEntry viewport in activePlayers[1].viewports)
                    {
                        viewport.camera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                        viewport.camera.fieldOfView = viewport.fovQuarterScreen;
                    }
                    foreach (ViewportEntry viewport in activePlayers[2].viewports)
                    {
                        viewport.camera.rect = new Rect(0.5f, 0f, 0.5f, 0.5f);
                        viewport.camera.fieldOfView = viewport.fovQuarterScreen;
                    }
                    break;

                case 4:
                    foreach (ViewportEntry viewport in activePlayers[0].viewports)
                    {
                        viewport.camera.rect = new Rect(0f, 0.5f, 0.5f, 0.5f);
                        viewport.camera.fieldOfView = viewport.fovQuarterScreen;
                    }
                    foreach (ViewportEntry viewport in activePlayers[1].viewports)
                    {
                        viewport.camera.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                        viewport.camera.fieldOfView = viewport.fovQuarterScreen;
                    }
                    foreach (ViewportEntry viewport in activePlayers[2].viewports)
                    {
                        viewport.camera.rect = new Rect(0f, 0f, 0.5f, 0.5f);
                        viewport.camera.fieldOfView = viewport.fovQuarterScreen;
                    }
                    foreach (ViewportEntry viewport in activePlayers[3].viewports)
                    {
                        viewport.camera.rect = new Rect(0.5f, 0f, 0.5f, 0.5f);
                        viewport.camera.fieldOfView = viewport.fovQuarterScreen;
                    }
                    break;
            }
        }

        
        
    }

    #region Classes

    public enum PlayerId { PlayerOne = 0, PlayerTwo = 1, PlayerThree = 2, PlayerFour = 3 }

    public class Player
    {
        public PlayerId id;
        public List<ViewportEntry> viewports;

        public Player(PlayerId playerId)
        {
            this.id = playerId;
            this.viewports = new List<ViewportEntry>();
        }
    }

    [Serializable]
    public class ViewportEntry
    {
        [HideInInspector]
        public Camera camera;
        public float fovFullScreen;
        public float fovHalfScreen;
        public float fovQuarterScreen;
    }

    #endregion
}

