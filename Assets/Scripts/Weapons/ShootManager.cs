using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetInteractionSystem;
using Photon.Pun;
using UnityEngine.Events;

[RequireComponent(typeof(NetEquipable))]
public class ShootManager : MonoBehaviour
{
    protected Transform shootPoint;
    public GameObject projectilePrefab;
    public ParticleSystem hitParticles;
    public UnityEvent onShoot;
    public UnityEvent onHit;
    NetEquipable netEquipable;
    protected PhotonView photonView;
    protected NetHandController shootingHand;
    protected Animator animator;
    protected WeaponManager weaponManager;
    protected ProjectileManager projectileManager;
    protected GrapplingHookManager grapplingHookManager;

    protected bool inAction=false;
    public bool InAction{get{return inAction;}}

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        netEquipable = GetComponent<NetEquipable>();
        photonView = GetComponent<PhotonView>();
        weaponManager = GetComponent<WeaponManager>();
            shootPoint = weaponManager.shootPoint;
        projectileManager = GetComponent<ProjectileManager>();
        grapplingHookManager = GetComponent<GrapplingHookManager>();
        netEquipable.onGrabbed.AddListener(EnableShooting);
        netEquipable.onReleased.AddListener(DisableShooting);
    }

    protected virtual void Update()
    {

    }

    public virtual void ProjectileHit(Projectile projectile, RaycastHit hit)
    {

    }
    
    public virtual void GrapplingHookHit(RaycastHit hitInfo)
    {

    }

    public virtual void EnableShooting()
    {
        shootingHand = netEquipable.GetObjectHolder().GetComponent<NetHandController>();
    }  

    public virtual void DisableShooting()
    {
        shootingHand = netEquipable.GetObjectHolder().GetComponent<NetHandController>();
    }

    protected virtual void OnDestroy()
    {
        netEquipable.onGrabbed.RemoveListener(EnableShooting);
        netEquipable.onReleased.RemoveListener(DisableShooting);
    }

}
