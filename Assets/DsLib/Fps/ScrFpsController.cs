using UnityEngine;
using System;
using System.Collections;

namespace DsLib
{
    [Serializable]
    public struct FpsControllerInput
    {
        public float lookX;
        public float lookY;
        public float moveX;
        public float moveZ;
        public bool jump;
        public bool fire;
        public bool aim;
    }

    public delegate FpsControllerInput GetInput();

    [RequireComponent(typeof(ScrPlayer))]
    [RequireComponent(typeof(ScrEffectsListener))]
    public class ScrFpsController : MonoBehaviour
    {
        PlayerId playerId;

        [HideInInspector]
        public ScrEffectsListener scrListener;
        public GetInput getInput;
        public Transform headCamera;
        public Transform weaponCamera;
        [HideInInspector]
        public Rigidbody body;
        CapsuleCollider capsule;

        [Header("Effects")]
        public Effects.SfxClip sfxJump;
        public Effects.VibrationClip vibJump;
        public Effects.SfxClip sfxLand;
        public Effects.VibrationClip vibLand;

        [Header("Movement Settings")]
        public float moveSpeed = 7.5f;
        public float moveAccel = 25f;
        public float moveDamping = 0.85f;
        [Space]
        public float stepHeight = 0.3f;
        public float stepLerp = 0.5f;
        public float slopeMaxAngle = 35f;
        [Space]
        public float jumpSpeed = 8f;
        public float jumpFootExtensionLerp = 0.125f;
        public float gravity = 0.5f;
        public float minLandSfxSpeed = 1f;
        float pitchLimit = 85f;
        float jumpSpeedLandPercentage = 0.75f;

        [Header("Collider Settings")]
        public LayerMask levelGeometryLayerMask;
        public float mass = 80f;
        public float height = 1.8f;
        public float radius = 0.3f;

        [Header("Status")]
        public bool grounded = false;
        public SurfaceInfo groundInfo;
        float pitch = 0f;
        bool jumpReleased = true;
        Vector3 headCameraOriginalPos;
        public float weaponMoveMult = 1f;

        // Use this for initialization
        void Start()
        {
            scrListener = transform.root.GetComponent<ScrEffectsListener>();

            scrListener.onReceivedReadOut += ReceiveReadOut;

            if (headCamera != null)
                headCameraOriginalPos = headCamera.localPosition;
            else
                Debug.Log("Warning: You need to manually assign the head camera to a players ScrFpsController!");

            // set up physics
            body = gameObject.AddComponent<Rigidbody>();
            body.mass = mass;
            body.drag = 0f;
            body.angularDrag = 0f;
            body.useGravity = false;

            PhysicMaterial frictionlessMaterial = new PhysicMaterial();
            frictionlessMaterial.dynamicFriction = 0f;
            frictionlessMaterial.staticFriction = 0f;
            frictionlessMaterial.bounciness = 0f;
            frictionlessMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
            frictionlessMaterial.bounceCombine = PhysicMaterialCombine.Minimum;

            capsule = gameObject.AddComponent<CapsuleCollider>();
            capsule.radius = radius;  
            capsule.sharedMaterial = frictionlessMaterial;

            EnterGrounded();
        }

        void Update()
        {
            FpsControllerInput input = new FpsControllerInput();

            if (getInput != null)
            {
                input = getInput();

                // mouselook X
                transform.Rotate(Vector3.up, input.lookX);

                // mouselook Y
                pitch = Mathf.Clamp(pitch + input.lookY, -pitchLimit, pitchLimit);
                headCamera.localEulerAngles = Vector3.right * pitch;

                Cursor.lockState = CursorLockMode.Locked;
            }

        }

