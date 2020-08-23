using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSceneManager : MonoBehaviour
{
    public GameObject GraveRobberPrefab;
    public List<GameObject> GraveRobbers;

    public List<Gravestone> AllGraves;

    private void Awake()
    {
        Events.current.GraveRobberDespawned += RemoveGraveRobber;
    }
    private void Start()
    {
        GraveRobbers = new List<GameObject>();
        SpawnGraveRobbers(3);
    }
    private void SpawnGraveRobbers(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject go = Instantiate(GraveRobberPrefab);
            go.transform.position += new Vector3(-i, 2 * i,0);
            go.GetComponent<GraveRobber>().InitAllGravestones(AllGraves);
            GraveRobbers.Add(go);
        }
        for (int i = 0; i < GraveRobbers.Count; i++) //remove all robber collisions
        {
            for (int j = 0; j < GraveRobbers.Count; j++)
            {
                Physics2D.IgnoreCollision(GraveRobbers[i].GetComponent<Collider2D>(), GraveRobbers[j].GetComponent<Collider2D>());
            }
        }
    }

    private void RemoveGraveRobber(GameObject graveRobber)
    {
        GraveRobbers.Remove(graveRobber);
        Destroy(graveRobber);
        if(GraveRobbers.Count == 0)
        {
            VictoryScreen();
        }
    }

    private void VictoryScreen()
    {
        Loader.Load(Loader.Scene.MainMenuScene);
    }
}
