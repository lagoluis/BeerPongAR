using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Need to Rename everything as BeerPong instead of BP
public class Listener : MonoBehaviour {

    [HideInInspector]
    public BP_NetworkManager beerPong;

    public void Awake()
    {
        beerPong = FindObjectOfType(typeof(BP_NetworkManager)) as BP_NetworkManager;
    }

    public virtual void OnStartConnecting()
    {
        // Override
    }

    public virtual void OnStopConnecting()
    {
        // Override
    }

    public virtual void OnServerCreated()
    {
        // Override
    }

    public virtual void OnReceivedBroadcast(string aFromAddress, string aData)
    {
        // Override
    }

    public virtual void OnDiscoveredServer(BeerPongDiscoveredServer aServer)
    {
        // Override
    }

    public virtual void OnJoinedLobby()
    {
        // Override
    }

    public virtual void OnLeftLobby()
    {
        // Override
    }

    public virtual void OnCountdownStarted()
    {
        // Override
    }

    public virtual void OnCountdownCancelled()
    {
        // Override
    }

    public virtual void OnStartGame(List<BeerPongPlayer> aStartingPlayers)
    {
        // Override
    }

    public virtual void OnAbortGame()
    {
        // Override
    }
}
