using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using TMPro;

namespace NetInteractionSystem
{

[RequireComponent(typeof(Rigidbody))]
public class NetSlidable : NetGrabbableObject, IPunObservable
{
    // Start is called before the first frame update
    public Transform pointA;
    public Transform pointB;
    public bool hideHandOnGrabbed=true;
    public TextMeshPro debugText;

    public UnityEvent onPointAReached;
    public UnityEvent onPointBReached;
    [System.Serializable]
    public class OnValueChanged : UnityEvent<float> { };
    public OnValueChanged onValueChanged;

    private Vector3 slideDirection;
    private bool clampPointReached;
    private bool correctCollision = false;
    private PositionClamper clamper;

    public override void Start()
    {
        base.Start();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        clamper = new PositionClamper(transform,pointA,pointB);
    }

    // Update is called once per frame
    public override void LocalUpdate()
    {
        if(interacting || isGrabbed)
        {
            transform.localPosition = clamper.GetClampedValue();
            CheckForEvents();
        }
    }

    public override void LocalFixedUpdate()
    {
        if(interacting) // if interacting via normal physics, just pushing in
            rb.velocity = Vector3.zero;
        else if(isGrabbed)
            rb.velocity = grabbedBy.GetComponent<NetHandController>().GetVelocity();
    }

    public override bool CanBeGrabbed()
    {
        if(grabbedBy == null)
            return true;
        return false;
    }

    public override void GetGrabbed(GameObject hand)
    {
        base.GetGrabbed(hand);
        grabbedBy = hand;
        interacting = false;
        UnfreezeObjectPosition();

        if(hideHandOnGrabbed)
            hand.GetComponent<NetHandController>().HideModel(true);
    }

    public override void GetReleased(Vector3 releaseVelocity, Vector3 releaseAngularVelocity)
    {
        base.GetReleased(releaseVelocity,releaseAngularVelocity);
        rb.velocity = Vector3.zero;

        if(hideHandOnGrabbed)
            grabbedBy.GetComponent<NetHandController>().HideModel(false);
        grabbedBy = null;
    }

    float prevAmountMoved;
    void CheckForEvents()
    {
        float amountSlided = clamper.GetAmountMoved();
        if(amountSlided == 0 && !clampPointReached)
        {
            onPointAReached.Invoke();
            clampPointReached = true;
        }
        else if(amountSlided == 1 && !clampPointReached)
        {
            onPointBReached.Invoke();
            clampPointReached = true;
        }
        else if(amountSlided != 0 && amountSlided != 1)
            clampPointReached = false;

        if(amountSlided!=prevAmountMoved)
            onValueChanged.Invoke(amountSlided);
        
        prevAmountMoved = amountSlided;
    }
    
    bool IsCorrectCollisionDirection(Vector3 contactDirection)
    {
        Vector3 correctPushDirection = (pointA.position - pointB.position); 
        float t = Vector3.Dot(correctPushDirection.normalized,contactDirection.normalized);
        
        if(t<=-1.0f || t>=1.0f) //facing the same direction. -1 if opposite directions, 0 if 90 degree angle (perpendicular)
            return true;

        return false;
    } 

    public override void OnCollisionEnter(Collision other)
    {
        base.OnCollisionEnter(other);

        if(isGrabbed)
        {
            FreezeObject();
            return;
        }
        Vector3 contactDirection = other.contacts[0].normal;
        correctCollision = IsCorrectCollisionDirection(contactDirection);

        if(!correctCollision)
            FreezeObject();
    }

    public override void OnCollisionExit(Collision other)
    {
        base.OnCollisionExit(other);
        UnfreezeObjectPosition();
    }
}
}