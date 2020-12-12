using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    [SerializeField]
    public Image healthbar;
    public Image healthbarbackground;

    public float lastHealth;

    public AudioSource fullHealthSound;
    public AudioSource healSound;
    public void HandleHealthChange(float health)
    {
        healthbar.fillAmount = health;
        if (healthbar.fillAmount == 1f && lastHealth < 1f)
        {
            StartCoroutine(FlashFullHealth());
        }
        lastHealth = health;
    }

    public IEnumerator FlashFullHealth()
    {
        fullHealthSound.Play();
        healthbarbackground.color = new Color(237/255f,182/255f,147/255f,1);
        for(int i = 0; i < 10; i++)
        {
            healthbarbackground.transform.localScale = new Vector3(1 + i/10f, 1 + i / 10f, 1 + i / 10f);
            yield return new WaitForSeconds(0.05f);

            healthbarbackground.color = new Vector4(healthbarbackground.color.r, healthbarbackground.color.g, healthbarbackground.color.b, 1-i/10f);
        }
        healthbarbackground.transform.localScale = Vector3.one;
        healthbarbackground.color = Vector4.one;
    }
}
