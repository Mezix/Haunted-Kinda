using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private GameObject gameObjectToTrack = null;
    private GameObject player;

    private void Awake()
    {
        Events.current.ObjectPossessed += TrackNewObject;
    }

    void Update()
    {
        if(gameObjectToTrack)
        {
            transform.position = gameObjectToTrack.transform.position;
        }
    }

    private void TrackNewObject(GameObject go)
    {
        gameObjectToTrack = go;
    }
    public void SetPlayerRef()
    {
        player = References.Player;
        gameObjectToTrack = player;
    }
}
