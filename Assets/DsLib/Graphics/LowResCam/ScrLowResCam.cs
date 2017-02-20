using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class ScrLowResCam : MonoBehaviour
{
    [Header("Base Settings")]
    RenderTexture targetTexture;
    public int renderWidth = 480;
    public int renderHeight = 270;
    public int GuiSortOrder;
    [HideInInspector]
    public Vector2 rectPos;
    [HideInInspector]
    public Rect textureRect;
    [HideInInspector]
    public int scaling;

    [Header("Orthographic Settings")]
    public bool SetAsOrthographic = false;
    public float pixelsPerUnit = 10f;

    void Start ()
    {
        targetTexture = new RenderTexture(renderWidth, renderHeight, 16);
        targetTexture.antiAliasing = 1;
        targetTexture.filterMode = FilterMode.Point;
        GetComponent<Camera>().targetTexture = targetTexture;

        DetermineScale();

        if (SetAsOrthographic)
            SetOrthographic();
    }

    void SetOrthographic()
    {
        Camera camera = GetComponent<Camera>();
        camera.orthographic = true;

        float renderSize = renderHeight;
        if (renderWidth < renderHeight )
            renderSize = renderWidth;

        camera.orthographicSize = renderSize / (pixelsPerUnit * 2);
    }

    int DetermineScale()
    {
        int scaleX = Screen.width / renderWidth;
        int scaleY = Screen.height / renderHeight;

        int scaleEffective = scaleX;
        if (scaleY < scaleX)
            scaleEffective = scaleY;

        return scaleEffective;
    }

    void OnGUI()
    {
        scaling = DetermineScale();
        int texWidth = renderWidth * scaling;
        int texHeight = renderHeight * scaling;
        rectPos = new Vector2(Mathf.FloorToInt((Screen.width - texWidth) / 2f), Mathf.FloorToInt((Screen.height - texHeight) / 2f));
        textureRect = new Rect(rectPos, new Vector2(texWidth, texHeight));

        GUI.depth = GuiSortOrder;
        GUI.DrawTexture(textureRect, targetTexture);
    }
}
