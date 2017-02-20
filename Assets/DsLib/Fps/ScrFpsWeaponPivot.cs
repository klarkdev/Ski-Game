using UnityEngine;
using System.Collections;

namespace DsLib
{
    public class ScrFpsWeaponPivot : MonoBehaviour
    {
        ScrFpsController scrFpsController;

        Quaternion originalRotation;
        Quaternion currentRotation;
        Vector3 originalPosition;
        Vector3 currentPosition;

        [Header("Recoil Settings")]
        public float recoilUpwardMagnitude = 1.5f;
        public float recoilBackwardMagnitude = 0.02f;
        [Space]
        public float recoilStiffness = 0.3f;
        public float recoilDamping = 0.65f;
        
        

        public vp_Spring recoilUpwardSpring;
        public vp_Spring recoilBackwardSpring; 

        [Header("Shouldering Settings")]
        public Vector3 shoulderingPosition;
        public float shoulderMovementIntensityMult = 0.7f;
        public float shoulderLerp = 0.2f;
        public float unshoulderLerp = 0.125f;
        [HideInInspector]
        public bool shouldering = false;

        [Header("Look Inertia Settings")]
        public Vector2 lookInertiaMagnitude = new Vector2(0.2f, 0.35f);
        public float lookInertiaStiffness = 0.75f;
        public float lookInertiaDamping = 0.1f;
        [Space]
        public int lookInertiaDistributeFrames = 8;

        public vp_Spring lookInertiaSpring;

        [Header("Move Inertia Settings")]
        public Vector3 moveInertiaMagnitude = new Vector3(0.02f, 0.0125f, 0.04f);
        public Vector2 moveInertiaStiffness = new Vector2(0.75f, 0.2f);
        public Vector2 moveInertiaDamping = new Vector2(0.1f, 0.6f);
        [Space]
        public float moveBankMagnitude = 0.75f;

        public vp_Spring moveInertiaSpring;

        [Header("Movement Bob Settings")]
        
        public float movementBobIntensity = 0.075f;
        public float movementBobMinFrequency = 4f;
        public float movementBobMaxFrequency = 10f; 

        float movementBobTime = 0f;

        void Start()
        {
            scrFpsController = transform.root.GetComponent<ScrFpsController>();

            if (scrFpsController == null)
                Debug.LogError("ScrFpsWeaponPivot needs ScrFpsController at the root transform!");

            // Save original Rotation
            originalRotation = transform.localRotation;
            currentRotation = originalRotation;
            originalPosition = transform.localPosition;
            currentPosition = originalPosition;

            SetupSprings();
        }
        
        void SetupSprings()
        {
            // Set Up Recoil Upward Spring
            recoilUpwardSpring = new vp_Spring(
                Vector3.one * recoilStiffness,
                Vector3.one * recoilDamping,
                Vector3.one * -90f,
                Vector3.one * 90f);

            // Set Up Recoil Backward Spring
            recoilBackwardSpring = new vp_Spring(
                Vector3.one * recoilStiffness,
                Vector3.one * recoilDamping,
                -Vector3.one,
                Vector3.one);

            // Set Up Look Inertia Spring
            lookInertiaSpring = new vp_Spring(
                Vector3.one * lookInertiaStiffness,
                Vector3.one * lookInertiaDamping,
                Vector3.one * -90f,
                Vector3.one * 90f);

            // Set Up Move Inertia Spring
            moveInertiaSpring = new vp_Spring(
                new Vector3(moveInertiaStiffness.x, moveInertiaStiffness.y, moveInertiaStiffness.x),
                new Vector3(moveInertiaDamping.x, moveInertiaDamping.y, moveInertiaDamping.x),
                -Vector3.one,
                Vector3.one);
        }

        void FixedUpdate()
        {
            // get input
            if (scrFpsController.getInput == null)
                return;

            FpsControllerInput input = scrFpsController.getInput();

            // lerp toward shouldered or unshouldered position
            float intensityMultiplier = 1f;

            if (shouldering)
            {
                currentRotation = Quaternion.Lerp(currentRotation, Quaternion.Euler(0f, 0f, 0f), shoulderLerp);
                currentPosition = Vector3.Lerp(currentPosition, shoulderingPosition, shoulderLerp);

                if (scrFpsController.grounded) // only have steady weapon if shouldering and grounded
                    intensityMultiplier = shoulderMovementIntensityMult;
            }
            else
            {
                currentRotation = Quaternion.Lerp(currentRotation, originalRotation, unshoulderLerp);
                currentPosition = Vector3.Lerp(currentPosition, originalPosition, unshoulderLerp);
            }
            
            // movement bob or vertical move inertia
            float movementBob = 0f;

            if (scrFpsController.grounded)
            {
                // vertical movement Bob
                movementBob = movementBobIntensity * intensityMultiplier * scrFpsController.GetMoveSpeedPercent() * Mathf.Sin(movementBobTime);
                movementBobTime += Time.fixedDeltaTime * Mathf.Lerp(movementBobMinFrequency, movementBobMaxFrequency, scrFpsController.GetMoveSpeedPercent());
            }
            else
            {
                movementBobTime = 0f;
            
                // jump inertia
                moveInertiaSpring.AddForce(Vector3.down * -scrFpsController.body.velocity.y * moveInertiaMagnitude.y * intensityMultiplier * Time.fixedDeltaTime);
            }

            // look inertia
            lookInertiaSpring.AddSoftForce(new Vector3(
                input.lookY * -lookInertiaMagnitude.y * intensityMultiplier, // Look Vertical
                input.lookX * -lookInertiaMagnitude.x * intensityMultiplier, // Look Horizontal
                input.moveX * -moveBankMagnitude * intensityMultiplier), // Movement Bank
                lookInertiaDistributeFrames);

            Vector3 rotatedVelocity = transform.InverseTransformDirection(scrFpsController.body.velocity);

            // move inertia
            moveInertiaSpring.AddForce(
                new Vector3(
                    -rotatedVelocity.x * moveInertiaMagnitude.x * Math.BoolToFloat(!(shouldering && scrFpsController.grounded)),
                    movementBob,
                    -rotatedVelocity.z * moveInertiaMagnitude.z * intensityMultiplier)
                * Time.fixedDeltaTime);

            // apply spring transformations
            transform.localRotation = currentRotation * Quaternion.Euler(lookInertiaSpring.UpdateState()) * Quaternion.Euler(recoilUpwardSpring.UpdateState());
            transform.localPosition = currentPosition + moveInertiaSpring.UpdateState() + recoilBackwardSpring.UpdateState();
        }

        public void Recoil()
        {
            recoilUpwardSpring.AddForce(Vector3.left * recoilUpwardMagnitude);
            recoilBackwardSpring.AddForce(Vector3.back * recoilBackwardMagnitude);
        }
    }
}
