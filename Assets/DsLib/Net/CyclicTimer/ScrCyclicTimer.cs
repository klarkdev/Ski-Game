using UnityEngine;
using System.Collections;

public class ScrCyclicTimer : MonoBehaviour
{
    public DsLib.CyclicTimer Timer;

    public void Start()
    {
        Timer.Start();
    }

    public void Update()
    {
        Timer.Update(Time.deltaTime);
    }
}
