using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Photon.Pun;

namespace NetInteractionSystem
{
public enum ToggleState{On, Off};

[RequireComponent(typeof(Rigidbody))]
public class NetPushableToggle : NetInteractableObject
{
    // Start is called before the first frame update
    public Transform offPoint;
    public Transform onPoint;
    public Transform contactPoint;
    public float resetSpeed = 5;
    public UnityEvent onActivate;
    public UnityEvent onDeactivate;
    public UnityEvent onReset;
    public TextMeshPro debugText;
    private Transform neutralPoint;
    private bool isOn= false;
    private bool correctCollision = false;
    private bool activated = false;
    private bool resetted = false;
    private PositionClamper clamper;

    IEnumerator resetCoroutine;


    public override void Start()
    {
        base.Start();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        neutralPoint = (isOn) ? onPoint : offPoint;
        clamper = new PositionClamper(transform,neutralPoint,contactPoint);
        debugText.text = "resetted";
    }

    // Update is called once per frame
    public override void LocalUpdate()
    {
        if(interacting)
        {
            transform.localPosition = clamper.GetClampedValue();
            if(transform.localPosition == contactPoint.localPosition)
                Activate();
        }
    }

    public override void LocalFixedUpdate()
    {
        if(interacting)
            rb.velocity = Vector3.zero;
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(interacting);
            stream.SendNext(activated);
            stream.SendNext(resetted);
            stream.SendNext(correctCollision);
        }
        if (stream.IsReading)
        {
            interacting = (bool)stream.ReceiveNext();
            activated = (bool)stream.ReceiveNext();
            resetted = (bool)stream.ReceiveNext();
            correctCollision = (bool)stream.ReceiveNext();
        }
    }


    bool IsCorrectCollisionDirection(Vector3 contactDirection)
    {
        Vector3 correctPushDirection = (contactPoint.position - offPoint.position);
        float t = Vector3.Dot(correctPushDirection.normalized,contactDirection.normalized);
        if(t>=1.0f) //facing the same direction. -1 if opposite directions, 0 if 90 degree angle (perpendicular)
            return true;
        return false;
    }

    void Activate()
    {
        if(activated)
            return;
        debugText.text = "activated";
        isOn = !isOn;
        if(isOn)
            onActivate.Invoke();
        neutralPoint = (isOn) ? onPoint : offPoint;

        activated = true;
        resetted=false;
    }

    void Reset()
    {
        rb.Sleep();

        if(resetted)
            return;
        debugText.text = "resetted";
        if(!isOn)
            onDeactivate.Invoke();

        activated = false;
        resetted = true;
    }

    public override void OnCollisionEnter(Collision other)
    {
        base.OnCollisionEnter(other);
        if(resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        if(other.gameObject.GetComponent<PhotonView>() != null)
        {
            if(other.gameObject.GetComponent<PhotonView>().IsMine && !photonView.IsMine)
                photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
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

       resetCoroutine = ResetPosition();
       StartCoroutine(resetCoroutine);
    }

    IEnumerator ResetPosition()
    {  
        Vector3 direction = (neutralPoint.position-transform.position).normalized;

        while(Vector3.SqrMagnitude(transform.localPosition-neutralPoint.localPosition) > 0.0001f)
        {
            rb.AddForceAtPosition(direction*resetSpeed*Time.deltaTime,neutralPoint.position);
            transform.localPosition = clamper.GetClampedValue();
            yield return null;
        }
        Reset();
    }

}
}
