using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private GameObject gameObjectToTrack;
    public GameObject Player;

    private void Awake()
    {
        //Events.current.GraveRobberDespawned += TrackNewObject;
    }
    void Start()
    {
        gameObjectToTrack = Player;
        Events.current.ObjectPossessed += TrackNewObject;
    }

    void Update()
    {
        transform.position = gameObjectToTrack.transform.position;
    }

    void TrackNewObject(GameObject go)
    {
        gameObjectToTrack = go;
    }
}
