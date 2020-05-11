using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetInteractionSystem;
using Photon.Pun;
public class EnergyBall : MonoBehaviour, IPunObservable, IVacuumable
{
    private NetPickable netPickable;
    private GrapplingHookManager hookManager;
    private Rigidbody rb;
    private bool loadedOnWeapon=false;

    private float normalizedEnergy;
    // Start is called before the first frame update
    void Awake()
    {
        rb=GetComponent<Rigidbody>();

        netPickable = GetComponent<NetPickable>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rb.isKinematic);
        }
        if (stream.IsReading)
        {
            rb.isKinematic = (bool)stream.ReceiveNext();
        }
    }

    public void SetEnergyLevel(float normalizedEnergy)
    {
        this.normalizedEnergy=normalizedEnergy;
        GetComponent<Renderer>().material.SetFloat("_Energy",normalizedEnergy);
    }

    public float GetEnergyLevel()
    {
        return normalizedEnergy;
    }


    public void LoadOntoWeapon(GrapplingHookManager hookManager,Transform parent)
    {
        this.hookManager=hookManager;
        loadedOnWeapon=true;
        LockToPoint(parent);
    }

    public bool CanBeVacuumed()
    {
        return true; 
    }

    public void LockToPoint(Transform lockPoint)
    {
        if(transform.parent == lockPoint)
            return;
        transform.parent = lockPoint;
        transform.localPosition=Vector3.zero;
        transform.localRotation=Quaternion.identity;
        rb.isKinematic=true;
    }  

    public void GetVacuumed()
    {
        rb.useGravity=false;
        rb.isKinematic=true;
        rb.velocity=Vector3.zero;
    }

    public void VacuumRelease()
    {
        transform.parent=null;
        rb.useGravity=true;
        rb.isKinematic=false;
    }
}
