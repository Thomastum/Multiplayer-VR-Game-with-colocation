using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;
using TMPro;

namespace NetInteractionSystem{
[RequireComponent(typeof(Rigidbody))]
public class NetEquipable : NetInteractableObject, IGrabbable, IHoverable,IPunObservable{
    public bool hideHandOnPickup = true;
    public Vector3 positionInHand;
    public Vector3 rotationInHand;
    public float resetSpeed = 5;
    protected TextMeshPro debugMessage;

    public override void Start()
    {
        base.Start();
        debugMessage = GetComponentInChildren<TextMeshPro>();
    }

    public override void LocalUpdate()
    {
        if (isGrabbed && !interacting)
        {
            if(!IsAtOrigin())
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, positionInHand, Time.fixedDeltaTime*resetSpeed);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(rotationInHand), Time.fixedDeltaTime*resetSpeed);
            }
        }
    }

    public override void LocalFixedUpdate()
    {
        if (isGrabbed)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    bool IsAtOrigin()
    {
        return (transform.localPosition == positionInHand) && (transform.localRotation == Quaternion.Euler(rotationInHand));
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

    public GameObject GetObjectHolder()
    {
        return grabbedBy;
    }

    public bool CanBeGrabbed()
    {
        if(!isGrabbed)
            return true;
        return false;
    }

    public void GetGrabbed(GameObject hand)
    {
        if(photonView.Owner != PhotonNetwork.LocalPlayer)
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        
        photonView.RPC("GetGrabbedNet", RpcTarget.All, hand.GetPhotonView().ViewID);

        onGrabbed.Invoke();
        FreezeObjectPosition();
        interacting = false;

        if(hideHandOnPickup)
            hand.GetComponent<NetHandController>().HideModel(true);
    }

    [PunRPC]
    public void GetGrabbedNet(int viewID)
    {
        GameObject hand = PhotonView.Find(viewID).gameObject;
        GameObject handModel = hand.GetComponent<NetHandController>().handModel.gameObject;
        if(hand.GetComponent<NetHandController>().hand == Hand.Left && gameObject.CompareTag("SecondaryWeapon"))
            transform.localScale = new Vector3(-transform.localScale.x,transform.localScale.y,transform.localScale.z);
            
        grabbedBy = hand;
        transform.parent = handModel.transform;
        rb.useGravity = false;
        isGrabbed = true;
    }
    
    public void GetReleased(Vector3 releaseVelocity, Vector3 releaseAngularVelocity)
    {
        onReleased.Invoke();
        UnfreezeObject();
        
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
