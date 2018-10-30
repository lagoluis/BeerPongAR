using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BeerPongDiscoveredServer : BeerPongBroadcastData{
    public string rawData;
    public string ipAddress;
    public float timestamp;

    public BeerPongDiscoveredServer(BeerPongBroadcastData aData)
    {
        version = aData.version;
        peerId = aData.peerId;
        isOpen = aData.isOpen;
        numPlayers = aData.numPlayers;
        serverScore = aData.serverScore;
        privateTeamKey = aData.privateTeamKey;
    }
}

public class BeerPongClient : NetworkDiscovery {

    public BP_NetworkManager networkManager;
    public Dictionary<string, BeerPongDiscoveredServer> discoveredServers;
    public const float ServerKeepAliveTime = 5.0f;
    public bool autoJoin;

    public Queue<string> receivedBroadcastLog;

    private const int maxLogLines = 4;
    private const string broadcastLogTokens = "-.";
    private int broadcastLogCounter = 0;

	// Use this for initialization
	void Start () {
        discoveredServers = new Dictionary<string, BeerPongDiscoveredServer>();
        receivedBroadcastLog = new Queue<string>();
        showGUI = false;

        InvokeRepeating("CleanServerList", 3, 1);
	}

    public void Setup(BP_NetworkManager aNetworkManager)
    {
        networkManager = aNetworkManager;
        broadcastKey = Mathf.Abs(aNetworkManager.broadcastIdentifier.GetHashCode());//Make sure broadcastKey matches server
    }

    public void Reset()
    {
        discoveredServers.Clear();
        receivedBroadcastLog.Clear();
        autoJoin = false;
    }

    public void StartJoining()
    {
        Reset();
        if(!Initialize())
        {
            Debug.LogError("#BeerPong# Network port is unavailable!");
        }
        if(!StartAsClient())
        {
            Debug.LogError("#BeerPong# Unable to listen!");

            //Clean up some data that Unity seems to not do
            if(hostId != -1)
            {
                NetworkTransport.RemoveHost(hostId);
                hostId = -1;
            }
        }
        autoJoin = true;
    }

    public void CleanServerList()
    {
        var toRemove = new List<string>();
        foreach(var kvp in discoveredServers)
        {
            float timeSinceLastBroadcast = Time.time - kvp.Value.timestamp;
            if(timeSinceLastBroadcast > ServerKeepAliveTime){
                toRemove.Add(kvp.Key);
            }
        }

        foreach(string server in toRemove){
            discoveredServers.Remove(server);
        }
    }

    public override void OnReceivedBroadcast(string aFromAddress, string aRawData)
    {
        BeerPongBroadcastData data = new BeerPongBroadcastData();
        data.FromString(aRawData);

        //Debug log
        broadcastLogCounter += 1;
        receivedBroadcastLog.Enqueue(broadcastLogTokens[broadcastLogCounter % broadcastLogTokens.Length] + " " + aRawData);
        if(receivedBroadcastLog.Count > maxLogLines){
            receivedBroadcastLog.Dequeue();
        }

        var server = new BeerPongDiscoveredServer(data);
        server.rawData = aRawData;
        server.ipAddress = aFromAddress;
        server.timestamp = Time.time;

        bool newData = false;
        if(!discoveredServers.ContainsKey(aFromAddress)){
            //New Server
            discoveredServers.Add(aFromAddress, server);
            newData = true;
        }
        else
        {
            if(discoveredServers[aFromAddress].rawData != aRawData)
            {
                //Old Server with new info
                discoveredServers[aFromAddress] = server;
                newData = true;
            }
            else
            {
                //Just update the timestamp
                discoveredServers[aFromAddress].timestamp = Time.time;
                newData = false;
            }
        }

        networkManager.OnReceivedBroadcast(aFromAddress, aRawData);

        if(newData)
        {
            networkManager.OnDiscoveredServer(server);
        }
    }
}
