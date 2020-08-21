using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this line of code allows debugging in edit mode == without running the game!
 // [ExecuteInEditMode]
public class PixelateImageEffect : MonoBehaviour
{

    public Material effectMaterial;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, effectMaterial);
    }
}
