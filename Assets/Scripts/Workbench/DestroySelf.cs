using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DestroySelf : MonoBehaviour, IPunObservable
{
    public float timeToDestroy = 10f;
    private IEnumerator destroyCoroutine;
    private PhotonView photonview;

    void Start()
    {
        photonview = GetComponent<PhotonView>();
    }

    public void DestroyInstance()
    {
        destroyCoroutine = DestroyTimer();
        StartCoroutine(destroyCoroutine);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(destroyCoroutine);
        }
        if (stream.IsReading)
        {
            destroyCoroutine = (IEnumerator)stream.ReceiveNext();
        }
    }

    public void CancelDestroy()
    {
        if(destroyCoroutine!=null)
        {
            StopCoroutine(destroyCoroutine);
            destroyCoroutine=null;
        }
    }

    IEnumerator DestroyTimer()
    {
        float timePassed = 0;
        while(timePassed<timeToDestroy)
        {
            timePassed += Time.deltaTime;
            yield return null;
        }
        PhotonNetwork.Destroy(gameObject);
    }
}
