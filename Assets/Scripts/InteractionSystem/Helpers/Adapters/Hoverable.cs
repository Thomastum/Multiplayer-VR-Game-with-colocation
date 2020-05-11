using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Hoverable : IHoverable
{
    public UnityEvent onHoverStart;
    public UnityEvent onHoverStay;
    public UnityEvent onHoverStop;


    public void StartHover()
    {
        onHoverStart.Invoke();
        //particleInstance = Instantiate(hoverParticles,transform);
    }

    public void StayHover()
    {
        onHoverStay.Invoke();
    }

    public void StopHover()
    {
        onHoverStop.Invoke();
        // var emission = particleInstance.emission;
        // emission.enabled = false;
        // var main = particleInstance.main;
        // Destroy(particleInstance.gameObject, main.startLifetime.constant);
    }
}
