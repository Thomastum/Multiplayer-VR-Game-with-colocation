using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
    public static PhotonLobby instance;
    public string gameVersion = "0";
    public int playerID;

    public delegate void OnConnectedToServer();
    public event OnConnectedToServer onConnectedToServer;

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
        ConnectToServer();
        playerID = Random.Range(0, 100);
        PhotonNetwork.NickName = playerID.ToString();
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
        onConnectedToServer.Invoke();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Successfully joined " + PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.LoadLevel("MultiplayerScene");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No free rooms found. Creating a new room.");
        CreateRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to create a room. Trying again...");
        CreateRoom();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TryJoinRandomRoom()
    {
        Debug.Log("Starting to search for a room...");
        PhotonNetwork.JoinRandomRoom();
    }

    void CreateRoom()
    {
        int randomRoomId = Random.Range(0, 10000);
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 10 };
        PhotonNetwork.CreateRoom("Room" + randomRoomId, roomOps);
        Debug.Log("New room created.");
    }
}
