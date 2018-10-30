using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;

public class BeerPongARLocationSync : MonoBehaviour {

    public GameObject statusGO;
    public Text statusText;

    ARTrackingState _arTrackingState;
    ARTrackingStateReason _arTrackingStateReason;

    void OnEnable()
    {
        statusGO.SetActive(false);
        UnityARSessionNativeInterface.ARSessionTrackingChangedEvent += TrackingChanged;
    }

    void OnDisable()
    {
        UnityARSessionNativeInterface.ARSessionTrackingChangedEvent -= TrackingChanged;
    }

    void TrackingChanged(UnityARCamera camera)
    {
        _arTrackingState = camera.trackingState;
        _arTrackingStateReason = camera.trackingReason;
    }

    public IEnumerator Relocate(byte[] receivedBytes)
    {
        //Start relocation
        statusGO.SetActive(true);
        statusText.text = "Start relocalize..";
        ARWorldMap arWorldMap = ARWorldMap.SerializeFromByteArray(receivedBytes);

        //Use the AR Session manager to restart session with received world map to sync up
        BP_ARSessionManager beerPongARSessionManager = FindObjectOfType<BP_ARSessionManager>();
        beerPongARSessionManager.StartSession(arWorldMap);

        //Check tracking state and update UI
        while(_arTrackingState != ARTrackingState.ARTrackingStateLimited || _arTrackingStateReason != ARTrackingStateReason.ARTrackingStateReasonRelocalizing)
        {
            yield return null; //wait until it starts relocalizing
        }
        statusText.text = "Relocalizing... look around the area";

        while(_arTrackingState != ARTrackingState.ARTrackingStateNormal)
        {
            yield return null;
        }

        statusText.text = "Relocalized!";
        yield return null;
        statusGO.SetActive(false);
        yield return null;
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
