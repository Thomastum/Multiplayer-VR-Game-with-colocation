using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
public class VRPlayerManager : MonoBehaviour, IPunInstantiateMagicCallback
//public class VRPlayerManager : MonoBehaviour
{
    public GameObject head;
    public GameObject leftHand;
    public GameObject rightHand;
    private GameObject localNetworkPlayer;
    public MonoBehaviour[] nonLocalPlayerScripts;
    private PhotonView photonView;
    private int score;

    private bool playerHMDMounted = true;
    public bool HMDMounted {get{return playerHMDMounted;}}
    // Start is called before the first frame update
    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        leftHand.GetComponent<NetHandController>().otherHand = rightHand.GetComponent<NetHandController>();
        rightHand.GetComponent<NetHandController>().otherHand = leftHand.GetComponent<NetHandController>();
        if(!photonView.IsMine && PhotonNetwork.IsConnected)
            RemoveNonLocalPlayerComponents();
        // OVRManager.HMDMounted += HandleHMDMounted;
        // OVRManager.HMDUnmounted += HandleHMDUnmounted;
    }

    void Update()
    {
    }

    void RemoveNonLocalPlayerComponents()
    {
        foreach(MonoBehaviour script in nonLocalPlayerScripts)
        {
            if(script != null)
                Destroy(script);
        }
        head.GetComponent<Camera>().enabled = false;
        head.GetComponent<AudioListener>().enabled = false;
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        info.Sender.TagObject = this.gameObject;
    }

    public void ReleaseHeldObjects()
    {
        //releases grabbed objects, if any, before getting destroyed
        leftHand.GetComponent<NetHandController>().ReleaseObject();
        rightHand.GetComponent<NetHandController>().ReleaseObject();
    }

    #region Mounting and Unmounting headset
    void HandleHMDMounted()
    {
        playerHMDMounted = true;
        photonView.RPC("UpdatePlayerMountState",RpcTarget.All,playerHMDMounted);
    }

    void HandleHMDUnmounted()
    {
        playerHMDMounted = false;
        photonView.RPC("UpdatePlayerMountState",RpcTarget.All,playerHMDMounted);
    }

    [PunRPC]
    void UpdatePlayerMountState(bool mounted)
    {
        playerHMDMounted = mounted;
        if(mounted)
            PhotonManager.instance.OnPlayerHMDMounted();
        else
            PhotonManager.instance.OnPlayerHMDUnmounted();
    }

    void OnDestroy()
    {
        if(photonView.IsMine && PhotonNetwork.IsConnected)
        {
            OVRManager.HMDMounted -= HandleHMDMounted;
            OVRManager.HMDUnmounted -= HandleHMDUnmounted;
        }
    }
    #endregion
}
