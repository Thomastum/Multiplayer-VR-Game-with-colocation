using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Jacovone;
public class FlyPath : MonoBehaviour
{
    private GameObject fly;
    private PathMagic pathMagic;

    public void StartFlying(GameObject fly)
    {
        this.fly=fly;
        pathMagic = GetComponent<PathMagic>(); 
        pathMagic.Target = fly.transform;

        Transform globalTarget = GameObject.FindGameObjectWithTag("Portal").transform;
        pathMagic.globalLookAt = globalTarget;
        
        fly.GetComponent<FlyDroneAIPath>().SetPath(this);
        pathMagic.Play();
        
    }

    public Vector3 GetWaypointPosition()
    {
        Vector3 localPos = pathMagic.waypoints[pathMagic.GetCurrentWaypoint()+1].position;
        Vector3 globalPos = transform.position + localPos;
        return globalPos;
    }

    public void Pause()
    {
        pathMagic.Pause();
    }

    public void Resume()
    {
        pathMagic.UpdateTarget();
        pathMagic.Play();
    }

    public void Destroy()
    {
        if (gameObject != null)
        {
            DestroyImmediate(gameObject);
        }
    }
}
