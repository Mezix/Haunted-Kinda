using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelSceneManager : MonoBehaviour
{
    public static LevelSceneManager level;

    public int NightLength = 3;
    public int DayLength = 3;
    public int AmountOfDays = 7;
    private int DaysPassed = 0;

    public DayNightLighting _lighting;
    public UIScript _UIScript;
    public CameraScript _cameraScript;

    public GameObject _playerPrefab;
    public Collider2D _nonCollidableTiles;

    public GameObject GraveRobberParent;
    public GameObject _graveRobberPrefab;
    private List<GameObject> _graveRobbers;
    public GameObject _graveRobberPositionParent;
    private List<GameObject> graveRobberSpawnPositions = new List<GameObject>();
    public GameObject _graveRobberEscapePos;

    public GameObject _graveParent;
    private List<Gravestone> allGraves = new List<Gravestone>();
    private List<Gravestone> blockedGraves = new List<Gravestone>();

    public List<GameObject> _allOfferingTypes;
    public GameObject _offeringsParent;
    public GameObject _offeringPosParent;
    private List<GameObject> offeringSpawnPositions = new List<GameObject>();

    public static bool paused;

    public AudioSource EndingMusic;


    private bool playingConversation;

    private void Awake()
    {
        Events.current.GraveRobberDespawned += RemoveGraveRobber;
        Events.current.DayIsOver += FinishDay;
        Events.current.GravestoneBlocked += BlockGrave;
        Events.current.GravestoneUnblocked += UnblockGrave;

        level = this;

        GetAllGravesInScene();
        GetAllRobberSpawnPositions();
        GetAllOfferingPositions();
    }

    private void Start()
    {
        Unpause();
        _UIScript.StartGame();
        SetupDayAndNight();

        SpawnPlayer();
        SetPlayerReferencesInScene();
        SpawnOfferings();
        _graveRobbers = new List<GameObject>();
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
        if(Input.GetKeyDown(KeyCode.T))
        {
            DialogueManager.instance.DisplayNextSentence();
        }
    }



    //EVENT STUFF



    private void RemoveGraveRobber(GameObject graveRobber)
    {
        _graveRobbers.Remove(graveRobber);
        Destroy(graveRobber);
    }
    private void BlockGrave(Gravestone grave)
    {
        blockedGraves.Add(grave);
        foreach(GameObject robber in _graveRobbers)
        {
            robber.GetComponent<GraveRobber>().BlockGrave(grave);
        }
    }
    private void UnblockGrave(Gravestone grave)
    {
        blockedGraves.Remove(grave);
        foreach (GameObject robber in _graveRobbers)
        {
            robber.GetComponent<GraveRobber>().UnblockGrave(grave);
        }
    }



    //DIALOGUE



    public void TriggerDialogue(ConversationScriptObj convo)
    {
        _UIScript.TurnOnDialogue();
        DialogueManager.instance.StartDialogue(convo);
    }



    //GET REFERENCES TO STUFF IN THE SCENE



    private void GetAllGravesInScene()
    {
        foreach(Gravestone grave in _graveParent.GetComponentsInChildren<Gravestone>())
        {
            allGraves.Add(grave);
        }
    }
    private void GetAllOfferingPositions()
    {
        foreach(Transform offering in _offeringPosParent.GetComponentsInChildren<Transform>())
        {
            offeringSpawnPositions.Add(offering.gameObject);
        }
        offeringSpawnPositions.Remove(_offeringPosParent);
    }
    private void GetAllRobberSpawnPositions()
    {
        foreach(Transform spawnPos in _graveRobberPositionParent.GetComponentsInChildren<Transform>())
        {
            graveRobberSpawnPositions.Add(spawnPos.gameObject);
        }
        graveRobberSpawnPositions.Remove(_graveRobberPositionParent);
    }
    private void SetPlayerReferencesInScene()
    {
        foreach(Gravestone grave in allGraves)
        {
            grave.GetComponentInChildren<GraveGhost>().SetPlayerReference();
        }
        _UIScript.SetPlayerRef();
        _cameraScript.SetPlayerRef();
    }



    //GAME PAUSING



    public void Pause()
    {
        _UIScript.UIPause();
        Time.timeScale = 0;
        paused = true;
    }
    public void Unpause()
    {
        _UIScript.UIUnpause();
        Time.timeScale = 1;
        paused = false;
    }
    void SetupDayAndNight()
    {
        _lighting.DayLength = DayLength;
        _lighting.NightLength = NightLength;

        StartCoroutine(_lighting.Night(_lighting.NightLength));
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
            StartCoroutine(SpawnGraveRobbers(Random.Range(3, 6)));
            print("new day");
            SpawnOfferings();
        }
    }

    //SPAWN STUFF

    private void SpawnPlayer()
    {
        GameObject spawnedPlayer = Instantiate(_playerPrefab);
        References.Player = spawnedPlayer.gameObject;
        Player playerscript = spawnedPlayer.GetComponent<Player>();
        playerscript._ui = _UIScript;
        playerscript._waterTilemap = _nonCollidableTiles;
        Physics2D.IgnoreCollision(_nonCollidableTiles, spawnedPlayer.GetComponent<Collider2D>()); //make sure we dont collide with the water

        _UIScript.SetPlayerRef();
    }

    private void SpawnOfferings()
    {
        for(int i = 0; i < offeringSpawnPositions.Count; i++)
        {
            GameObject go = Instantiate(_allOfferingTypes[Random.Range(0, _allOfferingTypes.Count)]);
            go.transform.position = offeringSpawnPositions[i].transform.position;
            go.transform.parent = _offeringsParent.transform;
            //print("offer");
        }
    }
    private IEnumerator SpawnGraveRobbers(int amount)
    {
        yield return new WaitForSeconds(3);

        for (int i = 0; i < amount; i++)
        {
            SpawnGraveRobber();
            yield return new WaitForSeconds(0.5f);

        }
        for (int i = 0; i < _graveRobbers.Count; i++) //remove all robber collisions
        {
            for (int j = 0; j < _graveRobbers.Count; j++)
            {
                Physics2D.IgnoreCollision(_graveRobbers[i].GetComponent<Collider2D>(), _graveRobbers[j].GetComponent<Collider2D>());
            }
        }
    }

    private void SpawnGraveRobber()
    {
        GameObject go = Instantiate(_graveRobberPrefab);
        go.transform.position = graveRobberSpawnPositions[1].transform.position; //Random.Range(0, graveRobberSpawnPositions.Count
        go.GetComponent<GraveRobber>().InitRobber(allGraves, _graveRobberEscapePos);
        go.transform.parent = GraveRobberParent.transform;
        _graveRobbers.Add(go);
        go.GetComponent<GraveRobber>().blockedGraves = blockedGraves;
    }
    

    private void EndOfGame()
    {
        print("VICTORY");
        EndingMusic.Play();
        _UIScript.ShowEndScreen();
    }

    public void GoToMainMenu()
    {
        Loader.Load(Loader.Scene.MainMenuScene);
    }
}
