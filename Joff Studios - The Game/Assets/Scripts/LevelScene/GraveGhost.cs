using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveGhost : MonoBehaviour
{
    public Material opacityMat;
    private UnityEngine.Experimental.Rendering.Universal.Light2D GhostGlow;
    private GameObject player;

    private void Awake()
    {
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
        for(int i = 1; i < 98; i++)
        {
            opacityMat.SetFloat("Vector1_D5B527C2", 1.3f + 10f - (float) i/10); //the name of the alpha factor in the shader. (Find property names in the compiled shader code)
            GhostGlow.intensity = (float) i / 100;
            yield return new WaitForSeconds(0.01f);
        }
    }
    public void FadeAway()
    {
        StartCoroutine(FadeOut());
    }
    public IEnumerator FadeOut()
    {
        for (int i = 1; i < 98; i++)
        {
            opacityMat.SetFloat("Vector1_D5B527C2", 1.3f + (float) i/10); //the name of the alpha factor in the shader. (Find property names in the compiled shader code)
            GhostGlow.intensity = 1 - (float)i / 100;
            yield return new WaitForSeconds(0.01f);
        }
        gameObject.SetActive(false);
    }
}
