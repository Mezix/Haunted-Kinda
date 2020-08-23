using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gravestone : MonoBehaviour
{
    [SerializeField]
    private GameObject InhabitedGhost;
    public bool Destroyed;

    public SpriteRenderer GravestoneRenderer;
    public Sprite initialSprite;
    public Sprite [] DestructionStates;

    public float maxHealth;
    public float currentHealth;

    public GameObject OfferingPos;

    private void Awake()
    {
        GravestoneRenderer = GetComponent<SpriteRenderer>();
        initialSprite = GravestoneRenderer.sprite;
    }
    void Start()
    {
        InitMaxHealth(100);
    }
    public void InitMaxHealth(float max)
    {
        maxHealth = max;
        currentHealth = maxHealth;
        CheckDestructionState();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Destroyed = true;
        }
        CheckDestructionState();
    }
    public void Restore(float heal)
    {
        currentHealth += heal;
        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;
            Destroyed = false;
        }
        CheckDestructionState();
    }

    private void CheckDestructionState()
    {
        int index = Mathf.RoundToInt((1f - (currentHealth / maxHealth)) * (DestructionStates.Length - 1));
        GravestoneRenderer.sprite = DestructionStates[index];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.GetComponent<Player>())
        {
            if(InhabitedGhost)
            InhabitedGhost.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Player>())
        {
            if(InhabitedGhost)
            InhabitedGhost.GetComponent<GraveGhost>().FadeAway();
        }
    }
}
