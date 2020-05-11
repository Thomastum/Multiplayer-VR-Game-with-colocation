using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Jacovone;

public class FlyDroneAIPath : EnemyAI
{
    public float velocityDecelerationRate = 0.1f;
    bool offBalance=false;
    IEnumerator moveCoroutine;
    IEnumerator decelerateCoroutine;
    Vector3 recoverPosition;
    Quaternion recoverRotation;
    FlyPath path;

    protected override void Start()
    {
        base.Start();
    }

    public void SetPath(FlyPath path)
    {
        this.path = path;
    }

    // Update is called once per frame
    protected override void LocalUpdate()
    {
        if(dead)
            return;

        if(offBalance && decelerateCoroutine==null)
        {
            decelerateCoroutine=Decelerate();
            StartCoroutine(decelerateCoroutine);
        }
    }
    protected override void LocalFixedUpdate()
    {
        Vector3 lookDir = transform.position-portal.position;
        Quaternion targetRotation = Quaternion.identity;
        if(lookDir!=Vector3.zero)
            targetRotation = Quaternion.LookRotation(lookDir,Vector3.up);
        rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime*lookResetSpeed);
    }

    protected override void HandleCollisionWithEnvironment(string nameOfObstacle)
    {
        
        if(moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine=null;
        }

        if(decelerateCoroutine != null)
        {
            StopCoroutine(decelerateCoroutine);
            decelerateCoroutine=null;
        }
        
        //PeTaK - comment this - this cause unpredicted movement of Fly 
        //Invoke("SetOffBalance",1f);
    }

    void SetOffBalance()
    {
        offBalance=true;
    }

    IEnumerator MoveToTarget(Vector3 targetPos, Quaternion targetRot)
    {
        Vector3 moveDir = targetPos-transform.position;
        Quaternion startRot = transform.rotation;
        float elapsedTime=0f;
        float timeToLerp=0.3f;
        while((targetPos-transform.position).sqrMagnitude>0.05f)
        {
            rb.AddForce(moveDir*moveSpeed*Time.deltaTime,ForceMode.Impulse);
            elapsedTime+=Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot,targetRot,elapsedTime/timeToLerp);
            yield return null;
        }
        targetPos = Vector3.zero;
        while(rb.velocity != Vector3.zero)
        {
            rb.velocity = rb.velocity * (1-velocityDecelerationRate);
            yield return null;
        }

        
        //PeTaK
        StartCoroutine(PathResume());
        //path.Resume();
    }

    private IEnumerator PathResume()
    {
        yield return new WaitForSeconds(1);
        path.Resume();
    }

    IEnumerator Decelerate()
    {
        yield return new WaitForFixedUpdate();
        while(rb.velocity != Vector3.zero || rb.angularVelocity != Vector3.zero)
        {
            rb.velocity = rb.velocity * (1-velocityDecelerationRate);
            rb.angularVelocity = rb.angularVelocity * (1-velocityDecelerationRate);
            yield return null;
        }
        offBalance=false;
        moveCoroutine = MoveToTarget(recoverPosition,recoverRotation);
        StartCoroutine(moveCoroutine);
    }


    [PunRPC]
    protected override void NetGetHit(string bodyPartHit, string hitVector, int damage, int knockbackForce)
    {
        path.Pause();
        recoverPosition = transform.position;
        recoverRotation = transform.rotation;
        base.NetGetHit(bodyPartHit,hitVector,damage,knockbackForce);
        
        if(moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine=null;
        }

        if(decelerateCoroutine != null)
        {
            StopCoroutine(decelerateCoroutine);
            decelerateCoroutine=null;
        }
        offBalance=true;
    }

    protected override void Die()
    {
        if (path != null)
        {
            path.Destroy();
        }

        base.Die();
    }
}
