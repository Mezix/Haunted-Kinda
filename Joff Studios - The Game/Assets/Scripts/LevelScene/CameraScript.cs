using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.Universal;

public class CameraScript : MonoBehaviour
{
    private GameObject gameObjectToTrack = null;
    private GameObject player;
    private PixelPerfectCamera pixelPerfect;

    private int defaultZoom;

    private void Awake()
    {
        Events.current.ObjectPossessed += TrackNewObject;
        pixelPerfect = GetComponentInChildren<PixelPerfectCamera>();
    }
    private void Start()
    {
        defaultZoom = 10;
        pixelPerfect.assetsPPU = defaultZoom;
    }
    void Update()
    {
        if(gameObjectToTrack)
        {
            transform.position = gameObjectToTrack.transform.position;
        }
    }

    public void TrackNewObject(GameObject go)
    {
        gameObjectToTrack = go;
    }
    public void SetPlayerRef()
    {
        player = References.Player;
        gameObjectToTrack = player;
    }
    public void Zoom(int value, float timeToZoom)
    {
        StopCoroutine("ZoomSlowly");
        StartCoroutine(ZoomSlowly(value, timeToZoom));
    }
    private IEnumerator ZoomSlowly(int zoomValue, float timeToZoom)
    {
        int currentValue = pixelPerfect.assetsPPU;
        float timer = 0f;
        while(timer < timeToZoom)
        {
            timer += Time.deltaTime;
            pixelPerfect.assetsPPU = currentValue + Mathf.RoundToInt((timer / timeToZoom) * (zoomValue - currentValue));
            yield return new WaitForFixedUpdate();
        }
        pixelPerfect.assetsPPU = zoomValue;
    }
}
