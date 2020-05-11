using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MenuScreen : MonoBehaviour
{
    PhotonView photonView;
    public GameObject startButton;
    public GameObject waitingPanel;
    public GameObject indicatorsPanel;
    public GameObject playerIndicatorPrefab;
    public GameObject survivalTimePanel;
    public Color readyIndicatorColor;
    public Color notReadyIndicatorColor;
    private AudioSource audioSource;
    private List<GameObject> playerIndicators;
    private GameObject previousPageButton;
    private GameObject nextPageButton;
    private List<GameObject> storyPanels;
    private int currentPage=0;

    private List<Player> players = new List<Player>();
    // Start is called before the first frame update
    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        audioSource = GetComponent<AudioSource>();
        GameManager.instance.onGameOver.AddListener(ShowSurvivalTime);
        SetPlayerReady(PhotonNetwork.LocalPlayer,false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
            PlayButtonHit();
    }

    public void NextPage()
    {
        photonView.RPC("NetNextPage",RpcTarget.AllBuffered);
    }

    [PunRPC]
    void NetNextPage()
    {
        audioSource.Play();
        previousPageButton.SetActive(true);

        storyPanels[currentPage].SetActive(false);
        currentPage++;
        storyPanels[currentPage].SetActive(true);
        if(currentPage == storyPanels.Count-1)
            nextPageButton.SetActive(false);
    }

    public void PreviousPage()
    {
        photonView.RPC("NetPreviousPage",RpcTarget.AllBuffered);
    }

    [PunRPC]
    void NetPreviousPage()
    {
        audioSource.Play();
        nextPageButton.SetActive(true);

        storyPanels[currentPage].SetActive(false);
        currentPage--;
        storyPanels[currentPage].SetActive(true);
        if(currentPage == 0)
            previousPageButton.SetActive(false);
    }

    public void ShowSurvivalTime()
    {
        survivalTimePanel.SetActive(true);
        survivalTimePanel.GetComponentInChildren<TextMeshProUGUI>().text = "You survived for " + GameManager.instance.GetTimeSurvived() + " seconds";
    }

    public void PlayButtonHit()
    {
        audioSource.Play();
        SetPlayerReady(PhotonNetwork.LocalPlayer,true);
        startButton.SetActive(false);
        waitingPanel.SetActive(true);
        photonView.RPC("NetPlayButtonHit",RpcTarget.All);
    }

    [PunRPC]
    void NetPlayButtonHit()
    {
        int idx=0;
        foreach(Player player in players)
        {
            if((bool)player.CustomProperties["IsReady"])
                playerIndicators[idx].GetComponent<Image>().color = readyIndicatorColor;
            else
                playerIndicators[idx].GetComponent<Image>().color = notReadyIndicatorColor;  
            idx++;
        }

        if(!PhotonNetwork.IsMasterClient)
            return;

        if(AllPlayersReady())
        {
            foreach(Player player in players)
                SetPlayerReady(player,false);
            Invoke("StartGame",0.7f);
        }

    }

    void StartGame()
    {
        GameManager.instance.StartGame();
    }


    public void UpdatePlayerCount()
    {
        photonView.RPC("NetUpdatePlayerCount",RpcTarget.All);
    }

    [PunRPC]
    void NetUpdatePlayerCount()
    {
        players.Clear();
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            if(!(bool)player.CustomProperties["IsSpectator"])
                players.Add(player);
        }
        if(playerIndicators!=null)
        {
            foreach(GameObject indicator in playerIndicators)
                Destroy(indicator);
        }
        playerIndicators = new List<GameObject>();

        foreach(Player player in players)
        {
            GameObject playerIndicator = Instantiate(playerIndicatorPrefab,indicatorsPanel.transform);
            if((bool)player.CustomProperties["IsReady"])
                playerIndicator.GetComponent<Image>().color = readyIndicatorColor;
            playerIndicators.Add(playerIndicator);
        }

        if(!PhotonNetwork.IsMasterClient)
            return;

        if(AllPlayersReady())
        {
            foreach(Player player in players)
                SetPlayerReady(player,false);
            Invoke("StartGame",0.7f);
        }
    }

    bool AllPlayersReady()
    {
        foreach(Player player in players)
        {
            if(!(bool)player.CustomProperties["IsReady"]) 
                return false;
        }
        return true;
    }

    public void SetPlayerReady(Player player, bool state)
    {
        Hashtable hash = new Hashtable();
        hash.Add("IsReady",state);
        player.SetCustomProperties(hash);
    }

}
