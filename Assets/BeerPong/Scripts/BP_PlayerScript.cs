using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

public class BeerPongMessage : MessageBase
{
    public byte[] byteBuffer;
}

public class BP_PlayerScript : BeerPongPlayer {

    public Image image;
    public Text nameField;
    public Text readyField;

    [SyncVar]
    public Color myColor;

    [SyncVar]
    public bool locationSynced;

    public GameObject spherePrefab;
    private byte[] savedBytes;
    private bool locationSent;
    private BeerPongARLocationSync _beerPongLocationSync;
    private BP_ARSessionManager _beerPongARSessionManager;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        _beerPongLocationSync = GetComponent<BeerPongARLocationSync>();
        _beerPongARSessionManager = FindObjectOfType<BP_ARSessionManager>();

        //Send Custome player info
        // This is an example of sending additional information to the server that might be needed in the lobby (eg. color, player avatar, personal settings, etc.)
        myColor = UnityEngine.Random.ColorHSV(0, 1, 1, 1, 1, 1);
        CmdSetCustomePlayerInfo(myColor);
        locationSent = false;
    }

    public IEnumerator RelocateDevice(byte[] receivedBytes)
    {
        yield return null;
        //actually sync up using arRelocator
        yield return _beerPongLocationSync.Relocate(receivedBytes);
        CmdSetLocationSynced();
        yield return null;
    }

    [Command]
    public void CmdSetLocationSynced()
    {
        locationSynced = true;
    }

    [Command]
    public void CmdSetCustomePlayerInfo(Color aColor)
    {
        myColor = aColor;
    }

    [Command]
    public void CmdMakeSphere(Vector3 position, Quaternion rotation)
    {
        var sphere = (GameObject)Instantiate(spherePrefab, position, rotation);
        NetworkServer.Spawn(sphere);
        RpcSetSphereColor(sphere, myColor.r, myColor.g, myColor.b);
    }

    [Command]
    public void CmdPlayAgain()
    {
        BP_GameSession.instance.PlayAgain();
    }

    public override void OnClientEnterLobby()
    {
        base.OnClientEnterLobby();
        //Briefly delay to let SyncVars propagate
        Invoke("ShowPlayer", 0.5f);
    }

    public override void OnClientReady(bool readyState)
    {
        if(readyState)
        {
            readyField.text = "Ready!";
            readyField.color = Color.green;
        }
        else
        {
            readyField.text = "Not Ready!";
            readyField.color = Color.red; 
        }
    }

    void ShowPlayer()
    {
        transform.SetParent(GameObject.Find("Canvas/PlayerContainer").transform, false);

        image.color = myColor;
        nameField.text = deviceName;
        readyField.gameObject.SetActive(true);
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        string synced = locationSynced ? "SYNC" : "NO";
	}

    [ClientRpc]
    public void RpcSetSphereColor(GameObject sphere, float r, float g, float b)
    {
        sphere.GetComponent<Renderer>().material.color = new Color(r, g, b);
    }

    [ClientRpc]
    public void RpcOnStartedGame()
    {
        readyField.gameObject.SetActive(false);
    }

    void OnGUI()
    {
        if(isLocalPlayer)
        {
            GUILayout.BeginArea(new Rect(0, Screen.height * 0.8f, Screen.width, 100));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            BP_GameSession gameSession = BP_GameSession.instance;
            if(gameSession)
            {
                if(gameSession.gameState == BP_GameState.Lobby || gameSession.gameState == BP_GameState.Countdown)
                {
                    if(GUILayout.Button(IsReady() ? "Not Ready" : "Ready", GUILayout.Width(Screen.width * 0.3f), GUILayout.Height(100)))
                    {
                        if(IsReady())
                        {
                            SendNotReadyToBeginMessage();
                        }
                        else
                        {
                            SendReadyToBeginMessage();
                        }
                    }
                }
                else if(gameSession.gameState == BP_GameState.WaitForLocationSync)
                {
                    if(isServer && !locationSent)
                    {
                        gameSession.CmdSendWorldMap();
                        locationSent = true;
                    }
                }//Change name of GameState WaitingForRolls Once started BeerPong player logic
                else if(gameSession.gameState == BP_GameState.WaitingForRolls)
                {
                    if(GUILayout.Button("Make Sphere", GUILayout.Width(Screen.width * 0.6f), GUILayout.Height(100))){
                        Transform cameraTransform = _beerPongARSessionManager.CameraTransform();
                        Vector3 spherePosition = cameraTransform.position + (cameraTransform.forward.normalized * 0.2f); //place sphere 2cm in front of device
                        CmdMakeSphere(spherePosition, cameraTransform.rotation);
                    }
                }
                else if(gameSession.gameState == BP_GameState.GameOver)
                {
                    if(isServer)
                    {
                        if(GUILayout.Button("Play Again", GUILayout.Width(Screen.width * 0.3f), GUILayout.Height(100)))
                        {
                            CmdPlayAgain();
                        }
                    }
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }    
    }
}
