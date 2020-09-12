using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lootbag : MonoBehaviour
{
    [SerializeField]
    private GameObject Player;
    private bool opened;
    private Animator animator;

    public GameObject lootPrefab;
    public GameObject lootPos;
    private void Awake()
    {
        Player = References.Player;
        animator = GetComponentInChildren<Animator>();
    }
    void Update()
    {
        if (Vector2.Distance(Player.transform.position, transform.position) < 0.75f && !opened)
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
        yield return new WaitForSeconds(0.3f);
        Instantiate(lootPrefab, lootPos.transform.position, transform.rotation);
        StartCoroutine(Fade());

    }
    IEnumerator Fade()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
        SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();
        /*for(int i = 10; i >= 0; i++)
        {
            sprite.color = new Color(0, 0, 0, (float) i /10);
            yield return new WaitForSeconds(0.1f);
        }*/
    }
}
