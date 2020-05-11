using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetInteractionSystem;
using Photon.Pun;
using UnityEngine.Events;

[RequireComponent(typeof(NetEquipable))]
public class OffHandWeaponManager : MonoBehaviour
{
    public Transform vacuumPoint;
    public float radius = .5f;
    public float placeRadius = .2f;
    public float angle = 0.7f;
    public float speed = 5;
    public ParticleSystem vacuumParticles;
    private ParticleSystem particlesInstance;
    NetEquipable netEquipable;
    protected PhotonView photonView;
    protected NetHandController vacuumHand;
    private GameObject lockedObject;

    private void Start()
    {
        netEquipable = GetComponent<NetEquipable>();
        photonView = GetComponent<PhotonView>();
        netEquipable.onGrabbed.AddListener(EnableVacuumGrab);
        netEquipable.onReleased.AddListener(DisableVacuumGrab);
    }

    void LockEnergyBall()
    {
        vacuumHand.ActionButtonHold -= VacuumObject;
        photonView.RPC("NetLockEnergyBall",RpcTarget.All,closestObject.GetPhotonView().ViewID);
    }

    [PunRPC]
    void NetLockEnergyBall(int objectViewID)
    {
        lockedObject = PhotonView.Find(objectViewID).gameObject;
        lockedObject.GetComponent<IVacuumable>().LockToPoint(vacuumPoint);
        closestObject=null;
        prevClosestObject=null;
    }

    void StartVacuuming()
    {
        photonView.RPC("NetStartVacuuming",RpcTarget.All);
    }

    [PunRPC]
    void NetStartVacuuming()
    {
        particlesInstance = Instantiate(vacuumParticles,vacuumPoint.transform);
    }
    
    GameObject closestObject;
    GameObject prevClosestObject;

    void VacuumObject()
    {       
        Collider[] hitObjects = Physics.OverlapSphere(transform.position,radius);
        float minDistanceToHand = float.MaxValue;

        for(int i=0;i<hitObjects.Length;i++)
        {  
            if(hitObjects[i].GetComponent<IVacuumable>() == null)
                continue;
            
            //filter out objects not in vacuum's view
            Vector3 normalizedDir = vacuumPoint.forward.normalized;
            Vector3 dirToObject = (hitObjects[i].transform.position - vacuumPoint.position).normalized;
            if(Vector3.Dot(normalizedDir,dirToObject) < angle)
                continue;
                
            float distanceToHand = Vector3.SqrMagnitude(hitObjects[i].transform.position - transform.position);
            if(distanceToHand < minDistanceToHand)
            {
                minDistanceToHand = distanceToHand;
                closestObject = hitObjects[i].gameObject;
            }
        }

        if(closestObject!=null)
        {
            PhotonView closesObjectView = closestObject.GetComponent<PhotonView>();
            if(closesObjectView.Owner != PhotonNetwork.LocalPlayer)
                closesObjectView.TransferOwnership(PhotonNetwork.LocalPlayer);
                
            closestObject.GetComponent<IVacuumable>().GetVacuumed();
        }

        if(prevClosestObject != closestObject && prevClosestObject!=null)
        {
            prevClosestObject.GetComponent<IVacuumable>().VacuumRelease();
        }

        prevClosestObject = closestObject;

        if(closestObject == null)
            return;

        float distanceToObject = (closestObject.transform.position-vacuumPoint.position).magnitude;
        float distanceSpeedMultiplier = Mathf.Lerp(0.1f,1, 1-(distanceToObject/radius));
        closestObject.transform.position = Vector3.Lerp(closestObject.transform.position, vacuumPoint.position, Time.deltaTime*speed*distanceSpeedMultiplier);
        if((closestObject.transform.position-vacuumPoint.position).magnitude < 0.05f)
            LockEnergyBall();
    }

    void StopVacuum()
    {
        photonView.RPC("NetStopVacuum",RpcTarget.All);
        vacuumHand.ActionButtonHold += VacuumObject;
        
        if(lockedObject==null)
            return;
        lockedObject.GetComponent<IVacuumable>().VacuumRelease();
        lockedObject=null;
    }

    [PunRPC]
    void NetStopVacuum()
    {
        var main = particlesInstance.main;
        main.loop = false;
        var emission = particlesInstance.emission;
        emission.rateOverTime = 0;
    }

    public void EnableVacuumGrab()
    {
        vacuumHand = netEquipable.GetObjectHolder().GetComponent<NetHandController>();
        vacuumHand.ActionButtonPress += StartVacuuming;
        vacuumHand.ActionButtonHold += VacuumObject;
        vacuumHand.ActionButtonRelease += StopVacuum;
    }  

    public void DisableVacuumGrab()
    {
        vacuumHand = netEquipable.GetObjectHolder().GetComponent<NetHandController>();
        vacuumHand.ActionButtonPress -= StartVacuuming;
        vacuumHand.ActionButtonHold -= VacuumObject;
        vacuumHand.ActionButtonRelease -= StopVacuum;
    }

    protected void OnDestroy()
    {
        netEquipable.onGrabbed.RemoveListener(EnableVacuumGrab);
        netEquipable.onReleased.RemoveListener(DisableVacuumGrab);
    }
}
