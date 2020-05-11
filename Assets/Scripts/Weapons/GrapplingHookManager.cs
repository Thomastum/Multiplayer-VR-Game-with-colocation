using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetInteractionSystem;
using Photon.Pun;
using UnityEngine.Events;

public class GrapplingHookManager : ShootManager
{
    public UnityEvent onMiss;
    public ParticleSystem hookMissParticles;
    public GameObject energyBallPrefab;
    public float maxDistance=10;
    public float hookSpeed = 15;
    public float retractSpeed = 2;
    public float ricochetTime=1f;
    public AudioClip shotSound;
    public UnityEvent onHookReturned;
    private GameObject energyBall;
    private Transform energyBallPos;
    GameObject hook;
    IEnumerator retractCoroutine;

    protected override void Start()
    {
        base.Start();
        energyBallPos = weaponManager.energyBallPosition;
        // projectileManager.onShoot.AddListener(EnableShooting);
        // projectileManager.onProjectileCreated.AddListener(DisableShooting);
    }

    protected override void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))
            ShootButtonClicked();
        if(Input.GetKeyUp(KeyCode.H))
            Retract();
    }

    void ShootButtonClicked()
    {
        if(!weaponManager.IsIdle || hook!=null)
            return;

       // animator.SetTrigger("HookShot");
        onShoot.Invoke();
        inAction=true;
        photonView.RPC("NetShoot",RpcTarget.All);
    }

    [PunRPC]
    void NetShoot()
    {
        weaponManager.PlaySound(shotSound);
        hook = Instantiate(projectilePrefab,shootPoint);
        hook.GetComponent<HookProjectile>().Fire(this, hookSpeed, maxDistance);
    }

    public void Retract()
    {
        if(!photonView.IsMine)
            return;
        if(hook==null || retractCoroutine!=null)
            return;
        
        if(!hook.GetComponent<HookProjectile>().CaughtEnergyBall())
        {
            //animator.SetTrigger("HookMiss");
            energyBall=null;
        }

        photonView.RPC("NetRetract",RpcTarget.All);
    }

    [PunRPC]
    void NetRetract()
    {
        retractCoroutine = RetractHook();
        StartCoroutine(retractCoroutine);
        hook.GetComponent<HookProjectile>().StopParticles();
    }

    IEnumerator RetractHook()
    {
        Vector3 startPos = hook.transform.position;
        float distanceTraveled=0;
        while(distanceTraveled<1)
        {
            distanceTraveled += Time.deltaTime*retractSpeed;
            hook.transform.position = Vector3.Lerp(startPos, shootPoint.position, distanceTraveled);
            yield return null;
        }
        retractCoroutine=null;

        if(photonView.IsMine)
            HookReturned();
    }

    public void HookReturned()
    {
        onHookReturned.Invoke(); 
        if(energyBall!=null)
        {
            energyBall.GetComponent<EnergyBall>().LoadOntoWeapon(this,energyBallPos);
            weaponManager.SetEnergyBall(energyBall);
        }
        inAction=false;   
        photonView.RPC("NetHookReturned",RpcTarget.All);
    }

    [PunRPC]
    void NetHookReturned()
    {
        if(retractCoroutine!=null)
        {
            StopCoroutine(retractCoroutine);
            retractCoroutine=null;
        }
        Destroy(hook);
    }

    public override void GrapplingHookHit(RaycastHit hitInfo)
    {
        if(!photonView.IsMine)
            return;
        
        onHit.Invoke();
        // PhotonView hitObjectView = hitInfo.collider.gameObject.GetComponent<PhotonView>();
        // if(photonView.IsMine && hitObjectView !=null)
        // {
        //     bool canTransfer = hitInfo.collider.gameObject.transform.root.GetComponent<VRPlayerManager>() == null;
        //     if(hitObjectView.Owner != PhotonNetwork.LocalPlayer && canTransfer)
        //         hitObjectView.TransferOwnership(PhotonNetwork.LocalPlayer);
        IDamageable damageable = hitInfo.collider.gameObject.transform.root.GetComponent<IDamageable>();
        if(damageable!=null)
        {
            damageable.GetHookHit(hook.transform, hitInfo);
            if(damageable.IsEyeHit(hitInfo.collider))
            {
                energyBall = PhotonNetwork.Instantiate(energyBallPrefab.name,Vector3.zero,Quaternion.identity);
                energyBall.GetComponent<EnergyBall>().LoadOntoWeapon(this, hook.transform.GetChild(1));
                energyBall.GetComponent<EnergyBall>().SetEnergyLevel(damageable.GetHealth());
                photonView.RPC("NetHookHitSuccess",RpcTarget.All);
                Retract();
            }
            else
            {
                hook.GetComponent<HookProjectile>().Ricochet(hitInfo,ricochetTime);
                photonView.RPC("NetHookMissHit",RpcTarget.All);
            }
        }
        else
        {
            hook.GetComponent<HookProjectile>().Ricochet(hitInfo,ricochetTime);
            photonView.RPC("NetHookMissHit",RpcTarget.All);
        }
       // }
    }

    [PunRPC]
    void NetHookHitSuccess()
    {
        hook.GetComponent<HookProjectile>().PlayHitSuccessSound();
        if(hitParticles!=null)
        {
           ParticleSystem successParticles = Instantiate(hitParticles,hook.transform);
           successParticles.transform.parent=null;
        }
    }


    [PunRPC]
    void NetHookMissHit()
    {
        hook.GetComponent<HookProjectile>().PlayHitMissSound();
        if(hookMissParticles!=null)
        {
           ParticleSystem missPartiles = Instantiate(hookMissParticles,hook.transform);
           missPartiles.transform.parent=null;
        }
    }

    public override void EnableShooting()
    {
        base.EnableShooting();
        shootingHand.GrapplingHookShoot += ShootButtonClicked;
        //shootingHand.GrapplingHookRelease += Retract;
    }  

    public override void DisableShooting()
    {
        base.EnableShooting();
        shootingHand.GrapplingHookShoot -= ShootButtonClicked;
        //shootingHand.GrapplingHookRelease -= Retract;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // projectileManager.onShoot.RemoveListener(EnableShooting);
        // projectileManager.onProjectileCreated.RemoveListener(DisableShooting);
    }

}
