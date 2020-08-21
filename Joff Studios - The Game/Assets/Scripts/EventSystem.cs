using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSystem : MonoBehaviour
{
    public static EventSystem current;
    private void Awake()
    {
        current = this;
    }

    //public event Action BattleStartEvent;
    //public event Action<Unit> UnitChangedEvent;
}
