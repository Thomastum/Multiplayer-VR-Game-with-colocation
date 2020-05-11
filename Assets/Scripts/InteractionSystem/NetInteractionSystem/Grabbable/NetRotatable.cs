using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using TMPro;

namespace NetInteractionSystem
{

[RequireComponent(typeof(Rigidbody))]
public class NetRotatable : NetGrabbableObject, IPunObservable
{
    // Start is called before the first frame update
    public Axis rotationAxis;
    public float minAngle;
    public float maxAngle;
    public bool hideHandOnGrabbed=true;
    public TextMeshPro debugText;

    public UnityEvent onMinAngleReached;
    public UnityEvent onMaxAngleReached;
    [System.Serializable]
    public class OnValueChanged : UnityEvent<float> { };
    public OnValueChanged onValueChanged;

    private Vector3 rotateDirection;
    private Vector3 startingPos;
    private bool clampAngleReached;
    private float amountRotated;
    private float fixedRotationValue;
    //private bool correctCollision = false;
    private RotationClamper clamper;

    public override void Start()
    {
        base.Start();
        rb.constraints = RigidbodyConstraints.FreezePosition;
        clamper = new RotationClamper(transform,minAngle,maxAngle,rotationAxis);
        startingPos = transform.localPosition;
    }

    public override void LocalUpdate()
    {
        if(interacting || isGrabbed)
        {
            transform.localPosition = startingPos;
            transform.localEulerAngles = clamper.GetClampedValue();
            CheckForEvents();
        }
        else
        {
            if(Input.GetKey(KeyCode.RightArrow))
                transform.localEulerAngles += new Vector3(0,0,0.5f);
            if(Input.GetKey(KeyCode.LeftArrow))
                transform.localEulerAngles += new Vector3(0,0,-0.5f);
            transform.localEulerAngles = clamper.GetClampedValue();
            CheckForEvents();
        }
    }

    public override void LocalFixedUpdate()
    {
        if(interacting) // if interacting via normal physics, just pushing in
            rb.angularVelocity = Vector3.zero;
        else if(isGrabbed)
            rb.angularVelocity = grabbedBy.GetComponent<NetHandController>().GetAngularVelocity();
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
        if(hideHandOnGrabbed)
            hand.GetComponent<NetHandController>().HideModel(true);

        interacting = false;
        UnfreezeObjectRotation();
        photonView.RPC("GetGrabbedNet", RpcTarget.All, hand.GetComponent<NetHandController>().handModel.gameObject.GetPhotonView().ViewID);
    }

    public override void GetReleased(Vector3 releaseVelocity, Vector3 releaseAngularVelocity)
    {
        base.GetReleased(releaseVelocity,releaseAngularVelocity);
        if(hideHandOnGrabbed)
            grabbedBy.GetComponent<NetHandController>().HideModel(false);
        rb.angularVelocity = Vector3.zero;

        photonView.RPC("GetReleasedNet", RpcTarget.All);
    }

    [PunRPC]
    protected override void GetGrabbedNet(int viewID)
    {
        GameObject hand = PhotonView.Find(viewID).gameObject;
        isGrabbed = true;
        grabbedBy = hand.transform.parent.gameObject;
    }

    [PunRPC]
    protected override void GetReleasedNet()
    {
        isGrabbed = false;
        grabbedBy = null;
    }

    // Update is called once per frame

    float FixRotation(float angle)
    {
        if(angle > 180f)
            angle = angle-360f;
        
        return angle;
    }

    float prevAmountRotated;
    void CheckForEvents()
    {
        fixedRotationValue = FixRotation(transform.localEulerAngles[(int)rotationAxis]);
        amountRotated = Mathf.Lerp(0,1,(fixedRotationValue-minAngle)/(maxAngle-minAngle));

        if(amountRotated == 0 && !clampAngleReached)
        {
            onMinAngleReached.Invoke();
            clampAngleReached = true;
        }
        else if(amountRotated == 1 && !clampAngleReached)
        {
            onMaxAngleReached.Invoke();
            clampAngleReached = true;
        }
        else if(amountRotated != 0 && amountRotated != 1)
            clampAngleReached = false;

        if(amountRotated!=prevAmountRotated)
            onValueChanged.Invoke(amountRotated);
        
        prevAmountRotated = amountRotated;
    }

    public override void OnCollisionEnter(Collision other)
    {
        base.OnCollisionEnter(other);

        if(isGrabbed)
        {
            FreezeObjectRotation();
            return;
        }
    }

    public override void OnCollisionExit(Collision other)
    {
       base.OnCollisionExit(other);
       UnfreezeObjectRotation();
    }
}
}
