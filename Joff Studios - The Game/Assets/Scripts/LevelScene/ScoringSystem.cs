using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoringSystem : MonoBehaviour
{
    [SerializeField]
    private List<GraveGhost> AllGraveGhosts;

    public void InitAllGraves(List<Gravestone> graves)
    {
        foreach(Gravestone grave in graves)
        {
            AllGraveGhosts.Add(grave.GetComponentInChildren<GraveGhost>());
        }
    }
}
