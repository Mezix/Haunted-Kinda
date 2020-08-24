using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public List<Gravestone> AllGraves;

    public List<Offering> AllOfferingTypes;

    static bool paused;

    private void Awake()
    {
        Events.current.GraveRobberDespawned += RemoveGraveRobber;
        Events.current.DayIsOver += FinishDay;
    }

    private void Start()
    {
        ui.StartGame();
        SetupDayAndNight();

        GraveRobbers = new List<GameObject>();
        StartCoroutine(SpawnGraveRobbers(6));
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused)
            {
                Time.timeScale = 1;
                paused = false;
            }
            else
            {
                Time.timeScale = 0;
                paused = true;
            }
        }
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
        }
    }

    private IEnumerator SpawnGraveRobbers(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject go = Instantiate(GraveRobberPrefab);
            go.transform.position = GraveRobberPositions[i % GraveRobberPositions.Length].transform.position;
            go.GetComponent<GraveRobber>().InitAllGravestones(AllGraves);
            GraveRobbers.Add(go);
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

    private void RemoveGraveRobber(GameObject graveRobber)
    {
        GraveRobbers.Remove(graveRobber);
        Destroy(graveRobber);

        //if(GraveRobbers.Count == 0)
        //{
        //    VictoryScreen();
        //}
    }

    private void EndOfGame()
    {
        print("VICTORY");
        ui.ShowEndScreen();
        //Loader.Load(Loader.Scene.MainMenuScene);
    }
}
