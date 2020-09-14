using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravestone : MonoBehaviour
{
    public GraveGhost inhabitedGhost { get; private set; }
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

    public float maxGhostHappiness;
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

    }
    private void Update()
    {
        timeSinceLastHealSound += Time.deltaTime;
    }
    void Start()
    {
        InitMaxHealth(100);
        InitHappiness(250f);
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
        if(timeSinceLastHealSound >= 2f)
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
        currentHappiness -= value;
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
