using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using TMPro;

namespace NetInteractionSystem{
[RequireComponent(typeof(Rigidbody))]
public class NetPickable : NetInteractableObject, IHoverable, IGrabbable, IPunObservable{

    public bool canSwitchHands = true;
    public bool isThrowable = true;
    public bool hideHandOnPickup = true;
    public float resetSpeed = 5;
    protected TextMeshPro debugMessage;

    public override void Start()
    {
        base.Start();
        debugMessage = GetComponentInChildren<TextMeshPro>();
    }

    public override void LocalFixedUpdate()
    {
        if (isGrabbed)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if(!interacting)
            {
                Vector3 direction = transform.parent.position - transform.position;
                float distanceFromOrigin = direction.sqrMagnitude;
                if(distanceFromOrigin > .02f)
                    rb.AddForceAtPosition(direction.normalized*resetSpeed, Vector3.zero);
            }
        }

    }

    #region Hoverable
    public ParticleSystem hoverParticles;
    public UnityEvent onHoverStart;
    public UnityEvent onHoverStay;
    public UnityEvent onHoverStop;
    private ParticleSystem particleInstance;
    public void StartHover()
    {
        onHoverStart.Invoke();
        if(hoverParticles ==  null)
            return;
        particleInstance = Instantiate(hoverParticles,transform);
    }

    public void StayHover()
    {
        onHoverStay.Invoke();
    }

    public void StopHover()
    {
        onHoverStop.Invoke();
        if(hoverParticles ==  null)
            return;
        var emission = particleInstance.emission;
        emission.enabled = false;
        var main = particleInstance.main;
        Destroy(particleInstance.gameObject, main.startLifetime.constant);
    }

    #endregion

    #region Grabbable
    public UnityEvent onGrabbed;
    public UnityEvent onReleased;
    protected bool isGrabbed=false;
    public bool IsGrabbed() {return isGrabbed;}
    protected GameObject grabbedBy=null;

    public bool CanBeGrabbed()
    {
        if(!isGrabbed)
            return true;
        if(isGrabbed && canSwitchHands)
            return true;
        return false;
    }

    public void GetGrabbed(GameObject hand)
    {   
        onGrabbed.Invoke();
        if(photonView.Owner != PhotonNetwork.LocalPlayer)
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        
        photonView.RPC("GetGrabbedNet", RpcTarget.All, hand.GetPhotonView().ViewID);
    }

    [PunRPC]
    public void GetGrabbedNet(int viewID)
    {
        GameObject hand = PhotonView.Find(viewID).gameObject;
        GameObject handModel = hand.GetComponent<NetHandController>().handModel.gameObject;

        if(hideHandOnPickup)
        {
            if(grabbedBy != null)
                grabbedBy.GetComponent<NetHandController>().HideModel(false);
            hand.GetComponent<NetHandController>().HideModel(true);
        }

        grabbedBy = hand;
        transform.parent = handModel.transform;
        rb.useGravity = false;
        isGrabbed = true;
    }

    public void GetReleased(Vector3 releaseVelocity, Vector3 releaseAngularVelocity)
    {
        onReleased.Invoke();
        if (isThrowable)
        {
            rb.velocity = releaseVelocity;
            rb.angularVelocity = releaseAngularVelocity;
        }

        if(hideHandOnPickup)
            grabbedBy.GetComponent<NetHandController>().HideModel(false);
        
        photonView.RPC("GetReleasedNet", RpcTarget.All);
    }

    [PunRPC]
    public void GetReleasedNet()
    {
        transform.parent = null;
        rb.useGravity = true;
        isGrabbed = false;
        grabbedBy = null;
    }
    #endregion

}
}
