using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using System;

namespace NetInteractionSystem
{
public class NetSolidHands : MonoBehaviour, IPunObservable
{
    public event Action<GameObject> onCollisionEnter= delegate{};
    public event Action<GameObject> onCollisionExit= delegate{};
    public event Action<GameObject> onHoverEnter = delegate{};
    public event Action<GameObject> onHoverStay = delegate{};
    public event Action<GameObject> onHoverExit = delegate{};

    public float maxGrabDistance;
    public float resetSpeed = 5;
    public LineRenderer debugLine;
    
    Rigidbody rb;
    Vector3 originalPosition;
    Quaternion originalRotation;
    PhotonView photonView;
    [HideInInspector]
    public NetSolidHands otherHand;
    private bool interacting;
    private bool isActive = true;
    //public bool IsVisible {get {return isVisible;} set {isVisible = value;}}
    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    void Update()
    {
        if(!photonView.IsMine && PhotonNetwork.IsConnected)
            return;
        if(debugLine!=null)
        {
            debugLine.SetPosition(0,transform.position);
            if(closestObject!=null)
                debugLine.SetPosition(1,closestObject.transform.position);
            else
                debugLine.SetPosition(1,transform.position);
        }

        if(isActive)
            GetClosestGrabbableObject();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if(!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        rb.velocity = Vector3.zero;
        if(!interacting)
        {
            if(!IsAtOrigin())
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.fixedDeltaTime*resetSpeed);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRotation, Time.fixedDeltaTime*resetSpeed);
            }
        }
    }

    bool IsAtOrigin()
    {
        return (transform.localPosition == originalPosition) && (transform.localRotation == originalRotation);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //nothing for now
        }
        if (stream.IsReading)
        {
            //nothing for now
        }
    }

    public void SetHandActive(bool state)
    {
        isActive = state;
        interacting = false;
        photonView.RPC("SetHandVisibleNet",RpcTarget.AllBuffered,state);

        if(!isActive && closestObject!=null) // if started to hold object
        {
            closestObject.GetComponent<IHoverable>().StopHover();
            closestObject=null;
            prevClosestObject=null;
        }
    }

    [PunRPC]
    void SetHandVisibleNet(bool state)
    {
        GetComponent<MeshRenderer>().enabled=state;
        GetComponent<Collider>().enabled = state;
    }

    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.GetComponent<NetInteractableObject>())
            onCollisionEnter(other.gameObject);
        interacting = true;
    }

    void OnCollisionExit(Collision other)
    {
        if(other.gameObject.GetComponent<NetInteractableObject>())
            onCollisionExit(other.gameObject);
        interacting = false;   
    }

    [HideInInspector]
    public GameObject closestObject=null;
    GameObject prevClosestObject = null;
    void GetClosestGrabbableObject()
    {
        Collider[] hitObjects = Physics.OverlapSphere(transform.position,maxGrabDistance);
        closestObject = null;
        float minDistanceToHand = float.MaxValue;

        for(int i=0;i<hitObjects.Length;i++)
        {
            if(hitObjects[i].GetComponent<IHoverable>() == null)
                continue;
            if(!hitObjects[i].GetComponent<IGrabbable>().CanBeGrabbed())
                continue;
            float distanceToHand = Vector3.SqrMagnitude(hitObjects[i].transform.position - transform.position);
            if(distanceToHand < minDistanceToHand)
            {
                minDistanceToHand = distanceToHand;
                closestObject = hitObjects[i].gameObject;
            }
        }

        if(closestObject == null)
        {   
            if(prevClosestObject !=null && prevClosestObject != otherHand.closestObject)
                prevClosestObject.GetComponent<IHoverable>().StopHover();
            prevClosestObject = null;
            return;
        }

        if(closestObject != prevClosestObject)
        {
            if(prevClosestObject !=null && prevClosestObject != otherHand.closestObject)
                prevClosestObject.GetComponent<IHoverable>().StopHover();

            if(closestObject != otherHand.closestObject)
                closestObject.GetComponent<IHoverable>().StartHover();
        }
        else
        {
            if(closestObject != otherHand.closestObject)
                closestObject.GetComponent<IHoverable>().StayHover();
        }

        prevClosestObject = closestObject;
    }

}
}

