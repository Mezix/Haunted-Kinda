using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSceneManager : MonoBehaviour
{
    public GameObject GraveRobberPrefab;
    public List<GameObject> GraveRobbers;

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
        for(int i = 0; i < amount; i++)
        {
            GameObject go = Instantiate(GraveRobberPrefab);
            GraveRobbers.Add(go);
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
