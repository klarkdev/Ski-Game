using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UnityEngine.UI.Text))]
public class ScrLowResCamCanvasCursorInfo : MonoBehaviour {

    UnityEngine.UI.Text text;
    public ScrLowResCam lowResCam;

    void Start ()
    {
        text = GetComponent<UnityEngine.UI.Text>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        text.text = Screen.width + " " + Screen.height + "\n" +
            Input.mousePosition.ToString() + "\n" +
            lowResCam.textureRect.ToString();
	}
}
