using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public GameObject flower;
    private float shrinkProtection;

    private void Awake()
    {
        flower.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        spriteRenderer.flipX = System.Convert.ToBoolean(Random.Range(0, 2));
        shrinkProtection = 60;
        ScoringSystem.instance.flowersPlanted++;
    }
    private void FixedUpdate()
    {
        if(shrinkProtection > 0)
        {
            shrinkProtection -= Time.deltaTime;
            if(shrinkProtection < 0)
            {
                shrinkProtection = 0;
            }
        }
        else if (shrinkProtection == 0)
        {
            Shrink(0.0005f);
        }
    }

    public void Grow(float growthAmount)
    {
        Vector3 scale = flower.transform.localScale;
        if (flower.transform.localScale.x + growthAmount > 1f)
        {
            flower.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            flower.transform.localScale = new Vector3(scale.x + growthAmount, scale.y + growthAmount, scale.z + growthAmount);
        }
        shrinkProtection = 60;
    }
    public void Shrink(float shrinkAmount)
    {
        Vector3 scale = flower.transform.localScale;
        if (flower.transform.localScale.x + shrinkAmount <= 0f)
        {
            flower.transform.localScale = new Vector3(0, 0, 0);
            ScoringSystem.instance.flowersPlanted--;
            Destroy(gameObject);
        }
        else
        {
            flower.transform.localScale = new Vector3(scale.x - shrinkAmount, scale.y - shrinkAmount, scale.z - shrinkAmount);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out Gravestone grave))
        {
            if(grave.currentHappiness < grave.maxGhostHappiness)
            {
                grave.RaiseHappiness(10); //grave ghosts are happier with flowers around
                grave.flowerOverheal = Mathf.Max(0, grave.currentHappiness + 10 - grave.maxGhostHappiness); //carry over the overheal if we exceed the conventional happiness limit
            }
            else
            {
                grave.flowerOverheal += 10;
                if(grave.flowerOverheal > 50)
                {
                    grave.flowerOverheal = 50; //overheal by a max of 50
                }
            }
            if(grave.inhabitedGhost.timesGraveWasDestroyed > 0)
            {
                grave.inhabitedGhost.timesGraveWasDestroyed--; //heal damages from permanent destructions
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        
    }
}
