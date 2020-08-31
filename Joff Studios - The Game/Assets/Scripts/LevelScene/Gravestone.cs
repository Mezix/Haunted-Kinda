using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravestone : MonoBehaviour
{
    [SerializeField]
    private GameObject InhabitedGhost;
    public bool _destroyed;
    public bool _isBeingAttacked;

    public SpriteRenderer GravestoneRenderer;
    public Sprite initialSprite;
    public Sprite [] DestructionStates;

    public float maxHealth;
    public float currentHealth;

    public GameObject OfferingPos;
    public Offering currentOffering;

    private AudioSource Healing;

    private void Awake()
    {
        GravestoneRenderer = GetComponent<SpriteRenderer>();
        initialSprite = GravestoneRenderer.sprite;
        Healing = GetComponent<AudioSource>();
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
            _destroyed = true;
        }
        CheckDestructionState();
    }
    public void Restore(float heal)
    {
        Healing.Play();
        currentHealth += heal;
        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;
            _destroyed = false;
        }
        CheckDestructionState();
    }

    private void CheckDestructionState()
    {
        int index = Mathf.RoundToInt((1f - (currentHealth / maxHealth)) * (DestructionStates.Length - 1));
        GravestoneRenderer.sprite = DestructionStates[index];
    }
}
