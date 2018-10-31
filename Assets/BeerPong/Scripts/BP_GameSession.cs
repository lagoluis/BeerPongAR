using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.XR.iOS;

public enum BP_GameState
{
    Offline,
    Connecting,
    Lobby,
    Countdown,
    SendLocationSync,
    WaitForLocationSync,
    WaitingForRolls,
    Scoring,
    GameOver
}

public class BP_GameSession : NetworkBehaviour 
{
    public Text gameStateField;
    public Text gameRulesField;
   
    public static BP_GameSession instance;
   
    BP_Listener networkListener;
    List<BP_PlayerScript> players;
    string specialMessage = "";
    private BeerPongNetworkTransmitter _networkTransmitter;
    private BP_ARSessionManager _arSessionManager;
   
    [SyncVar]
    public BP_GameState gameState;
   
    [SyncVar]
    public string message = "";
   
    public void OnDestroy()
    {
        if(gameStateField != null){
            gameStateField.text = "";
            gameStateField.gameObject.SetActive(false);
        }
        if(gameRulesField != null){
            gameRulesField.gameObject.SetActive(false);
        }
    }
   
    [Server]
    public override void OnStartServer()
    {
        networkListener = FindObjectOfType<BP_Listener>();
        _arSessionManager = FindObjectOfType<BP_ARSessionManager>();
        gameState = BP_GameState.Connecting;
    }
   
    [Server]
    public void OnStartGame(List<BeerPongPlayer> aStartingPlayers)
    {
        players = aStartingPlayers.Select(p => p as BP_PlayerScript).ToList();
   	
        RpcOnStartedGame();
        foreach(BP_PlayerScript p in players)
        {
            p.RpcOnStartedGame();
            p.locationSynced = false;
        }
        StartCoroutine(RunGame());
    }
   
    [Server]
    public void OnAbortGame()
    {
        RpcOnAbortedGame();
    }
   
    [Client]
    public override void OnStartClient()
    {
        if (instance)
            Debug.LogError("ERROR: Another GameSession!");
        instance = this;
   
        networkListener = FindObjectOfType<BP_Listener>();
        networkListener.gameSession = this;
   
        _networkTransmitter = GetComponent<BeerPongNetworkTransmitter>();
   
        if (gameState != BP_GameState.Lobby)
            gameState = BP_GameState.Lobby;
    }
   
    [Command]
    public void CmdSendWorldMap()
    {
        ARWorldMap aRWorldMap = _arSessionManager.GetSavedWorldMap();
        StartCoroutine(_networkTransmitter.SendBytesToClientsRoutine(0, aRWorldMap.SerializeToByteArray()));
    }
   
    [Client]
    private void OnDataCompleteReceived(int transmissionId, byte[] data)
    {
        BP_NetworkManager networkManager = NetworkManager.singleton as BP_NetworkManager;
        BP_PlayerScript p = networkManager.localPlayer as BP_PlayerScript;
   	 
        if(p != null)
        {
            //deserealize data and relocalize
            StartCoroutine(p.RelocateDevice(data));
        }
    }
   
    [Client]
    private void OnDataFragmentReceived(int transmissionId, byte[] data)
    {
        //update a progress bar or do something else witht the information
    }
   
    public void OnJoinedLobby()
    {
        gameState = BP_GameState.Lobby;
    }
   
    public void OnLeftLobby()
    {
        gameState = BP_GameState.Offline;
    }
   
    public void OnCountdownStarted()
    {
        gameState = BP_GameState.Countdown;
    }
   
    public void OnCountdownCancelled()
    {
        gameState = BP_GameState.Lobby;    
    }
   
    [Server]
    IEnumerator RunGame()
    {    
        gameState = BP_GameState.WaitForLocationSync;
        while(!AllPlayersHaveSyncedLocation())
        {
            yield return null;
        }
        gameState = BP_GameState.WaitingForRolls;
    }
   
    [Server]
    bool AllPlayersHaveSyncedLocation()
    {
        return players.All(p => p.locationSynced);
    }
    
    [Server]
    public void PlayAgain()
    {
        StartCoroutine(RunGame());
    }
   
    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            if (gameState == BP_GameState.Countdown)
            {
                //message = "Game Starting in " + Mathf.Ceil(networkListener.beerPong.CountdownTimer()) + "...";
            }
            else if (specialMessage != "")
            {
                message = specialMessage;
            }
            else
            {
                if (gameState == BP_GameState.WaitingForRolls)
                {
                    message = "";
                }
                else
                {
                    message = gameState.ToString();
                }
            }
        }
        gameStateField.text = message;
    }
   
    //Client RPCs
    public void RpcOnStartedGame()
    {
        _networkTransmitter = GetComponent<BeerPongNetworkTransmitter>();
        _networkTransmitter.BeerPongOnDataCompletelyReceived += OnDataCompleteReceived;
        _networkTransmitter.BeerPongOnDataFragmentReceived += OnDataFragmentReceived;
    }
   
    [ClientRpc]
    public void RpcOnAbortedGame()
    {
        gameRulesField.gameObject.SetActive(false);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	
}
