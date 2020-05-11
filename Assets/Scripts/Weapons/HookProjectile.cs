using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class HitEvent: UnityEvent<Collider>
{

}
public class HookProjectile : MonoBehaviour
{
    public HitEvent OnCollision = new HitEvent();
    public UnityEvent maxDistanceReached;
    public AudioClip hitMissSound;
    public AudioClip hitSuccessSound;
    Transform shootPoint;
    [HideInInspector]
    public Transform catchPoint;
    private LineRenderer hookLine;
    private ParticleSystem hookParticles;
    private float speed;
    private float maxDistance;
    private bool isFired=false;
    private AudioSource audioSource;
    GrapplingHookManager gun;
    IEnumerator particlesCoroutine;
    
    void Awake()
    {
        shootPoint = transform.parent;
        transform.parent=null;
        hookLine = GetComponent<LineRenderer>();
        hookParticles = GetComponentInChildren<ParticleSystem>();
        audioSource=GetComponent<AudioSource>();
        catchPoint = transform.GetChild(1);

        particlesCoroutine = LerpParticles(true);
        StartCoroutine(particlesCoroutine);
    }
    // Update is called once per frame
    void Update()
    {
        hookLine.SetPosition(0,shootPoint.position);
        hookLine.SetPosition(1,transform.position);

        if(!isFired)
            return;

        transform.position += transform.forward*speed*Time.deltaTime;

        Ray ray = new Ray(transform.position,transform.forward);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit,speed*Time.deltaTime))
        {
            isFired=false;
            gun.GrapplingHookHit(hit);
        }
        
        float distanceFromGun = (shootPoint.position - transform.position).magnitude;
        if(distanceFromGun>=maxDistance)
        {
            maxDistanceReached.Invoke();
            isFired=false;
            gun.Retract();
        }
    }

    public void Ricochet(RaycastHit hitInfo,float ricochetTime)
    {
        Vector3 ricochetDir = Vector3.Reflect(transform.forward,hitInfo.normal).normalized;
        StartCoroutine(RicochetCoroutine(ricochetDir,ricochetTime));
    }

    IEnumerator RicochetCoroutine(Vector3 ricochetDir,float ricochetTime)
    {
        float timeElapsed=0;

        while(timeElapsed<ricochetTime)
        {
            timeElapsed+=Time.deltaTime;
            transform.position += ricochetDir*(speed/4)*Time.deltaTime;
            yield return null;
        }
        gun.Retract();
    }

    public void Fire(GrapplingHookManager gun, float speed, float maxDistance)
    {
        this.gun=gun;
        this.speed = speed;
        this.maxDistance = maxDistance;
        isFired=true;
    }

    public bool CaughtEnergyBall()
    {
        return transform.GetChild(1).childCount>0;
    }

    public void StopParticles()
    {
        StopCoroutine(particlesCoroutine);
        particlesCoroutine = LerpParticles(false);
        StartCoroutine(particlesCoroutine);
    }

    IEnumerator LerpParticles(bool increase)
    {
        var velocity = hookParticles.velocityOverLifetime;
        float velocityBase = velocity.radial.constant;
        var main = hookParticles.main;
        float baseSpeed = main.startSpeed.constant;

        float timeToStop=.1f;
        float elapsedTime=0;
        while(elapsedTime<timeToStop)
        {
            elapsedTime+=Time.deltaTime;
            if(increase)
            {
               // main.startSpeed = Mathf.Lerp(0,baseSpeed,elapsedTime/timeToStop);
                velocity.speedModifier = Mathf.Lerp(0,1,elapsedTime/timeToStop);
            }
            else
            {
                main.startSpeed = Mathf.Lerp(baseSpeed,0,elapsedTime/timeToStop);
                velocity.radial = Mathf.Lerp(1,0,elapsedTime/timeToStop);
            }
            yield return null;
        }
    }

    public void PlayHitMissSound()
    {
        audioSource.clip=hitMissSound;
        audioSource.Play();
    }

    public void PlayHitSuccessSound()
    {
        audioSource.clip=hitSuccessSound;
        audioSource.Play();
    }
}
