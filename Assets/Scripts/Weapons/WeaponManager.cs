using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.Events;

public class WeaponManager : MonoBehaviour, IPunObservable
{
    public Transform shootPoint;
    public Transform energyBallPosition;
    public float multiplierValue = 1.5f;
    public float maxMultiplierEnergy=200;
    public float energyUsedPerProjectile=5;
    public float energyBallPullRadius=0.2f;
    public GameObject bonusIndicator;
    //public GameObject energyPlaceHolder;
    private Material indicatorMaterial;

    public GameObject regularProjectile;
    public GameObject bonusProjectile;
    public ParticleSystem regularHitParticles;
    public ParticleSystem bonusHitParticles;

    private PhotonView photonView;
    private ProjectileManager projectileManager;
    private GrapplingHookManager grapplingHookManager;
    [HideInInspector]
    public AudioSource audioSource;
    private Animator animator;
    private GameObject energyBall;
    public GameObject EnergyBall{get{return energyBall;}}
    private float multiplierEnergy=0;
    private bool multiplierActivated;
    public bool MultiplierActivated{get{return multiplierActivated;}}
    private bool isIdle;
    public bool IsIdle{get{return isIdle;}}
    
    void Start()
    {
        indicatorMaterial = bonusIndicator.GetComponent<Renderer>().material;
        projectileManager = GetComponent<ProjectileManager>();
        grapplingHookManager = GetComponent<GrapplingHookManager>();
        audioSource=GetComponent<AudioSource>();
        photonView = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();

        GameManager.instance.onGameOver.AddListener(ResetWeapon);
        
        photonView.RPC("NetSetRegularProjectile",RpcTarget.All);
    }

    void Update()
    {   
        float energyNormalized = Mathf.Lerp(0,1,multiplierEnergy/maxMultiplierEnergy);
        indicatorMaterial.SetFloat("Vector1_3F9CCCCF",energyNormalized);
        
        if(energyBallPosition.childCount==0)
            energyBall=null;
        
        if(!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        isIdle = CheckIfIdle();
        // if(isIdle)
        //     TryPullEnergyBall();
    }

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(multiplierEnergy);
        }
        if (stream.IsReading)
        {
            multiplierEnergy = (float)stream.ReceiveNext();
        }
    }

    bool CheckIfIdle()
    {
        return !projectileManager.InAction && !grapplingHookManager.InAction && energyBall==null;
    }


    public int MultiplyShotDamage(int damage)
    {
        multiplierEnergy-=energyUsedPerProjectile;
        if(multiplierEnergy<=0)
        {
            multiplierActivated=false;
            photonView.RPC("NetSetRegularProjectile",RpcTarget.All);
        }  
        return Mathf.RoundToInt(damage*multiplierValue);
    }

    // void TryPullEnergyBall()
    // {
    //     Collider[] hitObjects = Physics.OverlapSphere(energyBallPosition.position,energyBallPullRadius);
    //     bool placeholderEnabled = false;
    //     for(int i=0;i<hitObjects.Length;i++)
    //     {
    //         GameObject hitObject = hitObjects[i].gameObject;
    //         if(hitObject.GetComponent<EnergyBall>()!=null)
    //         {
    //             placeholderEnabled=true;
    //             if(hitObject.transform.parent==null)
    //             {
    //                 placeholderEnabled = false;
    //                 hitObject.GetComponent<EnergyBall>().LoadOntoWeapon(grapplingHookManager,energyBallPosition);
    //                 energyBall = hitObject;
    //                 break;
    //             }
    //         }
    //     }
    //     energyPlaceHolder.SetActive(placeholderEnabled);
    //     //animator.SetBool("EnergyBallClose",placeholderEnabled);
    // }

    public void SetEnergyBall(GameObject energyBall)
    {
        this.energyBall = energyBall;
    }

    public void ConsumeEnergyBall()
    {
        //animator.SetTrigger("ConsumeEnergy");

        multiplierEnergy = Mathf.Lerp(0,maxMultiplierEnergy,energyBall.GetComponent<EnergyBall>().GetEnergyLevel());
        PhotonNetwork.Destroy(energyBall);
        multiplierActivated=true;
        photonView.RPC("NetSetBonusProjectile",RpcTarget.All);
    }

    [PunRPC]
    void NetSetRegularProjectile()
    {
        projectileManager.SetProjectile(regularProjectile,regularHitParticles);
    }

    [PunRPC]
    void NetSetBonusProjectile()
    {
        projectileManager.SetProjectile(bonusProjectile,bonusHitParticles);
    }

    void ResetWeapon()
    {
        multiplierActivated = false;
        multiplierEnergy = 0;
        photonView.RPC("NetSetRegularProjectile",RpcTarget.All);
        if(energyBall!=null)
            PhotonNetwork.Destroy(energyBall);       
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    void OnDestroy()
    {
        if(GameManager.instance!=null)
        GameManager.instance.onGameOver.RemoveListener(ResetWeapon);
    }
}