        void FixedUpdate()
        {
            groundInfo = GroundRaycast(transform.position);

            // determine move input
            FpsControllerInput input = new FpsControllerInput();

            if (getInput != null)
                input = getInput();

            // reset jump input
            if (grounded && input.jump == false)
                jumpReleased = true;

            // determine walking velocity to move toward
            Vector3 moveVelocityTarget = CalculateMoveVelocityTarget(input);

            // determine how to interact with ground (either stand on it, land on it, step up/down or fall)
            if (Mathf.Abs(groundInfo.distance) <= stepHeight && groundInfo.upwardAngle <= slopeMaxAngle && body.velocity.y <= jumpSpeed * jumpSpeedLandPercentage) // check if grounded
            {
                if (!grounded) 
                {
                    // land
                    if (body.velocity.y <= -minLandSfxSpeed)
                    {
                        sfxLand.Play(scrListener.personalEffects);
                        vibLand.Play(scrListener.personalEffects);
                    }
                    
                    EnterGrounded();
                }

                //step
                transform.position = Vector3.Lerp( 
                    transform.position,
                    transform.position + Vector3.down * groundInfo.distance,
                    stepLerp);
            }

            if (grounded)
            {
                // enter free fall if not touching any ground
                if (groundInfo.distance == Mathf.Infinity) 
                    EnterFreefall();

                // if touching steep slope, start falling if there is no level ground nearby
                else if (groundInfo.upwardAngle > slopeMaxAngle && groundInfo.upwardAngle < 90) 
                {
                    SurfaceInfo checkForFooting = GroundRaycast(transform.position + Math.OnlyHorizontal(groundInfo.normal).normalized * radius);

                    if (checkForFooting.distance == Mathf.Infinity || checkForFooting.upwardAngle > slopeMaxAngle)
                        EnterFreefall();
                }
            }

            if (!grounded)
            {
                // fall
                body.velocity = body.velocity + (Vector3.down * gravity); 

                // extend feet to avoid clipping into slopes when falling
                capsule.height = Mathf.Lerp(capsule.height, height, jumpFootExtensionLerp);
                capsule.center = Vector3.Lerp(
                    capsule.center,
                    new Vector3(0f, height / 2f, 0f),
                    jumpFootExtensionLerp);
            }

            // jump conditions are checked
            if (input.jump && jumpReleased)
                Jump();

            if (grounded || groundInfo.upwardAngle <= slopeMaxAngle) // no friction during sliding only when grounded or in air
            {
                if (Vector3.Dot(moveVelocityTarget, body.velocity) <= 0f) // if walking opposite direction or input 0 apply friction
                    body.velocity = Math.HorizontalOverride(body.velocity, body.velocity * moveDamping);

                if (moveVelocityTarget.sqrMagnitude != 0f) // if move input move
                    body.velocity = Math.HorizontalOverride(body.velocity, Vector3.MoveTowards(body.velocity, moveVelocityTarget, moveAccel * Time.fixedDeltaTime));
            }
        }

        void Jump()
        {
            if (!grounded)
                return;

            EnterFreefall();

            body.velocity = Math.VerticalOverride(body.velocity, jumpSpeed);

            sfxJump.Play(scrListener.personalEffects);
            vibJump.Play(scrListener.personalEffects);

            jumpReleased = false;

            return;
        }

        void EnterGrounded()
        {
            grounded = true;
            body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

            capsule.height = height - stepHeight;
            capsule.center = new Vector3(0f, stepHeight + ((height - stepHeight) / 2), 0f);

            body.velocity = Math.VerticalOverride(body.velocity, 0f);
        }

        void EnterFreefall()
        {
            grounded = false;
            body.constraints = RigidbodyConstraints.FreezeRotation;
        }

        SurfaceInfo GroundRaycast(Vector3 footPosition)
        {
            SurfaceInfo tempGroundInfo = SurfaceInfo.zero;

            float rayOriginHeight = height - radius;
            float rayDistance = rayOriginHeight + stepHeight;

            // cast ray to check for level ground, ray because spherecast might touch only adjacent slope instead
            RaycastHit groundCastHit;
            Physics.Raycast(
                footPosition + (Vector3.up * rayOriginHeight),
                Vector3.down,
                out groundCastHit,
                rayDistance,
                levelGeometryLayerMask,
                QueryTriggerInteraction.Ignore);

            if (groundCastHit.collider != null)
            {
                tempGroundInfo.normal = groundCastHit.normal;
                tempGroundInfo.upwardAngle = Vector3.Angle(groundCastHit.normal, Vector3.up);
                tempGroundInfo.distance = groundCastHit.distance - rayOriginHeight;
            }

            return tempGroundInfo;
        }

        public void ReceiveReadOut(DsLib.Effects.ReadOut readOut)
        {
            if (headCamera != null)
            {
                headCamera.localPosition = headCameraOriginalPos + readOut.shake.posOffset;
                headCamera.localEulerAngles = readOut.shake.rotOffset + Vector3.right * pitch;
            }
        }

        void OnDestroy()
        {
            scrListener.onReceivedReadOut -= ReceiveReadOut;
        }

        public float GetMoveSpeedPercent()
        {
            return Math.OnlyHorizontal(body.velocity).magnitude / moveSpeed;
        }

        Vector3 CalculateMoveVelocityTarget(FpsControllerInput input)
        {
            // input movement
            Vector3 moveDesired = new Vector3(input.moveX, 0, input.moveZ);

            // normalize input to prevent diagonal speed up
            if (moveDesired.sqrMagnitude > 1f)
                moveDesired = moveDesired.normalized;

            return transform.rotation * moveDesired * moveSpeed * weaponMoveMult;
        }

        public enum GroundState { Grounded, Freefall }
        [Serializable]
        public struct SurfaceInfo
        {
            public Vector3 normal;
            public float upwardAngle;
            public float distance;

            public SurfaceInfo(Vector3 normal, float upwardAngle, float distance)
            {
                this.normal = normal;
                this.upwardAngle = upwardAngle;
                this.distance = distance;
            }
            
            public static SurfaceInfo zero = new SurfaceInfo(Vector3.up, 0f, Mathf.Infinity);
        }
    }
}