using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravestone : MonoBehaviour
{
    public GraveGhost inhabitedGhost { get; private set; }
    public bool _destroyed;
    public bool IsBeingTargeted { get; set; }

    public GameObject _dirtParticles;
    public GameObject _graveRestoreParticles;

    public SpriteRenderer GravestoneRenderer;
    public Sprite initialSprite;
    public Sprite [] DestructionStates;

    public float maxHealth;
    public float currentHealth;

    public float maxGhostHappiness;
    public float flowerOverheal;
    public float currentHappiness;

    public GameObject OfferingPos;
    public Offering currentOffering;

    private AudioSource Healing;
    [SerializeField]
    private float timeSinceLastHealSound = 1f;

    private void Awake()
    {
        GravestoneRenderer = GetComponent<SpriteRenderer>();
        initialSprite = GravestoneRenderer.sprite;
        Healing = GetComponent<AudioSource>();
        inhabitedGhost = GetComponentInChildren<GraveGhost>();

        if(_graveRestoreParticles)
        {
            _graveRestoreParticles.SetActive(false);
        }
        if(_dirtParticles)
        {
            _dirtParticles.SetActive(false);
        }
    }
    void Start()
    {
        InitMaxHealth(100);
        InitHappiness(250f);
        flowerOverheal = 0;
        inhabitedGhost.grave = this;
    }
    private void Update()
    {
        timeSinceLastHealSound += Time.deltaTime;
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
            inhabitedGhost.timesGraveWasDestroyed++;
            currentHealth = 0;
        }
        CheckDestructionState();
    }
    public void RestoreGrave(float heal)
    {
        _graveRestoreParticles.SetActive(true);
        if (timeSinceLastHealSound >= 2f)
        {
            Healing.Play();
            timeSinceLastHealSound = 0f;
        }
        currentHealth += heal;
        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;
            _destroyed = false;
            RaiseHappiness(30f);
            _graveRestoreParticles.SetActive(false);
        }
        CheckDestructionState();
    }
    private void CheckDestructionState()
    {
        int index = Mathf.RoundToInt((1f - (currentHealth / maxHealth)) * (DestructionStates.Length - 1));
        GravestoneRenderer.sprite = DestructionStates[index];
    }
    public void RaiseHappiness(float value)
    {
        currentHappiness += value;
        if(currentHappiness > maxGhostHappiness)
        {
            currentHappiness = maxGhostHappiness;
        }
        inhabitedGhost.happiness.HandleHealthChange(currentHappiness / maxGhostHappiness);
    }
    public void LowerHappiness(float value)
    {
        float val = flowerOverheal - value;
        currentHappiness -= flowerOverheal;
        if(currentHappiness <= 0)
        {
            currentHappiness = 0;
        }
        inhabitedGhost.happiness.HandleHealthChange(currentHappiness / maxGhostHappiness);
    }
    public void InitHappiness(float maxValue)
    {
        if(inhabitedGhost.happiness)
        {
            maxGhostHappiness = maxValue;
            currentHappiness = maxGhostHappiness / 2;
            inhabitedGhost.happiness.HandleHealthChange(currentHappiness / maxGhostHappiness);
        }
    }
    
    public void AttackGrave()
    {
        IsBeingTargeted = true;

        if(_dirtParticles)
        {
            _dirtParticles.SetActive(true);
        }
    }
    public void StopAttackingGrave()
    {
        IsBeingTargeted = false;
        if(_dirtParticles)
        {
            _dirtParticles.SetActive(false);
        }
    }
}
