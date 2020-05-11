using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class CrawlerAI : EnemyAI
{
    public float jumpForce = 20;
    public float jumpAngle = 40;
    public float preparationTime=1;
    public AudioClip adjustingSound;
    IEnumerator preparationCoroutine;

    protected override void Start()
    {
        base.Start();
        knockbackEnabled=false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }
    public void AttackPortal()
    {
        preparationCoroutine = PrepareForJump();
        StartCoroutine(preparationCoroutine);
        photonView.RPC("NetPrepareForJump",RpcTarget.All);
    }
    public void Jump()
    {
        float gravity = Physics.gravity.magnitude;
        float angle = jumpAngle*Mathf.Deg2Rad;

        Vector3 tp = portal.position;
        Vector3 planarTarget = new Vector3(tp.x, 0, tp.z);
        Vector3 planarPosition = new Vector3(transform.position.x, 0, transform.position.z);

        float distance = (planarTarget-planarPosition).magnitude;
        float yOffset = transform.position.y - tp.y;

        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));
        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));
        float angleBetweenObjects = Vector3.SignedAngle(Vector3.forward,planarTarget-planarPosition,Vector3.up);
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        photonView.RPC("NetJump",RpcTarget.All);
        rb.AddForce(finalVelocity, ForceMode.Impulse);
    }

    [PunRPC]
    void NetJump()
    {
        rb.constraints = ~RigidbodyConstraints.FreezeAll;
        rb.useGravity=true;
        knockbackEnabled=true;
    }

    IEnumerator PrepareForJump()
    {
        float timeElapsed=0;
        while(timeElapsed<preparationTime)
        {
            timeElapsed+=Time.deltaTime;
            Vector3 lookDir = transform.position-portal.position;
            Quaternion targetRotation = Quaternion.identity;
            if(lookDir!=Vector3.zero)
                targetRotation = Quaternion.LookRotation(lookDir,Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime*lookResetSpeed);
            yield return null;
        }
        Jump();
    }

    [PunRPC]
    void NetPrepareForJump()
    {
        audioSource.clip = adjustingSound;
        audioSource.loop=false;
        audioSource.Play();
    }

    protected override void Die()
    {
        rb.constraints = ~RigidbodyConstraints.FreezeAll;
        knockbackEnabled=true;
        if(preparationCoroutine !=null)
        {
            StopCoroutine(preparationCoroutine);
            preparationCoroutine=null;
        }
        base.Die();
    }
    
}
