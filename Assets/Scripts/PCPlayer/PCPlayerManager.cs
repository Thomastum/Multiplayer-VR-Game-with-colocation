using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PCPlayerManager : MonoBehaviour,IPunInstantiateMagicCallback
{
    public GameObject head;
    public MonoBehaviour[] nonLocalPlayerComponents;
    private PhotonView photonView;
    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        if(!photonView.IsMine && PhotonNetwork.IsConnected)
            RemoveNonLocalPlayerComponents();
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        info.Sender.TagObject = this.gameObject;
    }

    void RemoveNonLocalPlayerComponents()
    {
        foreach(MonoBehaviour script in nonLocalPlayerComponents)
            Destroy(script);
        head.GetComponent<Camera>().enabled = false;
        head.GetComponent<AudioListener>().enabled = false;
    }

}
