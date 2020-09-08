using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelViewer : MonoBehaviour
{
    public RenderTexture pixelFilter;

        void Start()
    {
        int realRatio = Mathf.RoundToInt(Screen.width / Screen.width);
        pixelFilter.width = NearestSuperiorPowerOf2(Mathf.RoundToInt(pixelFilter.width * realRatio));
    }

   
    void Update()
    {
        GUI.depth = 20;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), pixelFilter);
    }

    int NearestSuperiorPowerOf2(int n)
    {
        return (int)Mathf.Pow(2, Mathf.Ceil(Mathf.Log(n) / Mathf.Log(2)));
    }
}
