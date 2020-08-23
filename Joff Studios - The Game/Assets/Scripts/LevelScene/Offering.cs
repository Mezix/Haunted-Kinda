using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Offering : MonoBehaviour
{
    public Sprite[] FadeAnimation;
    private SpriteRenderer sprRend;
    public bool disappearing;

    public float HealAmount;
    private void Awake()
    {
        sprRend = GetComponent<SpriteRenderer>();
    }
    public void FadeAway()
    {
        disappearing = true;
        StartCoroutine(PlayFadeAnimation());
    }
    private IEnumerator PlayFadeAnimation()
    {
        for(int i = 0; i < FadeAnimation.Length; i++)
        {
            sprRend.sprite = FadeAnimation[i];
            yield return new WaitForSeconds(0.5f);
        }
        Destroy(gameObject);
    }
}
