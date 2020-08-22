using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Events : MonoBehaviour
{
    public static Events current;
    private void Awake()
    {
        current = this;
    }

    public event Action<float> OnHealthPctChanged;
    public event Action<GameObject> GraveRobberDespawned;

    public void ChangeHealth(float healthChange)
    {
        if (OnHealthPctChanged != null)
        {
            OnHealthPctChanged(healthChange);
        }
    }

    public void DespawnGraveRobber(GameObject graverobber)
    {
        if (GraveRobberDespawned != null)
        {
            GraveRobberDespawned(graverobber);
        }
    }
}
