using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private ShootManager gun;
    private Rigidbody rb;
    private TrailRenderer trailRenderer;
    private AudioSource audioSource;
    int index;
    public int Index{get{return index;}}
    int damage;
    public int Damage{get{return damage;}}
    int knockback;
    public int Knockback{get{return knockback;}}
    public float shootSpeed;
    private Vector3 shootDir;
    bool isFired = false;
    
    void Start()
    {
        rb=GetComponent<Rigidbody>();
        trailRenderer=GetComponent<TrailRenderer>();
        audioSource=GetComponent<AudioSource>();
        trailRenderer.enabled=false;
    }
    // Update is called once per frame
    void Update()
    {
        if(!isFired)
            return;
        
        transform.position += shootDir*shootSpeed*Time.deltaTime;
        Ray ray = new Ray(transform.position,transform.forward);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, shootSpeed*Time.deltaTime))
            gun.ProjectileHit(this,hit);
    }

    public void Setup(ShootManager gun, int index)
    {
        this.gun = gun;
        this.index = index;
    }

    public void Fire(Vector3 shootDirection, float shootSpeed, int damage, int knockback)
    {
        shootDir = shootDirection;
        this.damage = damage;
        this.knockback = knockback;
        this.shootSpeed = shootSpeed;

        transform.parent=null;
        isFired=true;
        trailRenderer.enabled=true;

        audioSource.pitch = Random.Range(0.7f,1f);
        audioSource.Play();
    }
}