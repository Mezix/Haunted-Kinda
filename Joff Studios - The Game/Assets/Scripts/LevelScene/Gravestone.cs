using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravestone : MonoBehaviour
{
    public bool _destroyed;
    public bool IsBeingTargeted { get; set; }

    public GameObject _dirtParticlesPrefab;
    private GameObject currentDirtParticles;
    public GameObject _dirtPos;

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
            currentHealth = 0;
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

    public void AttackGrave()
    {
        IsBeingTargeted = true;

        if(_dirtParticlesPrefab && _dirtPos)
        {
            currentDirtParticles = Instantiate(_dirtParticlesPrefab, _dirtPos.transform.position, _dirtPos.transform.rotation);
        }
    }
    public void StopAttackingGrave()
    {
        IsBeingTargeted = false;
        if(currentDirtParticles)
        {
            Destroy(currentDirtParticles);
        }
    }
}
