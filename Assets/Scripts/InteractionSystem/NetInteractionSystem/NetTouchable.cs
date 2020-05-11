using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

namespace NetInteractionSystem
{
public class NetTouchable : NetInteractableObject, IHittable
{
    public UnityEvent onInteract;

    public override void Start()
    {
        base.Start();
        rb.isKinematic = true;
    }
   public override void OnCollisionEnter(Collision other)
   {
        if(photonView.IsMine && PhotonNetwork.IsConnected)
            onInteract.Invoke();
   }

    public void GetHit()
    {
        onInteract.Invoke();
    }
}
}
