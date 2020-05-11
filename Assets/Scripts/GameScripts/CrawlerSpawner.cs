using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CrawlerSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] crawlerPathVariations;
    public GameObject crawlerPrefab;
    public float spawnRate = 5f;
    public float minX=-2;
    public float maxX=2;
    private List<GameObject> crawlerPaths = new List<GameObject>();
    private List<GameObject> crawlersAlive = new List<GameObject>();
    void Start()
    {
        if(PhotonNetwork.IsMasterClient)
            GameManager.instance.onGameOver.AddListener(ResetSpawner);
    }

    float spawnTimer = 0;
    // Update is called once per frame
    void Update()
    {
        if(!PhotonNetwork.IsMasterClient)
            return;

       spawnTimer+=Time.deltaTime;
       
       if(spawnTimer >= spawnRate)
       {
            //Vector3 spawnPos = this.crawlerPath.transform.position;
           //spawnPos.x = Random.Range(minX,maxX);
           var spawnCrawler = crawlerPathVariations[Random.Range(0,crawlerPathVariations.Length)];
            GameObject crawlerPath = Instantiate(spawnCrawler);
           crawlerPaths.Add(crawlerPath);

           GameObject crawler = PhotonNetwork.Instantiate(crawlerPrefab.name,crawlerPath.transform.position,Quaternion.identity);
           crawlersAlive.Add(crawler);

           crawlerPath.GetComponent<CrawlerPath>().StartCrawling(crawler);

           spawnTimer=0;
       }
       
    }

    void OnEnable()
    {
        crawlerPaths.Clear();
        crawlersAlive.Clear();
    }

    void ResetSpawner()
    {
        foreach(GameObject path in crawlerPaths)
        {
            //filter out null because when they get destroyed their references are not removed from list
            if(path!=null)
                Destroy(path);
        }
        foreach(GameObject crawler in crawlersAlive)
        {
            //filter out null because when they get destroyed their references are not removed from list
            if(crawler!=null)
                PhotonNetwork.Destroy(crawler);
        }
    }

    void OnDestroy()
    {
        if(GameManager.instance!=null && PhotonNetwork.IsMasterClient)
            GameManager.instance.onGameOver.RemoveListener(ResetSpawner);
    }


}
