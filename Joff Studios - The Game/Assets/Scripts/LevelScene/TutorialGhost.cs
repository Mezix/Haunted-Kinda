using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialGhost : MonoBehaviour
{
    private UnityEngine.Experimental.Rendering.Universal.Light2D GhostGlow; //the glow of our ghost
    [SerializeField]
    private GameObject player; //reference to the player in the scene
    private SpriteRenderer ghostRenderer; //the sprite rendered of our ghost
    private float ghostSpriteOpacity; //the current opacity of our ghost
    private float ghostSpriteMaxOpacity; //the max opacity our ghost should achieve
    private float fadeAmount; //the amount to change our opacity every frame

    //CONVERSATIONS

    public ConversationScriptObj[] _conversations;
    public int _conversationIndex;

    private void Awake()
    {
        GhostGlow = GetComponentInChildren<UnityEngine.Experimental.Rendering.Universal.Light2D>();
        ghostRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    private void Start()
    {
        ghostSpriteOpacity = 0; //set our ghost to be hidden by default
        ghostSpriteMaxOpacity = 0.85f; //our ghost should always be slightly opaque, so keep this value below 0
        ghostRenderer.color = new Color(1, 1, 1, ghostSpriteOpacity); //apply the ghostopacity value
        GhostGlow.intensity = 0;
        fadeAmount = 0.05f;
        _conversationIndex = 0;
    }

    public IEnumerator FadeIn()
    {
        while(ghostSpriteOpacity < ghostSpriteMaxOpacity)
        {
            yield return new WaitForFixedUpdate();
            ghostSpriteOpacity += fadeAmount;
            ghostRenderer.color = new Color(1, 1, 1, ghostSpriteOpacity);
            GhostGlow.intensity = ghostSpriteOpacity / ghostSpriteMaxOpacity; //swing between 0 and 1, depending on our fade
        }
        ghostSpriteOpacity = ghostSpriteMaxOpacity;
    }
    public IEnumerator FadeOut()
    {
        while(ghostSpriteOpacity > 0)
        {
            yield return new WaitForFixedUpdate();
            ghostSpriteOpacity -= fadeAmount;
            ghostRenderer.color = new Color(1, 1, 1, ghostSpriteOpacity);
            GhostGlow.intensity = Mathf.Max(0, ghostSpriteOpacity / ghostSpriteMaxOpacity);
        }
        ghostSpriteOpacity = 0;
        GhostGlow.intensity = 0; //this value becomes negative if we dont set it to 0
    }
}
