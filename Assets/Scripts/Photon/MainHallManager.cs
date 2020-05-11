using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class MainHallManager : MonoBehaviourPunCallbacks
{
    public static MainHallManager instance;
    
    public GameObject playerVR;
    public GameObject playerPC;
    // Start is called before the first frame update
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
    
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
            return;
        Debug.LogFormat("Welcome to {0} , {1}", SceneManager.GetActiveScene().name, PhotonNetwork.NickName);
        InstantiatePlayer();
    }

    void InstantiatePlayer()
    {
        string device = PlayerPrefs.GetString("UserDevice");

        if (device == "VR")
        {
            Vector3 randomPosition = new Vector3(Random.Range(-3.5f, 3.5f), playerVR.transform.position.y, Random.Range(-3.5f, 3.5f));
            PhotonNetwork.Instantiate(this.playerVR.name, Vector3.zero, Quaternion.identity, 0);
        }
        else if (device == "PC")
        {
            Vector3 randomPosition = new Vector3(Random.Range(-3.5f, 3.5f), playerPC.transform.position.y, Random.Range(-3.5f, 3.5f));
            PhotonNetwork.Instantiate(this.playerPC.name, randomPosition, Quaternion.identity);
        }
    }

    public void QuitRoom()
    {
        Debug.LogFormat("{0} has Left Room {1}", PhotonNetwork.NickName, PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Menu");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " has entered the room.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " has left the room.");
    }
}
