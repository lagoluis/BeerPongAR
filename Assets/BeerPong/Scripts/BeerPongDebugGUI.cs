using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;

public class BeerPongDebugGUI : MonoBehaviour {

    private BeerPong beerPong;
    private BP_NetworkManager networkManager;

    public void Awake()
    {
        beerPong = FindObjectOfType(typeof(BeerPong)) as BeerPong;
        networkManager = GetComponent<BP_NetworkManager>();
    }

    string NetworkDebugString()
    {
        string serverString = "[SERVER]\n";
        if(NetworkServer.active && networkManager.numPlayers > 0)
        {
            serverString += "Hosting at " + Network.player.ipAddress + "\n";
            serverString += string.Format("Players Ready = {0}/{1}", networkManager.NumReadyPlayers(), networkManager.NumPlayers()) + "\n";
        }
        if(networkManager.discoveryServer.running && networkManager.discoveryServer.isServer)
        {
            serverString += "Broadcasting: " + networkManager.discoveryServer.broadcastData + "\n";
        }

        string clientString = "[CLIENT]\n";
        if(networkManager.IsClientConnected())
        {
            clientString += "Connected to server: " + networkManager.client.connection.address + "\n";
        }
        if(networkManager.discoveryClient.running && networkManager.discoveryClient.isClient)
        {
            // Discovered Servers
            clientString += "Discovered servers =";
            foreach(BeerPongDiscoveredServer server in networkManager.discoveryClient.discoveredServers.Values)
            {
                bool isMe = (server.peerId == networkManager.peerId);
                clientString += "\n- ";
                if(isMe){
                    clientString += "(me) ";
                }
                clientString += server.ipAddress + " : " + server.rawData;
            }
            clientString += "\n";

            //Received broadcasts
            clientString += "Received broadcasts =";
            foreach(var entry in networkManager.discoveryClient.receivedBroadcastLog){
                clientString += "\n" + entry;
            }
            clientString += "\n";
        }
        return serverString + "\n" + clientString;
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();
        {
            if(networkManager.IsConnected())
            {
                GUI.color = Color.red;
                if(GUILayout.Button("Disconnect", GUILayout.Width(200), GUILayout.Height(100)))
                {
                    beerPong.Cancel();
                }
                GUI.color = Color.white;
            }
            else if(networkManager.IsBroadcasting() || networkManager.IsJoining())
            {
                GUI.color = Color.yellow;
                if(GUILayout.Button("Cancel", GUILayout.Width(200), GUILayout.Height(100)))
                {
                    beerPong.Cancel();
                }
                GUI.color = Color.white;
            }
            else
            {
                GUILayout.BeginHorizontal();
                {
                    GUI.color = Color.green;
                    if(GUILayout.Button("Auto Connect", GUILayout.Width(200), GUILayout.Height(120)))
                    {
                        beerPong.AutoConnect();
                    }
                    GUI.color = Color.white;

                    GUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("... OR ...");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    {
                        if(GUILayout.Button("Host", GUILayout.Width(150), GUILayout.Height(60)))
                        {
                            beerPong.StartHosting();
                        }
                        if(GUILayout.Button("Join", GUILayout.Width(150), GUILayout.Height(60)))
                        {
                            beerPong.StartJoining();
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();

            // Debug log
            var style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.yellow;
            style.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label(NetworkDebugString(), style);
        }
    }
}
