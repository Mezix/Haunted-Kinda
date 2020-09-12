using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelSceneManager : MonoBehaviour
{
    public static LevelSceneManager level;

    public GameObject playerSpawn;

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

    //TUTORIAL

    public TutorialGhost _tutorialGhost;
    public bool _playTutorial = true;
    public static bool _isPlayingTutorial = false;
    public List<ConversationScriptObj> TutorialConversations;
    public int tutorialIndex = 0;
    public List<Transform> playerTutorialPositions;
    public List<Transform> tutorialRobberPositions;

    public GraveGhost _grandma;
    public GraveGhost _grandpa;
    public GraveGhost _kitty;
    public Gravestone _kittyGrave;

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
        if(_playTutorial)
        {
            StartCoroutine(StartTutorial());
        }
        else
        {
            StartGame();
        }
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
        if(!paused)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                _UIScript.HidePlayerUI();
                DialogueManager.instance.DisplayNextSentence();
            }
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
        if(_graveRobbers is object)
        {
            foreach (GameObject robber in _graveRobbers)
            {
                robber.GetComponent<GraveRobber>().BlockGrave(grave);
            }
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

    //TUTORIAL

    private void StartGame()
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
        GameObject spawnedPlayer = Instantiate(_playerPrefab, playerSpawn.transform.position, playerSpawn.transform.rotation);
        References.Player = spawnedPlayer.gameObject;
        References.playerScript = References.Player.GetComponent<Player>();
        Player playerscript = spawnedPlayer.GetComponent<Player>();
        playerscript._ui = _UIScript;
        playerscript._waterTilemap = _nonCollidableTiles;
        playerscript.positionsToSeekOut = playerTutorialPositions;
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

    //      TUTORIAL STUFF

    private void DisableGraveghostFadein()
    {
        foreach(Gravestone grave in allGraves)
        {
            grave.GetComponentInChildren<GraveGhost>().DistanceFromPlayerToActivate = 0f;
        }
    }
    private void EnableGraveghostFadein()
    {
        foreach (Gravestone grave in allGraves)
        {
            grave.GetComponentInChildren<GraveGhost>().DistanceFromPlayerToActivate = 3f;
        }
    }
    private GraveRobber SpawnTutorialRobber(Transform transform)
    {
        GameObject go = Instantiate(_graveRobberPrefab, transform.position, transform.rotation);
        return go.GetComponent<GraveRobber>();
    }

    private IEnumerator StartTutorial()
    {
        print("starting tutorial");

        _isPlayingTutorial = true;


        //  Setup of Scene for the tutorial

        //  UI
        _UIScript.HidePlayerUI();
        _UIScript.proximityButtonsEnabled = false;
        _UIScript.portraitHidden = true;
        _UIScript.DashMeterHidden = true;
        _UIScript.ScreamMeterHidden = true;
        _UIScript.SundialHidden = true;
        _UIScript.SunDialObjectHidden = true;
        _UIScript.InventoryHidden = true;

        SpawnPlayer();
        SetPlayerReferencesInScene();
        References.playerScript.HidePlayer();
        References.playerScript.LockMovement();
        //lock all our abilities
        References.playerScript._screamLocked = References.playerScript._dashLocked = References.playerScript._possessionLocked
        = References.playerScript._interactionLocked = true;
        DayNightLighting.freezeDayNight = true;
        SetupDayAndNight();

        DisableGraveghostFadein();
        _kittyGrave.InitMaxHealth(100000); //make kitty grave unkillable for tutorial purposes
        _kittyGrave.TakeDamage(50000); //set at half health

         //  First Scene; Zooms In and caretaker talks to you

         StartCoroutine(_UIScript.FadeFromBlack());
        yield return new WaitForSeconds(2f);
        _cameraScript.Zoom(30, 0.75f);
        yield return new WaitForSeconds(1f);
        StartCoroutine(_tutorialGhost.FadeIn());
        yield return new WaitForSeconds(1f);

        //Scene #1: The Wakeup

        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        _UIScript.HidePlayerUI();

        References.playerScript.ShowPlayer();

        //  Scene #2; Player looks around and then talks to the ghost 

        yield return new WaitForSeconds(1.5f);

        //player looks around...
        References.playerScript.lastMovementDir = References.playerScript.movement = new Vector2(-1, -1);
        yield return new WaitForSeconds(0.5f);
        References.playerScript.lastMovementDir = References.playerScript.movement = new Vector2(0, -1);
        yield return new WaitForSeconds(0.5f);
        References.playerScript.lastMovementDir = References.playerScript.movement = new Vector2(1, -1);
        yield return new WaitForSeconds(1f);
        References.playerScript.lastMovementDir = References.playerScript.movement = new Vector2(0, -1);
        yield return new WaitForSeconds(0.1f);
        References.playerScript.lastMovementDir = References.playerScript.movement = new Vector2(-1, 0);
        yield return new WaitForSeconds(0.1f);
        References.playerScript.lastMovementDir = References.playerScript.movement = new Vector2(-1, 1);
        yield return new WaitForSeconds(0.5f);

        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);

        //Scene #3: Move our ghosts to the new location, then give the backstory of our ghost

        _tutorialGhost.MoveToNextPos();
        yield return new WaitForSeconds(1.5f);
        References.playerScript.MoveToNextPos();
        yield return new WaitWhile(() => !References.playerScript.reachedEndOfPath);
        yield return new WaitWhile(() => !_tutorialGhost.reachedEndOfPath);
        References.playerScript.lastMovementDir = References.playerScript.movement = new Vector2(-1, 0); //player looks at tutorial ghost
        yield return new WaitForSeconds(0.5f);

        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        yield return new WaitForSeconds(1f);

        // Scene #4: Tutorial ghost moves to our right to investigate, asks us to follow after

        _tutorialGhost.MoveToNextPos();
        while (!_tutorialGhost.reachedEndOfPath)
        {
            References.playerScript.lastMovementDir = References.playerScript.movement = (_tutorialGhost.transform.position - References.playerScript.transform.position);
            yield return new WaitForFixedUpdate();
        }
        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        _tutorialGhost.MoveToNextPos();
        yield return new WaitForSeconds(1f);
        References.playerScript.MoveToNextPos();
        yield return new WaitWhile(() => !References.playerScript.reachedEndOfPath);
        yield return new WaitWhile(() => !_tutorialGhost.reachedEndOfPath);
        //player look at ghost
        References.playerScript.lastMovementDir = References.playerScript.movement = new Vector2(1, 1);
        yield return new WaitForSeconds(0.2f);

        //  Scene #5: Talk with Grandma and Grandpa about Incense, and the graverobbers
        _grandma.DistanceFromPlayerToActivate = 100;
        _grandpa.DistanceFromPlayerToActivate = 100;
        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        _grandma.DistanceFromPlayerToActivate = 0;
        _grandpa.DistanceFromPlayerToActivate = 0;

        //  Scene #6: Move Player and tutorial to the kitty, and fight the robber!

        _kitty.DistanceFromPlayerToActivate = 100;

        GraveRobber Robber = SpawnTutorialRobber(tutorialRobberPositions[0]);
        Robber.InitRobber(allGraves, _graveRobberEscapePos);
        _tutorialGhost.MoveToNextPos();
        yield return new WaitForSeconds(1f);
        References.playerScript.MoveToNextPos();
        yield return new WaitWhile(() => !References.playerScript.reachedEndOfPath);
        yield return new WaitWhile(() => !_tutorialGhost.reachedEndOfPath);

        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        _UIScript.PromptScream();
        Robber.fearLevel.InitMaxFear(40);
        References.playerScript._screamLocked = false;
        _UIScript.ScreamMeterHidden = false;
        _UIScript.ShowPlayerUI();

        yield return new WaitWhile(() => !Robber.isTerrified);

        float timer = 0f;
        while(timer < 1.5f)
        {
            timer += Time.deltaTime;
            References.playerScript.lastMovementDir = References.playerScript.movement = (Robber.transform.position - References.playerScript.transform.position);
            yield return new WaitForFixedUpdate();
        }
        Destroy(Robber.gameObject);
        References.playerScript.lastMovementDir = References.playerScript.movement = new Vector2(0, 1);
        yield return new WaitForSeconds(0.5f);

        //  Scene #7: Player defeated Robber, now fixes Cats Grave

        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        

        //  Final Scene! Play final dialogue, relinquish the lock on the player and then finally start the game!

        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);

        StartCoroutine(_tutorialGhost.FadeOut());
        yield return new WaitForSeconds(1f);

        _isPlayingTutorial = false;

        _cameraScript.Zoom(15, 0.2f);
        References.playerScript.UnlockMovement();

        //unlock all our abilities
        References.playerScript._screamLocked = References.playerScript._dashLocked = References.playerScript._possessionLocked
            = References.playerScript._interactionLocked = false;
        DayNightLighting.freezeDayNight = false;

        //  Reset UI
        _UIScript.proximityButtonsEnabled = true;
        _UIScript.portraitHidden = false;
        _UIScript.DashMeterHidden = false;
        _UIScript.ScreamMeterHidden = false;
        _UIScript.SundialHidden = false;
        _UIScript.SunDialObjectHidden = false;
        _UIScript.InventoryHidden = false;
        _UIScript.ShowPlayerUI();

        //  Reset graves to default values again
        EnableGraveghostFadein();
        _kittyGrave.InitMaxHealth(100f);
    }
}
