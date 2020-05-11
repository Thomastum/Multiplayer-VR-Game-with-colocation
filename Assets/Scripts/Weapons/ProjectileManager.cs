using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetInteractionSystem;
using Photon.Pun;
using TMPro;
using UnityEngine.Events;

public class ProjectileManager : ShootManager
{
    public UnityEvent onProjectileCreated;
    public int baseDamage = 5;
    public int maxDamage = 12;
    public float baseSpeed = 30;
    public float maxSpeed = 50;
    public int baseKnockback = 4;
    public int maxKnockback = 10;
    public float timeToMaxCharge = 1f;
    public float defaultDestroyTime=3;
    private float speed;
    private int damage;
    private int knockback;

    protected override void Start()
    {
        base.Start();
        projectilePrefab = weaponManager.regularProjectile;
        // grapplingHookManager.onShoot.AddListener(DisableShooting);
        // grapplingHookManager.onHookReturned.AddListener(EnableShooting);
    }

    protected override void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
            CreateProjectile();
        if(Input.GetKey(KeyCode.E))
            PowerUp();
        if(Input.GetKeyUp(KeyCode.E))
            Shoot();
    }

    GameObject chargingProjectile;
    GameObject[] projectiles = new GameObject[30];
    void CreateProjectile()
    {
        if(weaponManager.EnergyBall!=null)
        {
            weaponManager.ConsumeEnergyBall();
            return;
        }

        if(!weaponManager.IsIdle)
            return;

        int projectileIndex=0;
        for(int i=0;i<projectiles.Length;i++)
        {
            if(projectiles[i]==null)
            {
                projectileIndex = i;
                break;
            } 
        }

        inAction=true;
        onProjectileCreated.Invoke();
        photonView.RPC("NetCreateProjectile",RpcTarget.All,projectileIndex);
    }

    [PunRPC]
    void NetCreateProjectile(int index)
    {
        chargingProjectile = Instantiate(projectilePrefab,shootPoint);
        projectiles[index] = chargingProjectile;
        chargingProjectile.GetComponent<Projectile>().Setup(this, index);
    }

    public float chargedAmount = 0;
    void PowerUp()
    {
        if(chargingProjectile==null)
            return;
        
        chargedAmount+=Time.deltaTime;
        damage = Mathf.RoundToInt(Mathf.Lerp(baseDamage,maxDamage,chargedAmount/timeToMaxCharge));
        speed = Mathf.Lerp(baseSpeed,maxSpeed,chargedAmount/timeToMaxCharge);
        knockback = Mathf.RoundToInt(Mathf.Lerp(baseKnockback,maxKnockback,chargedAmount/timeToMaxCharge));
    }

    void Shoot()
    {
        if(chargingProjectile==null)
            return;
        
        if(weaponManager.MultiplierActivated) 
            damage = weaponManager.MultiplyShotDamage(damage);
        //animator.SetTrigger("FireProjectile");
        inAction=false; 
        chargedAmount=0;
        photonView.RPC("NetShoot",RpcTarget.All, speed, damage, knockback);
    }
    

    [PunRPC]
    void NetShoot(float projectileSpeed, int damage, int knockback)
    {
        onShoot.Invoke();
        chargingProjectile.GetComponent<Projectile>().Fire(shootPoint.forward,projectileSpeed, damage, knockback);
        Destroy(chargingProjectile,defaultDestroyTime);
    }

    public override void ProjectileHit(Projectile projectile, RaycastHit hitInfo)
    {
        //only the master client can process the shot upon hit
        if(!photonView.IsMine)
            return;
        GameObject hitObject = hitInfo.collider.gameObject;
        string hitPoint = hitInfo.point.ToString();
        PhotonView hitObjectView = hitInfo.collider.gameObject.GetComponent<PhotonView>();
        
        IDamageable damageable = hitObject.gameObject.transform.root.GetComponent<IDamageable>();
        if(damageable !=null)
            damageable.GetHit(hitInfo,projectile.Damage,projectile.Knockback);
            
        IHittable hittable = hitObject.gameObject.GetComponent<IHittable>();
        if(hittable!=null)
            hittable.GetHit();

        photonView.RPC("NetProjectileHit",RpcTarget.All,projectile.Index, hitPoint);
    }

    [PunRPC]
    void NetProjectileHit(int index, string hitPoint)
    {
        if(hitParticles!=null)
        {
            Vector3 hitVector = HelpfulFunctions.StringToVector3(hitPoint);
            Instantiate(hitParticles,hitVector,Quaternion.identity);
        }
        Destroy(projectiles[index],.5f);
    }

    public override void EnableShooting()
    {
        base.EnableShooting();
        shootingHand.ActionButtonPress += CreateProjectile;
        shootingHand.ActionButtonHold += PowerUp;
        shootingHand.ActionButtonRelease += Shoot;
    }  

    public override void DisableShooting()
    {
        base.DisableShooting();
        shootingHand.ActionButtonPress -= CreateProjectile;
        shootingHand.ActionButtonHold -= PowerUp;
        shootingHand.ActionButtonRelease -= Shoot;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // grapplingHookManager.onShoot.RemoveListener(DisableShooting);
        // grapplingHookManager.onHookReturned.RemoveListener(EnableShooting);    
    }

    public void SetProjectile(GameObject projectile, ParticleSystem hitParticles)
    {
        projectilePrefab = projectile;
        this.hitParticles = hitParticles;
    }
}
