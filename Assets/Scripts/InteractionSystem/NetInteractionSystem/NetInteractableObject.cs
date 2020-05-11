using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Photon.Pun;

namespace NetInteractionSystem
{
[RequireComponent(typeof(PhotonView),typeof(Rigidbody))]

public class NetInteractableObject : MonoBehaviour, IPunObservable{

    protected PhotonView photonView;
    protected Rigidbody rb;
    protected bool interacting;
    private void Awake()
    {
    }

    public virtual void Start()
    {
        photonView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(!photonView.IsMine && PhotonNetwork.IsConnected)
            return;
        LocalUpdate();
    }

    void FixedUpdate()
    {
        if(!photonView.IsMine && PhotonNetwork.IsConnected)
            return;
        LocalFixedUpdate();
    }

    public virtual void LocalUpdate(){}
    public virtual void LocalFixedUpdate(){}

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        info.Sender.TagObject = this.gameObject;
    }

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(interacting);
        }
        if (stream.IsReading)
        {
            interacting = (bool)stream.ReceiveNext();
        }
    }
    
    public virtual void OnCollisionEnter(Collision other)
    {
        interacting = true;
    }

    public virtual void OnCollisionExit(Collision other)
    {
        interacting = false;
    }

    protected void FreezeObjectPosition() => rb.constraints = RigidbodyConstraints.FreezePosition;
    protected void FreezeObjectRotation() => rb.constraints = RigidbodyConstraints.FreezeRotation;
    protected void UnfreezeObjectPosition() => rb.constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezePosition;
    protected void UnfreezeObjectRotation() => rb.constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezeRotation;
    protected void UnfreezeObject() => rb.constraints = RigidbodyConstraints.None;
    protected void FreezeObject() => rb.constraints = RigidbodyConstraints.FreezeAll;
    


}
}
