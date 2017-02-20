using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class ScrLowResCamCanvasCursor : MonoBehaviour
{
    public ScrLowResCam lowResCanvasCam;
    bool hideHardwareCursor = true;
    RectTransform rectTransform;

	void Start ()
    {
        rectTransform = transform.GetComponent<RectTransform>();
        Cursor.visible = !hideHardwareCursor;
    }

	void Update ()
    {
        rectTransform.anchoredPosition = GetLowResCamMousePos();
    }

    Vector2 GetLowResCamMousePos()
    {
        Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        if ((mousePosition.x <= lowResCanvasCam.textureRect.x + lowResCanvasCam.textureRect.width) &&
            (mousePosition.y <= lowResCanvasCam.textureRect.y + lowResCanvasCam.textureRect.height + rectTransform.sizeDelta.y))
            return (mousePosition - new Vector2(lowResCanvasCam.textureRect.x, lowResCanvasCam.textureRect.y)) / lowResCanvasCam.scaling;
        else
            return mousePosition;
    }
}
