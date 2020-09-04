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
    public event Action<GameObject> ObjectPossessed;
    public event Action DayIsOver;
    public event Action<Gravestone> GravestoneBlocked;
    public event Action<Gravestone> GravestoneUnblocked;

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
    public void PossessObject(GameObject obj)
    {
        if (ObjectPossessed != null)
        {
            ObjectPossessed(obj);
        }
    }
    public void DayOver()
    {
        if(DayIsOver != null)
        {
            DayIsOver();
        }
    }
    public void BlockGravestone(Gravestone grave)
    {
        if (GravestoneBlocked != null)
        {
            GravestoneBlocked(grave);
        }
    }
    public void UnblockGravestone(Gravestone grave)
    {
        if (GravestoneUnblocked != null)
        {
            GravestoneUnblocked(grave);
        }
    }
}
