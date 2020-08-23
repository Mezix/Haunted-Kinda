using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    [SerializeField]
    private Image healthbar;
    public Image healthbarbackground;
    private void Awake()
    {
        //Events.current.OnHealthPctChanged += HandleHealthChange;
    }

    public void HandleHealthChange(float health)
    {
        healthbar.fillAmount = health;
    }
}
