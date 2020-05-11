using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LifePedestal : MonoBehaviour
{
    // Start is called before the first frame update
    public ParticleSystem restoreHealthParticles;
    private List<HealthOrb> healthOrbs;
    private int health;
    void Start()
    {
        healthOrbs = new List<HealthOrb>();
        foreach(Transform orb in transform)
        {
            HealthOrb healthOrb = orb.gameObject.GetComponent<HealthOrb>();
            healthOrbs.Add(healthOrb);
        }
        GameManager.instance.onGameStart.AddListener(SpawnHealthOrbs);
        GameManager.instance.onGameOver.AddListener(DestroyOrbs);
    }

    void SpawnHealthOrbs()
    {
        foreach(HealthOrb orb in healthOrbs)
            orb.LightUp();
        health=healthOrbs.Count;
    }

    void DestroyOrbs()
    {
        foreach(HealthOrb orb in healthOrbs)
            orb.Disappear();
    }

    public void LoseHealth()
    {
       for(int i=healthOrbs.Count-1;i>=0;i--)
        {
            if(healthOrbs[i].isPresent)
            {
                healthOrbs[i].Shatter();
                if(i!=healthOrbs.Count-1)
                    healthOrbs[i+1].DisablePulling();
                healthOrbs[i].EnablePulling();
                health--;
                break;
            }
        }
        
        if(health==0 && PhotonNetwork.IsMasterClient)
            GameManager.instance.GameOver();
    }


    public void RestoreHealth()
    {
        health++;
        Instantiate(restoreHealthParticles, healthOrbs[health-1].transform.position,Quaternion.identity);
        healthOrbs[health-1].LightUp();
        healthOrbs[health-1].DisablePulling();
        
        if(health < healthOrbs.Count)
            healthOrbs[health].EnablePulling();

    }


    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        GameManager.instance.onGameStart.RemoveListener(SpawnHealthOrbs);
        GameManager.instance.onGameOver.RemoveListener(DestroyOrbs);
    }
}
