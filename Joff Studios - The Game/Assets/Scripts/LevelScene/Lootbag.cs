using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lootbag : MonoBehaviour
{
    private GameObject Player;
    private bool opened;
    private Animator animator;

    public GameObject lootPrefab;
    public GameObject lootPos;
    private void Awake()
    {
        Player = GameObject.Find("PlayerCharacter");
        animator = GetComponentInChildren<Animator>();
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
        StartCoroutine(SpawnLoot());
    }

    IEnumerator SpawnLoot()
    {
        yield return new WaitForSeconds(0.5f);
        Instantiate(lootPrefab, lootPos.transform.position, transform.rotation);
    }
}
