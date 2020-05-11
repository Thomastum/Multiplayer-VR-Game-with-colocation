using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FlySpawner : MonoBehaviour
{
    public GameObject flyDronePrefab;
    
    public float startingSpawnRate = 5f;
    public float minSpawnRate = 1f;

    public float minDistance = 15f;
    public float maxDistance = 20f;
    public float minHeight = 1f;
    public float maxHeight = 5f;

    private float spawnRate;

    private List<GameObject> fliesAlive = new List<GameObject>();

    void Start()
    {
        spawnRate=startingSpawnRate;
        GameManager.instance.onGameOver.AddListener(ResetSpawner);
    }

    float spawnTimer = 0;
    // Update is called once per frame
    void Update()
    {
        if(!PhotonNetwork.IsMasterClient)
            return;
        
       if(spawnTimer >= spawnRate)
       {
           float distance = Random.Range(minDistance, maxDistance);
           float height = Random.Range(minHeight,maxHeight);
           float angle = Random.Range(0, Mathf.PI);
           //float angle = Random.Range(-Mathf.PI, Mathf.PI);
           Vector3 spawnPos = new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*distance;
           spawnPos.y = height;
           GameObject fly = PhotonNetwork.Instantiate(flyDronePrefab.name,spawnPos,Quaternion.identity);
           fliesAlive.Add(fly);

           spawnTimer=0;
           if(spawnRate>minSpawnRate)
                spawnRate -= 0.1f;
       } 
       spawnTimer+=Time.deltaTime;
    }

    void OnEnable()
    {
        fliesAlive.Clear();
    }

    void OnDestroy()
    {
        if(GameManager.instance!=null)
        GameManager.instance.onGameOver.RemoveListener(ResetSpawner);
    }

    void ResetSpawner()
    {
        spawnRate=startingSpawnRate;

        foreach(GameObject fly in fliesAlive)
        {
            //filter out null flies because when flies get destroyed their references are not removed from list
            if(fly!=null)
                PhotonNetwork.Destroy(fly);
        }
    }
}
