using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;


public class GameManager : MonoBehaviour, IPunObservable
{
    public static GameManager instance;
    public int startPortalHealth=5;
    public ParticleSystem portalHitParticles;
    public ParticleSystem portalDeathParticles;
    public AudioClip portalHitSound;
    public AudioClip gameOverSound;
    public Light gameLightning;
    public float lightLerpTime=2f;
    public GameObject menuScreenPrefab;
    public List<Renderer> healthMaterials;
    //public LifePedestal lifePedestal;
    private GameObject menuScreen;
    private List<HealthOrb> healthOrbs;
    private PhotonView photonView;
    private FlySpawner flySpawner;
    private FlySpawnerPath flySpawnerPath;
    private CrawlerSpawner crawlerSpawner;
    private AudioSource audioSource;
    private int health;

    public UnityEvent onGameOver;
    public UnityEvent onGameStart;
    
    bool isPlaying=false;
    public float timeSurvived = 0;

    void Awake()
    {
        if(instance==null)
            instance=this;

        Setup();
    }
    void OnDestroy()
    {
        if(instance==this)
            instance=null;
    }
    // Start is called before the first frame update
    void Setup()
    {
        flySpawner = GetComponent<FlySpawner>();
        flySpawnerPath = GetComponent<FlySpawnerPath>();
        crawlerSpawner = GetComponent<CrawlerSpawner>();
        photonView=GetComponent<PhotonView>();
        audioSource=GetComponent<AudioSource>();
        menuScreen = GameObject.FindGameObjectWithTag("MenuScreen");
        gameLightning.intensity=0;
        flySpawnerPath.enabled=false;
        crawlerSpawner.enabled=false;
    }

    public GameObject GetMenuScreen()
    {
        return menuScreen;
    }
    public void FindMenuScreen()
    {
        menuScreen = GameObject.FindGameObjectWithTag("MenuScreen");
        if(menuScreen==null)
            throw new ArgumentNullException("Couldnt find menuScreen");
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
            InflictDamage();
        if(!PhotonNetwork.IsMasterClient)
            return;
        
        if(!isPlaying)
            return;

        timeSurvived += Time.deltaTime;
    }

    public void UpdatePlayersList()
    {
        if(menuScreen==null)
            FindMenuScreen();
        photonView.RPC("NetUpdatePlayersList",RpcTarget.MasterClient);
    }

    [PunRPC]
    void NetUpdatePlayersList()
    {
        menuScreen.GetComponent<MenuScreen>().UpdatePlayerCount();
    }

    public void StartGame()
    {
        if(!PhotonNetwork.IsMasterClient)
            return;

        timeSurvived=0;
        isPlaying=true;
        flySpawnerPath.enabled=true;
        crawlerSpawner.enabled=true;
        PhotonNetwork.Destroy(menuScreen);
        photonView.RPC("NetStartGame",RpcTarget.All);
    }

    [PunRPC]
    void NetStartGame()
    {
        health = startPortalHealth;
        UpdateMaterialColor();
        onGameStart.Invoke();
        StartCoroutine(EnableLight(true));
    }

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Mathf.RoundToInt(timeSurvived));
        }
        if (stream.IsReading)
        {
            timeSurvived = (int)stream.ReceiveNext();
        }
    }

    public int GetTimeSurvived()
    {
        return Mathf.RoundToInt(timeSurvived);
    }

    public void InflictDamage()
    {
        photonView.RPC("NetInflictDamage",RpcTarget.All);
        if(health==0)
            GameOver();
    }


    [PunRPC]
    void NetInflictDamage()
    {
        //lifePedestal.LoseHealth();
        health--;
        UpdateMaterialColor();

        Instantiate(portalHitParticles);
        audioSource.PlayOneShot(portalHitSound);

    }

    public void RestoreHealth(GameObject energyBall)
    {
        // PhotonNetwork.Destroy(energyBall);
        // photonView.RPC("NetRestoreHealth",RpcTarget.All);
    }

    // [PunRPC]
    // public void NetRestoreHealth()
    // {
    //     lifePedestal.RestoreHealth();
    // }

    public void GameOver()
    {
        flySpawnerPath.enabled=false;
        crawlerSpawner.enabled=false;
        isPlaying=false;
        
        menuScreen = PhotonNetwork.InstantiateSceneObject(menuScreenPrefab.name,menuScreenPrefab.transform.position,menuScreenPrefab.transform.rotation);
        menuScreen.GetComponent<MenuScreen>().UpdatePlayerCount();
        photonView.RPC("NetGameOver",RpcTarget.All);
    }

    [PunRPC]
    void NetGameOver()
    {
        onGameOver.Invoke();
        audioSource.PlayOneShot(gameOverSound);
        Instantiate(portalDeathParticles);
        StartCoroutine(EnableLight(false));
    }

    void UpdateMaterialColor()
    {
        float normalizedHealth = Mathf.Lerp(0,1,(float)health/(float)startPortalHealth);
        foreach(Renderer renderer in healthMaterials)
            renderer.material.SetFloat("_Health",normalizedHealth);
    }

    IEnumerator EnableLight(bool enable)
    {
        float elapsedTime = 0f;
        while(elapsedTime<lightLerpTime)
        {
            elapsedTime+=Time.deltaTime;
            
            if(enable)
                gameLightning.intensity = Mathf.Lerp(0,1,elapsedTime/lightLerpTime);
            else
                gameLightning.intensity = Mathf.Lerp(1,0,elapsedTime/lightLerpTime);
            yield return null;
        }
    }

    void ResumeGame()
    {
        Time.timeScale=1;
    }

    void PauseGame()
    {
        Time.timeScale=0;
    }



}
