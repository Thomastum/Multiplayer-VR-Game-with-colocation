using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ItemSpawner : MonoBehaviour
{
    public GameObject item;
    public Transform spawnPoint;
    private PhotonView photonView;
    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }


    GameObject itemInstance;
    public void Spawn()
    {            
        if(PhotonNetwork.IsConnected)
            photonView.RPC("NetSpawn",RpcTarget.MasterClient);
        else
            itemInstance = Instantiate(item, spawnPoint.position, item.transform.rotation);
    }

    [PunRPC]
    void NetSpawn()
    {
        PhotonNetwork.InstantiateSceneObject(this.item.name, spawnPoint.position, Quaternion.identity, 0);
    }

}
