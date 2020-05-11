using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public enum UserDevice {PC, VR, SPECTATOR};
public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance;
    public string gameVersion = "0.1";
    public string sceneToLaunch;
    public UserDevice userDevice;
    void Awake()
    {
        if(instance == null)
            instance = this;
    }

    void OnDestroy()
    {
        if(instance == this)
            instance = null;
    }
    // Start is called before the first frame update
     void Start()
    {
        SaveUserDevice();

        ConnectToServer();
    }

    void Update()
    {
    }

    void SaveUserDevice()
    {
            PlayerPrefs.SetInt("UserDevice", (int)userDevice);
    }

    private void ConnectToServer()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected To Game Server");
        JoinLobby();
    }

    void JoinLobby()
    {
        //try to join. If failed, create lobby room
        PhotonNetwork.JoinRoom(sceneToLaunch);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Successfully joined " + sceneToLaunch);
        PhotonNetwork.LoadLevel(sceneToLaunch);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogFormat("Failed to join {0}", sceneToLaunch);
        CreateLobby();
    }

    void CreateLobby()
    {
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 10 };
        PhotonNetwork.CreateRoom(sceneToLaunch, roomOps);
        Debug.Log(sceneToLaunch + " created.");
    }

}
