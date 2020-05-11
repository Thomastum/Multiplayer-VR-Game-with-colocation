using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Jacovone;
public class CrawlerPath : MonoBehaviour
{
    public GameObject crawlerPrefab;
    private GameObject crawler;
    private PathMagic pathMagic;
    public void EndOfPathReached()
    {
        pathMagic.waypoints[pathMagic.waypoints.Length-1].reached.RemoveListener(crawler.GetComponent<CrawlerAI>().AttackPortal);
        Destroy(gameObject);
    }

    public void StartCrawling(GameObject crawler)
    {
        this.crawler=crawler;
        pathMagic = GetComponent<PathMagic>(); 
        pathMagic.Target = crawler.transform;
        pathMagic.waypoints[pathMagic.waypoints.Length-1].reached.AddListener(crawler.GetComponent<CrawlerAI>().AttackPortal);
        pathMagic.Play();
    }
}
