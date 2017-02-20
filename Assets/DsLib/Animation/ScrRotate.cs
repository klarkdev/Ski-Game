using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DsLib
{
    public class ScrRotate : MonoBehaviour
    {
        public Axis axis;
        public float speed = 30f;

        void Update()
        {
            transform.Rotate(axis.ToVector(), speed * Time.deltaTime);
        }
    }
}