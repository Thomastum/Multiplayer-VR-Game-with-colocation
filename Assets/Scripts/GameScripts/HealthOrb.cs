using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthOrb : MonoBehaviour
{
    public GameObject orbPrefab;
    public GameObject orbPlaceholder;
    public float pullRadius = .3f;
    public bool isPresent=false;
    public float enlargedScale = .23f;
    public Color neutralColor;
    public Color pullingColor;

    private GameObject orb;
    private GameObject placeHolder;
    bool enlarged=false;
    private float scale;
    private bool canPullEnergyBall=false;

    void Start()
    {
        
    }

    void Update()
    {
        if(canPullEnergyBall)
            TryPullEnergyBall();
        if(!canPullEnergyBall && enlarged)
            Deflate();
    }
    public void LightUp()
    {
        isPresent=true;
        if(placeHolder!=null)
            Destroy(placeHolder);
        orb = Instantiate(orbPrefab,transform);
    }

    public void Shatter()
    {
        isPresent=false;
        Destroy(orb);
        placeHolder = Instantiate(orbPlaceholder,transform);
        scale=placeHolder.transform.localScale.x;
    }

    public void Disappear()
    {
        if(isPresent)
            Destroy(orb);
        else
            Destroy(placeHolder);

        isPresent=false;
        canPullEnergyBall=false;
        enlarged=false;
    }

    public void EnablePulling()
    {
        canPullEnergyBall=true;
        placeHolder.GetComponent<Renderer>().material.SetColor("_Color",pullingColor);
    }

    public void DisablePulling()
    {
        canPullEnergyBall=false;
        placeHolder.GetComponent<Renderer>().material.SetColor("_Color",neutralColor);
    }

    void Enlarge()
    {
        placeHolder.transform.localScale = Vector3.one * enlargedScale;
        placeHolder.GetComponent<Renderer>().material.SetFloat("_Intensity",10);
        enlarged=true;
    }

    void Deflate()
    {
        placeHolder.transform.localScale = Vector3.one * scale;
        placeHolder.GetComponent<Renderer>().material.SetFloat("_Intensity",1);
        enlarged=false;
    }

    void TryPullEnergyBall()
    {
        Collider[] hitObjects = Physics.OverlapSphere(transform.position,pullRadius);
        bool withinRadius=false;
        for(int i=0;i<hitObjects.Length;i++)
        {
            GameObject hitObject = hitObjects[i].gameObject;
            if(hitObject.GetComponent<EnergyBall>()!=null)
            {
                withinRadius=true;
                if(!enlarged)
                    Enlarge();
                if(hitObject.transform.parent==null)
                {
                    GameManager.instance.RestoreHealth(hitObject);
                    break;
                }
            }
        }
        
        if(enlarged && !withinRadius)
            Deflate();
    }
}
