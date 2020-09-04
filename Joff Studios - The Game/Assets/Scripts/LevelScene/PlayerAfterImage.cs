using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAfterImage : MonoBehaviour
{
    private Player player;
    private SpriteRenderer playerRenderer;
    [SerializeField]
    private SpriteRenderer afterImageRenderer;

    public float afterImageTime;

    private void OnEnable()
    {
        player = References.Player.GetComponent<Player>();
        playerRenderer = player.GetComponent<SpriteRenderer>();

        afterImageRenderer.sprite = playerRenderer.sprite;

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
