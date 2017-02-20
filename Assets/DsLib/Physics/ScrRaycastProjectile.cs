using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DsLib
{
    public class ScrRaycastProjectile : MonoBehaviour
    {
        public float speed = 1f;
        public float lifeTime = 30f;
        public float impactForce = 20f;
        public LayerMask collisionLayers;

        public GameObject impactObject;
        public Vector3 impactObjectScale;

        float lifeTimeCurrent;

        void Start()
        {
            lifeTimeCurrent = lifeTime;
        }

        void FixedUpdate()
        {
            if (lifeTimeCurrent <= 0f)
            {
                Disable();
                return;
            }

            RaycastHit hitInfo;

            if (Physics.Raycast(transform.position, transform.rotation * Vector3.forward, out hitInfo, speed * Time.fixedDeltaTime, collisionLayers))
            {
                GameObject impactInstance = GameObject.Instantiate(impactObject, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                impactInstance.transform.parent = hitInfo.transform;

                if (hitInfo.rigidbody != null)
                    hitInfo.rigidbody.AddForceAtPosition(transform.rotation * Vector3.forward * impactForce, hitInfo.point);

                Disable();
                return;
            }
            else
            {
                if (speed == Mathf.Infinity)
                {
                    Disable();
                    return;
                }
                    
                transform.position += transform.rotation * Vector3.forward * speed * Time.fixedDeltaTime;
                lifeTimeCurrent -= Time.fixedDeltaTime;
            }
        }

        public void Disable()
        {
            lifeTimeCurrent = lifeTime;
            this.Recycle();
        }
    }
}

