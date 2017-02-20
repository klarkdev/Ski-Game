using UnityEngine;
using System.Collections;

namespace DsLib
{
    public class ScrBillboard : MonoBehaviour
    {
        public bool flipBackwards = true;

        void OnWillRenderObject()
        {
            if (!flipBackwards)
                transform.rotation = Quaternion.LookRotation(
                    new Vector3(Camera.current.transform.position.x, 0, Camera.current.transform.position.z) -
                    new Vector3(transform.position.x, 0, transform.position.z));
            else
            {
                Vector3 lookRotation = new Vector3(transform.position.x, 0, transform.position.z) -
                    new Vector3(Camera.current.transform.position.x, 0, Camera.current.transform.position.z);
                if (lookRotation == Vector3.zero)
                    transform.rotation = Quaternion.Euler(0f,0f,0f);
                else
                    transform.rotation = Quaternion.LookRotation(lookRotation);
            }
        }
    }
}
