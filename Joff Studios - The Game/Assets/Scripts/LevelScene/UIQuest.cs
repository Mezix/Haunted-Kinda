using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIQuest : MonoBehaviour
{
    public Text QuestName;
    public GameObject Crossout;
    public GraveGhost assignedGhost;
    private void Awake()
    {
        Crossout.SetActive(false);
    }
}
