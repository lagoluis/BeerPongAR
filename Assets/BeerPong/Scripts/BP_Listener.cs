using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class BP_Listener : Listener 
{
       public enum BeerPongNetworkState{
           Init,
           Offline,
           Connecting,
           Connected,
           Disrupted
       }
       [HideInInspector]
       public BeerPongNetworkState networkState = BeerPongNetworkState.Init;
       public Text networkStateField;
    
       public GameObject gameSessionPrefab;
       public BP_GameSession gameSession;
    
    // Use this for initialization
    public void Start () 
       {
           networkState = BeerPongNetworkState.Offline;
    
           ClientScene.RegisterPrefab(gameSessionPrefab);
    }
    
       public override void OnStartConnecting()
       {
           networkState = BeerPongNetworkState.Connecting;
       }
    
       public override void OnStopConnecting()
       {
           networkState = BeerPongNetworkState.Offline;
       }
    
       public override void OnServerCreated()
       {
           //Create game session
           BP_GameSession oldSession = FindObjectOfType<BP_GameSession>();
           if(oldSession == null)
           {
               GameObject serverSession = Instantiate(gameSessionPrefab);
               NetworkServer.Spawn(serverSession);
           }
           else
           {
               Debug.LogError("GameSession already exists!");
           }
       }
    
       public override void OnJoinedLobby()
       {
           networkState = BeerPongNetworkState.Connected;
    
           gameSession = FindObjectOfType<BP_GameSession>();
           if(gameSession){
               gameSession.OnJoinedLobby();
           }
       }
    
       public override void OnLeftLobby()
       {
           networkState = BeerPongNetworkState.Offline;
    
           gameSession.OnLeftLobby();
       }
    
       public override void OnCountdownStarted()
       {
           gameSession.OnCountdownStarted();
       }
    
       public override void OnCountdownCancelled()
       {
           gameSession.OnCountdownCancelled();
       }
    
       public override void OnStartGame(List<BeerPongPlayer> aStartingPlayers)
       {
           Debug.Log("GO!");
           gameSession.OnStartGame(aStartingPlayers);
       }
    
       public override void OnAbortGame()
       {
           Debug.Log("ABORT!");
           gameSession.OnAbortGame();
       }
    
       // Update is called once per frame
       void Update () 
       {
           networkStateField.text = networkState.ToString();	
    }
}
