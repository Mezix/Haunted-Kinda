using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lootbag : MonoBehaviour
{
    private GameObject Player;
    private bool opened;
    private Animator animator;
    private void Awake()
    {
        Player = GameObject.Find("PlayerCharacter");
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (Vector3.Distance(Player.transform.position, transform.position) < 0.5f && !opened)
        {
            OpenLootbag();
        }
    }

    void OpenLootbag()
    {
        opened = true;
        print("open 'er up");
        animator.SetBool("Opened", true);
    }
}
