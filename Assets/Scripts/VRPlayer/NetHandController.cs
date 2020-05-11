using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using NetInteractionSystem;

public enum HandType{None,PrimaryHand,OffHand};
public enum Hand{Left,Right};

[RequireComponent(typeof(PhotonView))]
public class NetHandController : MonoBehaviour {

    public event Action ActionButtonPress = delegate{};
    public event Action ActionButtonHold = delegate{};
    public event Action ActionButtonRelease = delegate{};
    public event Action GrapplingHookShoot = delegate{};
    public event Action GrapplingHookRelease = delegate{};
    public OVRInput.Controller controller;
    public OVRInput.Button grabButton;
    public OVRInput.Button actionButton;
    public OVRInput.Button grapplingHookButton;
    public Hand hand;
    [HideInInspector]
    public NetHandController otherHand;
    [HideInInspector]
    public NetSolidHands handModel;
    
    private GameObject player;
    private HandType handType=HandType.None;
    private PhotonView photonView;
    private GameObject objectBeingTouched;
    private GameObject objectBeingHeld;

    void Start()
    {   
        handModel = GetComponentInChildren<NetSolidHands>();
        handModel.otherHand = otherHand.GetComponentInChildren<NetSolidHands>();
        player = transform.parent.gameObject;
        photonView = GetComponent<PhotonView>();

        handModel.onCollisionEnter += OnHandCollisionEnter;
        handModel.onCollisionExit += OnHandCollisionExit;
    }

    void Update()
    {
        if(!photonView.IsMine && PhotonNetwork.IsConnected)
            return;
        // if (settingsPanel.GetStateDown(pose.inputSource))
        //     ManagePlayerPanel();
        if(handType == HandType.None)
        {
            if(OVRInput.GetDown(actionButton))
                GrabObject();
            else if(OVRInput.GetUp(actionButton))
                ReleaseObject();
        }

        else if(handType == HandType.PrimaryHand)
        {
            if(OVRInput.GetDown(actionButton))
                ActionButtonPress();
            else if(OVRInput.Get(actionButton))
                ActionButtonHold();
            else if(OVRInput.GetUp(actionButton))
                ActionButtonRelease();
            
            else if(OVRInput.GetDown(grapplingHookButton))
                GrapplingHookShoot();
            else if(OVRInput.GetUp(grapplingHookButton))
                GrapplingHookRelease();
        }

        else if(handType == HandType.OffHand)
        {
            if(OVRInput.GetDown(actionButton))
                ActionButtonPress();
            else if(OVRInput.Get(actionButton))
                ActionButtonHold();
            else if(OVRInput.GetUp(actionButton))
                ActionButtonRelease();
        }

    }

    void GrabObject()
    {
        //if grabbing object that is currently being touched by hand
        GameObject obj = objectBeingTouched;
        if(obj == null)
            obj = handModel.closestObject;

        if(obj != null && obj.GetComponent<IGrabbable>().CanBeGrabbed())
        {
            objectBeingHeld = obj;
            photonView.RPC("GrabObjectNet",RpcTarget.All,objectBeingHeld.GetPhotonView().ViewID);
            objectBeingHeld.GetComponent<IGrabbable>().GetGrabbed(this.gameObject);

            if(obj.CompareTag("PrimaryWeapon"))
                handType = HandType.PrimaryHand;
            else if(obj.CompareTag("SecondaryWeapon"))
                handType = HandType.OffHand;
        }
    }

    public void ReleaseObject()
    {
        if (objectBeingHeld == null)
            return;
        objectBeingHeld.GetComponent<IGrabbable>().GetReleased(GetVelocity(), GetAngularVelocity());
        objectBeingHeld = null;
        photonView.RPC("ReleaseObjectNet",RpcTarget.All);
    }

    // Vector3 CalculateFuturePos(int secondsInAdvance)
    // {
    //     Vector3 finalPos = transform.position;
    //     Vector3 velocity = GetVelocity();

    //     velocity*=secondsInAdvance;
    //     finalPos += velocity;
    //     return finalPos;
    // }

    [PunRPC]
    void GrabObjectNet(int objectViewID)
    {
         objectBeingHeld = PhotonView.Find(objectViewID).gameObject;
    }

    [PunRPC]
    void ReleaseObjectNet()
    {
        objectBeingHeld = null;
    }

    private void OnHandCollisionEnter(GameObject other)
    {
        objectBeingTouched = other;
    }

    private void OnHandCollisionExit(GameObject other)
    {
        objectBeingTouched = null;
    }

    public void HideModel(bool hidden)
    {
        handModel.SetHandActive(!hidden);
        if(!hidden) // this is used for hand switching
            objectBeingHeld = null;
    }

    public GameObject GetHeldObject() {return objectBeingHeld;}
    public Vector3 GetVelocity() {return OVRInput.GetLocalControllerVelocity(controller);}
    public Vector3 GetAngularVelocity() {return OVRInput.GetLocalControllerAngularVelocity(controller);}

    void OnDisable()
    {
        handModel.onCollisionEnter -= OnHandCollisionEnter;
        handModel.onCollisionExit -= OnHandCollisionExit;
    }
}
