using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FearLevel : MonoBehaviour
{
    public float maxFear;
    public float currentFear;
    public HealthBarScript health;

    private void Awake()
    {
        health = GetComponentInChildren<HealthBarScript>();
    }
    public void InitMaxFear(float max)
    {
        maxFear = max;
        currentFear = 0;
    }
    public bool AddFear(float fear)
    {
        health.healthbarbackground.gameObject.SetActive(true);
        currentFear = Mathf.Min(maxFear, currentFear + fear);
        health.HandleHealthChange(currentFear / maxFear);
        if (currentFear >= maxFear)
        {
            currentFear = maxFear;
            return true;
        }
        return false;
    }
    public void ReduceFear(float fear)
    {
        if(currentFear != 0)
        {
            currentFear = Mathf.Max(0, currentFear - fear);
            health.HandleHealthChange(currentFear / maxFear);
        }
        else
        {
            health.healthbarbackground.gameObject.SetActive(false);
        }
    }
}
