using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveGhost : MonoBehaviour
{
    public Material opacityMat;
    private UnityEngine.Experimental.Rendering.Universal.Light2D GhostGlow;
    private GameObject player;
    public float defaultOpacity = 11;

    private bool fadingIn;

    private void Awake()
    {
        opacityMat.SetFloat("Vector1_D5B527C2", defaultOpacity);
        player = GameObject.Find("PlayerCharacter");
        GhostGlow = GetComponentInChildren<UnityEngine.Experimental.Rendering.Universal.Light2D>();
        gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        GhostGlow.intensity = 0;
        StartCoroutine(FadeIn());
    }
    private IEnumerator FadeIn()
    {
        fadingIn = true;
        for(int i = 1; i < 98; i++)
        {
            opacityMat.SetFloat("Vector1_D5B527C2", 1.3f + 10f - (float) i/10); //the name of the alpha factor in the shader. (Find property names in the compiled shader code)
            GhostGlow.intensity = (float) i / 100;
            yield return new WaitForEndOfFrame();
        }
        fadingIn = false;
    }
    public void FadeAway()
    {
        if(gameObject.activeSelf)
        StartCoroutine(FadeOut());
    }
    public IEnumerator FadeOut()
    {
        yield return new WaitWhile(() => fadingIn);
        for (int i = 1; i < 98; i++)
        {
            opacityMat.SetFloat("Vector1_D5B527C2", 1.3f + (float) i/10); //the name of the alpha factor in the shader. (Find property names in the compiled shader code)
            GhostGlow.intensity = 1 - (float)i / 100;
            yield return new WaitForEndOfFrame();
        }
        gameObject.SetActive(false);
    }
}
