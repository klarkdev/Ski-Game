using UnityEngine;
using System.Collections;

namespace DsLib
{
    [RequireComponent(typeof(ScrFpsController))]
    public class ScrFpsControllerInputKeyboardMouse : MonoBehaviour
    {
        public float mouseSensitivity = 1f;
        public bool mouseInverted = false;

        void Start()
        {
            GetComponent<ScrFpsController>().getInput = ProvideInput;
        }

        public FpsControllerInput ProvideInput()
        {
            FpsControllerInput input = new FpsControllerInput();

            input.lookX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            input.lookY = -Input.GetAxisRaw("Mouse Y") * mouseSensitivity * DsLib.Math.BoolToSign(!mouseInverted);

            input.moveX = Input.GetAxisRaw("Horizontal");
            input.moveZ = Input.GetAxisRaw("Vertical");

            input.jump = Input.GetKey(KeyCode.Space);
            input.fire = Input.GetKey(KeyCode.Mouse0);
            input.aim = Input.GetKey(KeyCode.Mouse1);

            return input;
        }
    }
}
