using System;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.Events;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager instance;
    public GameObject playerVR;
    public GameObject playerPC;
    public GameObject spectator;
    private bool gameIsPlaying = true;
    public UnityEvent onPlayerJoined;
    private List<Player> players = new List<Player>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
            return;
        PhotonNetwork.NickName = "Player " + PhotonNetwork.PlayerList.Length.ToString();
        Debug.LogFormat("Welcome to {0} , {1}", SceneManager.GetActiveScene().name, PhotonNetwork.NickName);
        InstantiatePlayer();
    }

    void InstantiatePlayer()
    {
        UserDevice device = (UserDevice)PlayerPrefs.GetInt("UserDevice");


        GameObject playerObject;
        switch (device)
        {
            case UserDevice.VR:
                playerObject = playerVR;
                PhotonNetwork.Instantiate(playerObject.name, playerObject.transform.position, Quaternion.identity);
                SetPlayerSpectator(false);
                break;
            case UserDevice.PC:
                playerObject = playerPC;    
                PhotonNetwork.Instantiate(playerObject.name, playerObject.transform.position, Quaternion.identity);
                SetPlayerSpectator(false);
                break;
            case UserDevice.SPECTATOR:
                Instantiate(spectator);
                SetPlayerSpectator(true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        SetPlayerReady(false);
        GameManager.instance.UpdatePlayersList();
    }

    public void SetPlayerSpectator(bool state)
    {
        Hashtable hash = new Hashtable();
        hash.Add("IsSpectator",state);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public void SetPlayerReady(bool state)
    {
        Hashtable hash = new Hashtable();
        hash.Add("IsReady",state);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public void QuitRoom()
    {
        Debug.LogFormat("{0} has Left Room {1}", PhotonNetwork.NickName, PhotonNetwork.CurrentRoom.Name);
        
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("LoginScene");
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        onPlayerJoined.Invoke();
        Debug.Log(player.NickName + " has entered the room.");
    }

    public override void OnPlayerLeftRoom(Player player)
    {  
        if(PhotonNetwork.IsMasterClient)
        {
            if(GameManager.instance.GetMenuScreen() == null)
                GameManager.instance.FindMenuScreen();
            GameManager.instance.UpdatePlayersList();

            GameObject playerObj = (GameObject)player.TagObject;
            if(playerObj.GetComponent<VRPlayerManager>() != null)
                playerObj.GetComponent<VRPlayerManager>().ReleaseHeldObjects();
            
        }
        Debug.Log(player.NickName + " has left the room.");
    }

    #region Pausing and resuming
    public void OnPlayerHMDMounted()
    {
        if(!PhotonNetwork.IsMasterClient)
            return;

        foreach(Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerObject = (GameObject)player.TagObject;
            if(playerObject.GetComponent<VRPlayerManager>() == null)
                continue;
            
            if(!playerObject.GetComponent<VRPlayerManager>().HMDMounted)
                return;
        }

        photonView.RPC("ResumeGame",RpcTarget.All);
    }

    public void OnPlayerHMDUnmounted()
    {
        if(!PhotonNetwork.IsMasterClient)
            return;

        if(gameIsPlaying)
            photonView.RPC("PauseGame",RpcTarget.All);
    }

    [PunRPC]
    void ResumeGame()
    {
        GameObject localPlayer = (GameObject)PhotonNetwork.LocalPlayer.TagObject;
        if(localPlayer.GetComponent<VRPlayerManager>() != null)
            Debug.Log("GameResumed");
    }

    [PunRPC]
    void PauseGame()
    {
        GameObject localPlayer = (GameObject)PhotonNetwork.LocalPlayer.TagObject;
        if(localPlayer.GetComponent<VRPlayerManager>() != null)
            Debug.Log("GamePaused");
    }
    #endregion
}
