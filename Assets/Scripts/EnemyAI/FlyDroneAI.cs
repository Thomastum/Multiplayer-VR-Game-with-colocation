using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;

public class FlyDroneAI : EnemyAI
{
    public float flyBurstDistance = 1.5f;
    public float flyBurstRate = 1;
    public float velocityDecelerationRate = 0.1f;
    private Vector3 targetPos=Vector3.zero;
    bool offBalance=false;
    IEnumerator moveCoroutine;
    IEnumerator decelerateCoroutine;
    float flyLoadTime = 0;

    
    [Header("Fly Init Position")]
    [SerializeField] private bool isReadyToFly = false;
    [SerializeField] private Transform enemyFlyReadyPosition;
    private float speedToReadyFly = 10;
    private string sceneName;
    private IEnumerator moveToInitPosition;
    
    
    private void Awake()
    {
        isReadyToFly = false;
        
        
    }

    protected override void Start()
    {
        base.Start();
        sceneName = SceneManager.GetActiveScene().name;
        

        if (sceneName == "WorkBench 1")
        {
            isReadyToFly = false;
            enemyFlyReadyPosition = GameObject.FindWithTag("FlyStartPosition").transform;
            InitFlyPosition();
        }
        else
        {
            isReadyToFly = true;
        }

    }

    // Update is called once per frame
    protected override void LocalUpdate()
    {
        
        if (isReadyToFly == false)
            return;
        
        if(dead)
            return;

        if(targetPos==Vector3.zero && !offBalance)
        {
            flyLoadTime += Time.deltaTime;
            if(flyLoadTime >= flyBurstRate)
            {
                targetPos = CalculateTargetPos();
                moveCoroutine = MoveToTarget();
                StartCoroutine(moveCoroutine);
            }
        }

        if(offBalance && decelerateCoroutine==null)
        {
            decelerateCoroutine = Decelerate();
            StartCoroutine(decelerateCoroutine);
        }
    }

    private void InitFlyPosition()
    {
        Debug.Log("Init Fly");
        moveToInitPosition = MoveToStartFlyPosition();
        StartCoroutine(moveToInitPosition);
    }
    
    IEnumerator MoveToStartFlyPosition()
    {
        Vector3 moveDir = enemyFlyReadyPosition.position-transform.position;
        while((enemyFlyReadyPosition.position-transform.position).sqrMagnitude>0.05f)
        {
            rb.AddForce(moveDir*speedToReadyFly*Time.deltaTime,ForceMode.Force);
            yield return null;
        }

        moveCoroutine=null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("FlyStartPosition"))
        {
            Debug.Log("Trigger Hit");
            rb.velocity = Vector3.zero;
            isReadyToFly = true;
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

    float distanceToTargetPortal = 5f;
    Vector3 CalculateTargetPos()
    {
        float distanceToPortal = (portal.position - transform.position).magnitude;
        if(distanceToPortal<=distanceToTargetPortal)
            return portal.position;


        Vector3 targetPosition = transform.position + -transform.forward * flyBurstDistance;
        targetPosition = targetPosition + Random.insideUnitSphere;
        return targetPosition;
    }

    IEnumerator MoveToTarget()
    {
        Vector3 moveDir = targetPos-transform.position;
        while((targetPos-transform.position).sqrMagnitude>0.05f)
        {
            rb.AddForce(moveDir*moveSpeed*Time.deltaTime,ForceMode.Impulse);
            yield return null;
        }
        targetPos = Vector3.zero;
        flyLoadTime=0;
        while(rb.velocity != Vector3.zero)
        {
            rb.velocity = rb.velocity * (1-velocityDecelerationRate);
            yield return null;
        }
        moveCoroutine=null;
    }

    IEnumerator Decelerate()
    {
        yield return new WaitForFixedUpdate();
        flyLoadTime=0;
        while(rb.velocity != Vector3.zero || rb.angularVelocity != Vector3.zero)
        {
            flyLoadTime+=Time.deltaTime;
            rb.velocity = rb.velocity * (1-velocityDecelerationRate);
            rb.angularVelocity = rb.angularVelocity * (1-velocityDecelerationRate);
            yield return null;
        }
        offBalance=false;
        decelerateCoroutine=null;
    }

    [PunRPC]
    protected override void NetGetHit(string bodyPartHit, string hitVector, int damage, int knockbackForce)
    {
        base.NetGetHit(bodyPartHit,hitVector,damage,knockbackForce);

        if(moveCoroutine !=null)
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
        targetPos = Vector3.zero;
        flyLoadTime=0;

    }
}
