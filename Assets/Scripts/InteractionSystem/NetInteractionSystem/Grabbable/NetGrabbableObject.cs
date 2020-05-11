using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Photon.Pun;

namespace NetInteractionSystem
{

public class NetGrabbableObject : NetInteractableObject{

    protected bool isGrabbed=false;
    public bool IsGrabbed {get{return isGrabbed;}}
    protected GameObject grabbedBy=null;
    protected bool parentToHandOnGrab=false;

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView(stream,info);
        
        if (stream.IsWriting)
        {
            stream.SendNext(isGrabbed);
        }
        if (stream.IsReading)
        {
            isGrabbed = (bool)stream.ReceiveNext();
        }
    }

    public virtual bool CanBeGrabbed()
    {
        return false;
    }
    
    //local information about object getting grabbed and released
    public virtual void GetGrabbed(GameObject hand)
    {
         if(photonView.Owner != PhotonNetwork.LocalPlayer)
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        
         photonView.RPC("GetGrabbedNet", RpcTarget.All, hand.GetPhotonView().ViewID);
    }

    public virtual void GetReleased(Vector3 releaseVelocity, Vector3 releaseAngularVelocity)
    {
        photonView.RPC("GetReleasedNet", RpcTarget.All);
    }

    //Neccessary information that is sent to other clients (gravity, isgrabbed)
    [PunRPC]
    protected virtual void GetGrabbedNet(int viewID)
    {
        GameObject hand = PhotonView.Find(viewID).gameObject;
        grabbedBy = hand;
        isGrabbed = true;
    }

    [PunRPC]
    protected virtual void GetReleasedNet()
    {
        isGrabbed = false;
        grabbedBy = null;
    }

}
}
