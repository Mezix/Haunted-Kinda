using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer afterImageRenderer;

    public float afterImageTime;

    private void Awake()
    {
        afterImageTime = 0.5f;
    }
    public void SetAndFadeAfterimage(SpriteRenderer objRenderer)
    {
        afterImageRenderer.sprite = objRenderer.sprite;
        StartCoroutine(FadeSprite());
    }

    private IEnumerator FadeSprite()
    {
        float timer = 0;
        while(timer <= afterImageTime)
        {
            timer += Time.deltaTime;
            afterImageRenderer.color = new Color(1,1,1, Mathf.Max(0, 1 - timer/afterImageTime));
            yield return new WaitForFixedUpdate();
        }
        Destroy(gameObject);
    }
}
