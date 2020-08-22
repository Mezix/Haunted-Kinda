using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightLighting : MonoBehaviour
{
    public UnityEngine.Experimental.Rendering.Universal.Light2D RedGlobalLight;
    public UnityEngine.Experimental.Rendering.Universal.Light2D BlueGlobalLight;
    public UnityEngine.Experimental.Rendering.Universal.Light2D WhiteGlobalLight;

    public List<UnityEngine.Experimental.Rendering.Universal.Light2D> Lamps;

    [Range(0,1)]
    public float DayToNightRatio;
    public bool DayToNight; //true -> day to night. False -> night to day

    void Start()
    {
        RedGlobalLight.intensity = 0;
        BlueGlobalLight.intensity = 0;
        WhiteGlobalLight.intensity = 1;

        DayToNightRatio = 0;
        DayToNight = true;
    }

    void Update()
    {
        UpdateRedLight();
        UpdateBlueLight();
        UpdateWhiteLight();

        UpdateAllLamps();
    }

    private void FixedUpdate()
    {
        DayToNightCycle();
    }

    private void DayToNightCycle()
    {
        if(DayToNightRatio <= 0)
        {
            DayToNight = true; //go forwards
        }
        if (DayToNightRatio >= 1)
        {
            DayToNight = false; //go back
        }
        if(DayToNight)
        {
            DayToNightRatio += 0.001f;
        }
        else
        {
            DayToNightRatio -= 0.001f;
        }
    }

    private void UpdateRedLight()
    {
        //at 0.3 DTN ration -> at max value of 0.5
        //after that, slowly go back down

        //at 0.9 DTN -> at 0.1
        //at 1 -> 0

        if(DayToNightRatio == 0)
        {
            RedGlobalLight.intensity = 0;
        }
        else if(DayToNightRatio <= 0.3f)
        {
            RedGlobalLight.intensity = 0.5f *  (DayToNightRatio/0.3f); //make sure daytonight isnt 0 -> so we dont get infinity!
            //print("test");
        }
        else
        {
            RedGlobalLight.intensity = 0.5f *  (1 - ((DayToNightRatio - 0.3f) / 0.7f));
        }
    }
    private void UpdateBlueLight()
    {
        BlueGlobalLight.intensity = DayToNightRatio;
    }

    private void UpdateWhiteLight()
    {
        WhiteGlobalLight.intensity = 1 - DayToNightRatio;
    }
    private void UpdateAllLamps()
    {
        foreach(UnityEngine.Experimental.Rendering.Universal.Light2D lamp in Lamps)
        {
            lamp.intensity = DayToNightRatio;
            lamp.pointLightOuterRadius = 2.5f + 2.5f * DayToNightRatio;
        }
    }

}
