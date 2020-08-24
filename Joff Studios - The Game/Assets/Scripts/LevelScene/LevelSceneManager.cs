using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelSceneManager : MonoBehaviour
{
    public int NightLength = 3;
    public int DayLength = 3;
    public int AmountOfDays = 7;
    private int DaysPassed = 0;

    public DayNightLighting Lighting;
    public UI ui;

    public GameObject GraveRobberPrefab;
    public List<GameObject> GraveRobbers;
    public GameObject [] GraveRobberPositions;
    public GameObject GraveRobberEscapePos;
    public GameObject GraveRobberParent;

    public List<Gravestone> AllGraves;

    public List<GameObject> AllOfferingTypes;
    public List<GameObject> OfferingSpawnPositions;
    public GameObject OfferingsParent;

    public static bool paused;

    public AudioSource EndingMusic;

    private void Awake()
    {
        Events.current.GraveRobberDespawned += RemoveGraveRobber;
        Events.current.DayIsOver += FinishDay;
    }

    private void Start()
    {
        Unpause();

        ui.StartGame();
        SetupDayAndNight();

        GraveRobbers = new List<GameObject>();
        SpawnOfferings();
        StartCoroutine(SpawnGraveRobbers(6));
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused)
            {
                Unpause();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        ui.UIPause();
        Time.timeScale = 0;
        paused = true;
    }
    public void Unpause()
    {
        ui.UIUnpause();
        Time.timeScale = 1;
        paused = false;
    }

    void SetupDayAndNight()
    {
        Lighting.DayLength = DayLength;
        Lighting.NightLength = NightLength;

        StartCoroutine(Lighting.Night(Lighting.NightLength));
    }
    private void FinishDay()
    {
        DaysPassed++;
        if(DaysPassed >= AmountOfDays) //week over => game over
        {
            EndOfGame();
        }
        else //start a new day
        {
            SetupDayAndNight();
            SpawnGraveRobbers(Random.Range(3, 6));
        }
    }

    private void SpawnOfferings()
    {
        
        for(int i = 0; i < OfferingSpawnPositions.Count; i++)
        {
            GameObject go = Instantiate(AllOfferingTypes[Random.Range(0, AllOfferingTypes.Count)]);
            go.transform.position = OfferingSpawnPositions[i].transform.position;
            go.transform.parent = OfferingsParent.transform;
            print("offer");
        }
    }
    
    private IEnumerator SpawnGraveRobbers(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            SpawnGraveRobber();
            yield return new WaitForSeconds(0.5f);

        }
        for (int i = 0; i < GraveRobbers.Count; i++) //remove all robber collisions
        {
            for (int j = 0; j < GraveRobbers.Count; j++)
            {
                Physics2D.IgnoreCollision(GraveRobbers[i].GetComponent<Collider2D>(), GraveRobbers[j].GetComponent<Collider2D>());
            }
        }
    }

    private void SpawnGraveRobber()
    {
        GameObject go = Instantiate(GraveRobberPrefab);
        go.transform.position = GraveRobberPositions[Random.Range(0, GraveRobberPositions.Length)].transform.position;
        go.GetComponent<GraveRobber>().Init(AllGraves, GraveRobberEscapePos);
        go.transform.parent = GraveRobberParent.transform;
        GraveRobbers.Add(go);
    }
    private void RemoveGraveRobber(GameObject graveRobber)
    {
        GraveRobbers.Remove(graveRobber);
        Destroy(graveRobber);
    }

    private void EndOfGame()
    {
        print("VICTORY");
        EndingMusic.Play();
        ui.ShowEndScreen();
    }

    public void GoToMainMenu()
    {
        Loader.Load(Loader.Scene.MainMenuScene);
    }
}
