using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public GameObject flower;

    private void Awake()
    {
        flower.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        spriteRenderer.flipX = System.Convert.ToBoolean(Random.Range(0, 2));
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
    }
    public void Shrink(float shrinkAmount)
    {
        Vector3 scale = flower.transform.localScale;
        if (flower.transform.localScale.x + shrinkAmount > 1f)
        {
            flower.transform.localScale = new Vector3(0, 0, 0);
        }
        else
        {
            flower.transform.localScale = new Vector3(scale.x - shrinkAmount, scale.y - shrinkAmount, scale.z - shrinkAmount);
        }
    }
}
