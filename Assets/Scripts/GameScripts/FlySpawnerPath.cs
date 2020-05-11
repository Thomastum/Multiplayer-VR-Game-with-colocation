using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FlySpawnerPath : MonoBehaviour
{
    public List<GameObject> flyPathVariations;
    public GameObject flyPrefab;
    public float startSpawnRate = 7f;
    private float spawnRate;
    float spawnTimer = 0;
    private List<GameObject> activeFlyPaths = new List<GameObject>();
    private List<GameObject> fliesAlive = new List<GameObject>();
    void Start()
    {
        if(PhotonNetwork.IsMasterClient)
            GameManager.instance.onGameOver.AddListener(ResetSpawner);
        spawnRate = startSpawnRate;
        spawnTimer=startSpawnRate;
    }

    // Update is called once per frame
    void Update()
    {
        if(!PhotonNetwork.IsMasterClient)
            return;

       spawnTimer+=Time.deltaTime;
       if(spawnTimer >= spawnRate)
       {
           Spawn();
           spawnTimer=0;
           spawnRate-=0.1f;
       } 
    }

    void OnEnable()
    {
        activeFlyPaths.Clear();
        fliesAlive.Clear();
        spawnRate=startSpawnRate;
    }

    void Spawn()
    {
        int randomPathIndex = Random.Range(0,flyPathVariations.Count);
        GameObject randomPath = flyPathVariations[randomPathIndex];
        GameObject flyPath = Instantiate(randomPath, randomPath.transform.position, randomPath.transform.rotation);
        activeFlyPaths.Add(flyPath);
       
        
        GameObject fly = PhotonNetwork.Instantiate(flyPrefab.name,flyPath.transform.position,Quaternion.identity);
        fliesAlive.Add(fly);
        flyPath.GetComponent<FlyPath>().StartFlying(fly);
        
        
        //PeTaK
        var rbFly = fly.GetComponent<Rigidbody>();
        rbFly.velocity = Vector3.zero;


    }

    void ResetSpawner()
    {
        foreach(GameObject path in activeFlyPaths)
        {
            //filter out null because when they get destroyed their references are not removed from list
            if(path!=null)
                Destroy(path);
        }
        foreach(GameObject fly in fliesAlive)
        {
            //filter out null because when they get destroyed their references are not removed from list
            if(fly!=null)
                PhotonNetwork.Destroy(fly);
        }
    }

    void OnDestroy()
    {
        if(GameManager.instance!=null && PhotonNetwork.IsMasterClient)
            GameManager.instance.onGameOver.RemoveListener(ResetSpawner);
    }

}
