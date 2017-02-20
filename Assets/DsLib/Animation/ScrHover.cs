using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DsLib
{
    public class ScrHover : MonoBehaviour
    {
        public Axis axis;
        public float intensity = 0.0625f;
        public float frequency = 2f;

        float timeCurrent = 0f;
        float positionCurrent = 0f;

        Vector3 originalPosition;

        void Start()
        {
            originalPosition = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            // vertical sinus hover
            positionCurrent = intensity * Mathf.Sin(timeCurrent);
            timeCurrent += Time.deltaTime * frequency;

            transform.position = axis.ChangeOnlyAxis(transform.position, (axis.GetAxisValue(originalPosition) + positionCurrent));
        }
    }
}

