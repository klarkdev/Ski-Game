using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DsLib
{
    [RequireComponent(typeof(ScrFpsWeaponPivot))]
    public class ScrFpsWeaponDefault : MonoBehaviour
    {
        ScrFpsController scrFpsController;
        ScrFpsWeaponPivot scrFpsWeaponPivot;

        Camera headCamera;
        float headCameraOrigFov;
        Camera weaponCamera;
        float weaponCameraOrigFov;

        [Header("Firing Settings")]
        public CyclicTimer fireTimer;
        public GameObject projectile;
        public int projectilesPerShot = 1;
        public Vector2 projectileDeviation = new Vector2(1f,1f);

        bool fireReleased = true;

        [Header("Magazine Settings")]
        public CyclicTimer reloadTimer;
        public int magazineMax = 1;
        public int magazineReloadAmount = 1;
        public int magazineCurrent;

        [Header("Movement Settings")]
        public float moveAimMultiplier = 0.55f;
        public float moveMultiplier = 1f;

        [Header("Shouldering Settings")]
        public float zoomLerp = 0.1f;
        public float zoomFovMult = 0.8f;
        
        [Header("Juice")]
        public Effects.SfxClip sfxFire;
        public Effects.VibrationClip vibFire;
        public Effects.ShakeClip shkFire;
        public Effects.SfxClip sfxCock;
        public Effects.SfxClip sfxEmpty;

        Effects.Source sfxSource;

        // Use this for initialization
        void Start()
        {
            scrFpsController = transform.root.GetComponent<ScrFpsController>();
            scrFpsWeaponPivot = transform.GetComponent<ScrFpsWeaponPivot>();
            sfxSource = transform.root.GetComponent<ScrEffectsListener>().personalEffects;

            headCamera = scrFpsController.headCamera.GetComponent<Camera>();
            headCameraOrigFov = headCamera.fieldOfView;

            weaponCamera = scrFpsController.weaponCamera.GetComponent<Camera>();
            weaponCameraOrigFov = weaponCamera.fieldOfView;

            fireTimer.onIterate += SpawnProjectiles;
            fireTimer.onCooldown += Cock;

            magazineCurrent = magazineMax;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (scrFpsController.getInput == null)
                return;

            FpsControllerInput input = scrFpsController.getInput();

            if (input.aim) // always shoulder weapon when aim is pressed
            {
                scrFpsWeaponPivot.shouldering = true;

                headCamera.fieldOfView = Mathf.Lerp(headCamera.fieldOfView, headCameraOrigFov * zoomFovMult, zoomLerp);
                weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, weaponCameraOrigFov * zoomFovMult, zoomLerp);

                scrFpsController.weaponMoveMult = moveAimMultiplier;
            }
            else
            {
                scrFpsWeaponPivot.shouldering = false;

                headCamera.fieldOfView = Mathf.Lerp(headCamera.fieldOfView, headCameraOrigFov, zoomLerp);
                weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, weaponCameraOrigFov, zoomLerp);

                scrFpsController.weaponMoveMult = moveMultiplier;
            }

            if (input.fire)
                Fire();
            else
            {
                fireReleased = true;
            }

            fireTimer.Update(Time.fixedDeltaTime);
            reloadTimer.Update(Time.fixedDeltaTime);
        }

        void Fire()
        {
            if (magazineCurrent > 0)
            {
                if (fireTimer.state == CyclicTimer.State.Idle)
                    fireTimer.Start();
            }
            else
            {
                if (fireReleased == true)
                    sfxEmpty.Play(sfxSource);
            }

            fireReleased = false;
        }

        void Cock()
        {
            sfxCock.Play(sfxSource);
        }

        void SpawnProjectiles()
        {
            if (magazineCurrent <= 0)
            {
                sfxEmpty.Play(sfxSource);
                return;
            }

            for (int i = projectilesPerShot; i > 0; i--)
            {
                Vector2 projectileDeviationTemp = new Vector2(
                    Random.Range(-projectileDeviation.x, projectileDeviation.x),
                    Random.Range(-projectileDeviation.y, projectileDeviation.y));

                GameObject.Instantiate(projectile, scrFpsController.headCamera.position, Quaternion.Euler(
                    scrFpsController.headCamera.rotation.eulerAngles.x + projectileDeviationTemp.y,
                    scrFpsController.headCamera.rotation.eulerAngles.y + projectileDeviationTemp.x,
                    scrFpsController.headCamera.rotation.eulerAngles.z));
            }

            sfxFire.Play(sfxSource);
            vibFire.Play(sfxSource);
            shkFire.Play(sfxSource);

            scrFpsWeaponPivot.Recoil();

            magazineCurrent--;
        }
    }
}

