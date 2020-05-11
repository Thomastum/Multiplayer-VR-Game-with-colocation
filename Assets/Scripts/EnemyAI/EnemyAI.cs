using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
public class EnemyAI : MonoBehaviour, IDamageable, IPunObservable
{
    public float moveSpeed = 8;
    public float lookResetSpeed = 4;
    public Collider eye;
    public float criticalHitMultiplier = 2f;
    public int startHealth = 20;
    protected int health = 20;
    public TextMeshPro debugText;
    public ParticleSystem deathParticlesPrefab;
    public AudioClip hitSound;
    public AudioClip eyeHitSound;
    public AudioClip deathSound;
    protected PhotonView photonView;
    protected Rigidbody rb;
    protected Transform portal;
    protected bool knockbackEnabled=true;
    protected AudioSource audioSource;
    protected bool dead;
    protected Material eyeMaterial;
    private float normalizedHealth;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        portal = GameObject.FindGameObjectWithTag("Portal").transform;
        photonView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        eyeMaterial = eye.gameObject.GetComponent<Renderer>().material;
        health=startHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
            Die();
        debugText.text = health.ToString();
        normalizedHealth = Mathf.Lerp(0,1,(float)health/startHealth);
        eyeMaterial.SetFloat("_Energy",normalizedHealth);
        if(!photonView.IsMine && PhotonNetwork.IsConnected)
            return;
        LocalUpdate();
    }

    void FixedUpdate()
    {
        if(!photonView.IsMine && PhotonNetwork.IsConnected)
            return;
        LocalFixedUpdate();
    }

    protected virtual void LocalUpdate(){}
    protected virtual void LocalFixedUpdate(){}
    
    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(health);
        }
        if (stream.IsReading)
        {
            health = (int)stream.ReceiveNext();
        }
    }

    public float GetNormalizedHealth()
    {
        return normalizedHealth;
    }
    
    public virtual void GetHit(RaycastHit hitInfo, int damage, int knockbackForce)
    {
        string hitVector = hitInfo.normal.ToString();
        string bodyPart="";
        Collider hitCollider = hitInfo.collider;
        if(hitCollider == eye)
            bodyPart="Eye";
        else
            bodyPart="Body";

        photonView.RPC("NetGetHit",RpcTarget.MasterClient,bodyPart,hitVector,damage,knockbackForce);
    }

    [PunRPC]
    protected virtual void NetGetHit(string bodyPartHit, string hitVector, int damage, int knockbackForce)
    {
        Vector3 hitNormal = HelpfulFunctions.StringToVector3(hitVector);
        if(bodyPartHit=="Eye")
        {
            damage = Mathf.RoundToInt(damage*criticalHitMultiplier);
            audioSource.PlayOneShot(eyeHitSound);
        }
        else
            audioSource.PlayOneShot(hitSound);
        health -= damage;

        if(health<=0)
            Die();  

        if(knockbackEnabled)
        {
            rb.AddForce(-hitNormal * knockbackForce, ForceMode.Impulse);
            rb.AddTorque(hitNormal * knockbackForce, ForceMode.Impulse);
        }
    }

    public virtual void GetHookHit(Transform hook, RaycastHit hitInfo)
    {
        Collider hitBodyPart = hitInfo.collider;
        if(hitBodyPart == eye)
            photonView.RPC("NetHookEyeHit",RpcTarget.All);
    }

    [PunRPC]
    protected virtual void NetHookEyeHit()
    {
        eye.gameObject.SetActive(false);
        if(PhotonNetwork.IsMasterClient)
            Die();
    }

    public bool IsEyeHit(Collider hitCollider)
    {
        if(hitCollider == eye)
            return true;
        return false;
    }

    public float GetHealth()
    {
        return normalizedHealth;
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        if(photonView==null)
            return;
        if(!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        var collisionName = other.gameObject.name;
        if(other.gameObject.CompareTag("Portal"))
        {
            if(destroyedAlready)
                return;
            GameManager.instance.InflictDamage();
            GetDestroyed();
        }
        else if(other.gameObject.CompareTag("Ground"))
            GetDestroyed();
        else
            HandleCollisionWithEnvironment(collisionName);
    }

    bool destroyedAlready=false; //this can cause a problem when both eye and body collide with the portal at the same time. So when we get the first hit, we set a bool to not
                                //trigger photonnetwork.destroy twice
    void GetDestroyed()
    {
        destroyedAlready=true;
        PhotonNetwork.Destroy(gameObject);
    }

    protected virtual void HandleCollisionWithEnvironment(string nameOfObstacle)
    {
        
    }

    protected virtual void Die()
    {
        rb.useGravity = true;
        dead=true;
        photonView.RPC("NetDie",RpcTarget.All);
    }

    ParticleSystem deathParticles;
    [PunRPC]
    protected virtual void NetDie()
    {
        audioSource.clip = deathSound;
        audioSource.loop= false;
        audioSource.Play();
        StartCoroutine(BurnToCrisp());
        deathParticles = Instantiate(deathParticlesPrefab,transform);
        
    }

    IEnumerator BurnToCrisp()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        bool burned=false;
        bool darkened=false;
        float timeToDarken = 1.5f;
        float timeToBurn = 1f;
        float timeElapsed=0f;
        while(!darkened)
        {
            timeElapsed+=Time.deltaTime;
            foreach(Renderer renderer in renderers)
                renderer.material.SetFloat("Vector1_CCB4236F", Mathf.Lerp(0f,0.95f,timeElapsed/timeToDarken));
            if(timeElapsed>=timeToDarken)
                darkened=true;
            yield return null;
        }
        timeElapsed=0;
        while(!burned)
        {
            timeElapsed+=Time.deltaTime;
            foreach(Renderer renderer in renderers)
                renderer.material.SetFloat("Vector1_812388C2", Mathf.Lerp(-0.1f,1.1f,timeElapsed/timeToBurn));
            if(timeElapsed>=timeToBurn)
                burned=true;
            yield return null;
        }
        if(deathParticles!=null)
            deathParticles.gameObject.transform.parent = null;
        if(PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);
    }

}
