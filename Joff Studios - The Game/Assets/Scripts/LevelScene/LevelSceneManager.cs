using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

using UnityEngine.Rendering;

public class LevelSceneManager : MonoBehaviour
{
    public static LevelSceneManager instance;

    public GameObject playerSpawn;

    public int HalfNightLength;
    public int HalfDayLength;
    public int AmountOfDays;
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
    public ScoringSystem score;

    public float timeSinceLastDialogueStarted = 0f;
    public float timeUntilNextDialogueCanBeStarted = 0.25f;

    public AudioSource EndingMusic;

    //DIFFICULTY

    [Range(0, 3)]
    public int DifficultySlider;
    private int RobbersPerSpawn = 3;
    private float TimeSinceLastRobbers = 10f;

    //TUTORIAL

    public TutorialGhost _tutorialGhost;
    public bool _playTutorial = true;
    public static bool _isPlayingTutorial = false;
    public List<ConversationScriptObj> TutorialConversations;
    public int tutorialIndex = 0;
    public List<Transform> playerTutorialPositions;
    public List<Transform> tutorialRobberPositions;
    public Transform tutorialOffering;

    public GraveGhost _grandma;
    public GraveGhost _grandpa;
    public GraveGhost _kitty;
    public Gravestone _kittyGrave;
    public GraveGhost _coolGhost;
    public Gravestone _coolGrave;

    //ENDING

    public TutorialGhost _endingGhost;
    public List<ConversationScriptObj> EndingConversations;
    public int EndingConversationIndex = 0;

    //DAD AND CAT QUEST MISC

    public static GameObject catGhostQuestItem;

    //MISC

    public Shader Outline;
    public RenderPipelineAsset Pipeline2D;
    public RenderPipelineAsset Pipeline3D;
    public GameObject LoadingScreen;

    private void Awake()
    {
        LoadingScreen.SetActive(false);
        GraphicsSettings.renderPipelineAsset = Pipeline2D;
        QualitySettings.renderPipeline = Pipeline2D;

        _playTutorial = MenuSceneManager.playTutorial;
        DifficultySlider = MenuSceneSettings._difficulty;

        SetDifficulty();

        MenuSceneManager.playTutorial = false;
        DialogueManager.instance.level = this;

        Events.current.GraveRobberDespawned += RemoveGraveRobber;
        Events.current.DayIsOver += FinishDay;
        Events.current.GravestoneBlocked += BlockGrave;
        Events.current.GravestoneUnblocked += UnblockGrave;

        instance = this;

        GetAllGravesInScene();
        _graveRobbers = new List<GameObject>();
        GetAllRobberSpawnPositions();
        GetAllOfferingPositions();
    }

    private void Start()
    {
        score.InitAllGraves(allGraves);
        SetUIDay();

        if (_playTutorial)
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
            if (Input.GetKeyDown(KeyCode.E) && DialogueManager.playingConversation)
            {
                _UIScript.HidePlayerUI();
                DialogueManager.instance.DisplayNextSentence();
            }
            if(TimeDisplay.hours == 0 && TimeDisplay.minutes == 1 && TimeSinceLastRobbers > 5)
            {
                TimeSinceLastRobbers = 0;
                StartCoroutine(SpawnGraveRobbers(RobbersPerSpawn));
            }
        }
        timeSinceLastDialogueStarted += Time.deltaTime;
        TimeSinceLastRobbers += Time.deltaTime;
    }

    private void SetDifficulty()
    {
        RobbersPerSpawn = DifficultySlider * 2;
    }

    private void RemoveGraveRobber(GameObject graveRobber)
    {
        if (_graveRobbers.Contains(graveRobber))
        {
            _graveRobbers.Remove(graveRobber);
        }
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
        StartCoroutine(_UIScript.FadeFromBlack());
        _UIScript.StartGame();
        SetupDayAndNight();

        SpawnPlayer();
        References.playerScript.UnlockMovement();
        References.playerScript.UnlockAbilities();
        References.playerScript._interactionLocked = false;
        References.playerScript.playerAnimator.SetBool("HatOn", true);
        SetPlayerReferencesInScene();
        SpawnOfferings();
        SpawnQuestItems();
    }

    //DIALOGUE

    public void TriggerDialogue(ConversationScriptObj convo)
    {
        if(timeSinceLastDialogueStarted >= timeUntilNextDialogueCanBeStarted)
        {
            _UIScript.TurnOnDialogue();
            DialogueManager.instance.StartDialogue(convo);
            if(!_isPlayingTutorial)
            {
                References.playerScript.LockMovement();
                References.playerScript.LockAbilities();
            }
        }
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
    private void SetUIDay()
    {
        _UIScript.SetDays(DaysPassed + 1, AmountOfDays);
    }

    //  TIME/DAYS

    void SetupDayAndNight()
    {
        _lighting.DayLength = HalfDayLength;
        _lighting.NightLength = HalfNightLength;

        StartCoroutine(_lighting.Day(_lighting.DayLength));
    }
    private void FinishDay()
    {
        DaysPassed++;
        if(DaysPassed >= AmountOfDays) //week over => game over
        {
            SetUIDay();
            StartCoroutine(PlayEnding());
        }
        else //start a new day
        {
            SetupDayAndNight();
            //StartCoroutine(SpawnGraveRobbers(Random.Range(3, 6)));
            print("new day");
            SpawnOfferings();
            SetUIDay();
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
        }
    }
    private IEnumerator SpawnGraveRobbers(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            SpawnGraveRobber();
            yield return new WaitForSeconds(0.25f);
            //yield return new WaitForFixedUpdate();

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
        bool SpawnRobber = false;
        foreach(Gravestone grave in allGraves)
        {
            if(!grave.IsBeingTargeted && !blockedGraves.Contains(grave) && !grave._destroyed)
            {
                SpawnRobber = true;
            }
        }
        if(SpawnRobber)
        {
            GameObject go = Instantiate(_graveRobberPrefab);
            go.transform.position = graveRobberSpawnPositions[Random.Range(0, graveRobberSpawnPositions.Count)].transform.position;
            go.GetComponent<GraveRobber>().InitRobber(allGraves, _graveRobberEscapePos);
            go.transform.parent = GraveRobberParent.transform;
            _graveRobbers.Add(go);
            go.GetComponent<GraveRobber>().blockedGraves = blockedGraves;
        }
    }

    private void SpawnQuestItems()
    {
        foreach(Gravestone grave in allGraves)
        {
            grave.GetComponentInChildren<GraveGhost>().SpawnQuestItem();
        }

        catGhostQuestItem = GameObject.FindGameObjectWithTag("CatGhostQuestItem");
        catGhostQuestItem.SetActive(false);
    }

    public void GoToMainMenu()
    {
        StartCoroutine(ShowLoadingScreen());
    }

    public IEnumerator ShowLoadingScreen()
    {
        LoadingScreen.SetActive(true);
        Time.timeScale = 1;
        yield return new WaitForSeconds(0.5f);
        Loader.Load(Loader.Scene.MainMenuScene);
    }

    //      TUTORIAL STUFF

    private void DisableGraveghostFadein()
    {
        foreach(Gravestone grave in allGraves)
        {
            grave.GetComponentInChildren<GraveGhost>().DistanceFromPlayerToActivate = 0f;
            grave.GetComponentInChildren<GraveGhost>().happiness.gameObject.SetActive(false);
        }
    }
    private void EnableGraveghostFadein()
    {
        foreach (Gravestone grave in allGraves)
        {
            grave.GetComponentInChildren<GraveGhost>().DistanceFromPlayerToActivate = 3f;
            grave.GetComponentInChildren<GraveGhost>().happiness.gameObject.SetActive(true);
        }
    }
    private GraveRobber SpawnTutorialRobber(Transform transform)
    {
        GameObject go = Instantiate(_graveRobberPrefab, transform.position, transform.rotation);
        return go.GetComponent<GraveRobber>();
    }

    //EVENT STUFF

    private IEnumerator StartTutorial()
    {
        print("starting tutorial");

        _isPlayingTutorial = true;


        //  Setup of Scene for the tutorial

        //  UI

        _UIScript.StartTutorial();

        SpawnPlayer();
        SetPlayerReferencesInScene();
        References.playerScript.HidePlayer();
        References.playerScript.LockMovement();
        References.playerScript.playerAnimator.SetBool("HatOn", false);
        //lock all our abilities
        References.playerScript._screamLocked = References.playerScript._dashLocked = References.playerScript._possessionLocked
        = References.playerScript._depossessionLocked = References.playerScript._interactionLocked = true;
        DayNightLighting.freezeDayNight = true;
        SetupDayAndNight();
        

        DisableGraveghostFadein();

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
        _kittyGrave.InitMaxHealth(100000); //make kitty grave unkillable for tutorial purposes
        _kittyGrave.TakeDamage(40000); //set at half health

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
        StartCoroutine(_UIScript.FadeInPrompts());
        _UIScript.PromptScream();
        Robber.fearLevel.InitMaxFear(40);
        References.playerScript._screamLocked = false;
        _UIScript.ScreamMeterHidden = false;
        _UIScript.portraitHidden = false;
        _UIScript.ShowPlayerUI();

        yield return new WaitWhile(() => !Robber.isTerrified);
        StartCoroutine(_UIScript.FadeOutPrompts());

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


        //  Scene #7: Player defeated Robber, unlock possession, fix Cats Grave


        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);

        References.playerScript.MoveToNextPos();
        yield return new WaitWhile(() => !References.playerScript.reachedEndOfPath);
        _UIScript.PromptPossess();
        StartCoroutine(_UIScript.FadeInPrompts());
        References.playerScript._possessionLocked = false;
        _UIScript.ShowPlayerUI();
        yield return new WaitWhile(() => !References.playerScript.IsPossessing);
        StartCoroutine(_UIScript.FadeOutPrompts());
        yield return new WaitForSeconds(2.5f);
        _kittyGrave.InitMaxHealth(100f);
        _kittyGrave.RestoreGrave(1f); //plays the healing sound
        yield return new WaitForSeconds(0.5f);


        //  Scene #8: Player is prompted to leave grave


        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        StartCoroutine(_UIScript.FadeInPrompts());
        _UIScript.PromptDepossess();
        References.playerScript._depossessionLocked = false;
        yield return new WaitWhile(() => References.playerScript.IsPossessing);
        StartCoroutine(_UIScript.FadeOutPrompts());
        References.playerScript.LockMovement();
        References.playerScript._depossessionLocked = true;
        References.playerScript._possessionLocked = true;
        yield return new WaitForSeconds(1f);


        //Scene #9: Player has depossessed, but theres a new robber on our left


        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);


        // Scene #10: Robber is stealing loot, cool ghost talks

        _coolGhost.DistanceFromPlayerToActivate = 100f;
        _coolGrave.InitMaxHealth(100000);
        _coolGrave.TakeDamage(50000); //get to last stage of damage

        GameObject coolGhostGraveItem = _coolGhost._graveItem;
        _coolGhost._graveItem = _coolGhost._questItemPrefab;

        Robber = SpawnTutorialRobber(tutorialRobberPositions[1]);
        Robber.InitRobber(allGraves, _graveRobberEscapePos);

        _tutorialGhost.MoveToNextPos();
        yield return new WaitForSeconds(1f);
        References.playerScript.MoveToNextPos();
        yield return new WaitWhile(() => !References.playerScript.reachedEndOfPath);
        yield return new WaitWhile(() => !_tutorialGhost.reachedEndOfPath);
        yield return new WaitForSeconds(0.25f);
        _kitty.DistanceFromPlayerToActivate = 0f;
        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        yield return new WaitForSeconds(1f);
        _coolGrave.TakeDamage(100000);
        yield return new WaitForSeconds(0.85f);
        Robber.lockMovement = true;

        //Scene #11: Ghost tells you to dash

        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        Robber.fearLevel.InitMaxFear(20);
        StartCoroutine(_UIScript.FadeInPrompts());
        _UIScript.PromptDash();
        References.playerScript.lastMovementDir = References.playerScript.movement = new Vector2(-1, 0);
        References.playerScript._dashLocked = false;
        _UIScript.DashMeterHidden = false;
        _UIScript.ShowPlayerUI();

        yield return new WaitWhile(() => !Robber.isTerrified);
        StartCoroutine(_UIScript.FadeOutPrompts());
        Robber.lockMovement = false;
        References.playerScript._dashLocked = true;

        //Scene #12: Move to cool ghost, Cool Ghost compliments you asks you to pick up his shades

        yield return new WaitWhile(() => References.playerScript.TimeSinceLastDash < 1f);
        _tutorialGhost.MoveToNextPos();
        yield return new WaitForSeconds(0.1f);
        References.playerScript.MoveToNextPos();
        yield return new WaitWhile(() => !References.playerScript.reachedEndOfPath);
        yield return new WaitWhile(() => !_tutorialGhost.reachedEndOfPath);
        yield return new WaitForSeconds(0.25f);

        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        yield return new WaitForSeconds(0.25f);


        //Scene #13: Move to bag, press interact and carry shades over

        _tutorialGhost.MoveToNextPos();
        yield return new WaitForSeconds(0.1f);
        References.playerScript.MoveToNextPos();
        yield return new WaitWhile(() => !References.playerScript.reachedEndOfPath);
        yield return new WaitWhile(() => !_tutorialGhost.reachedEndOfPath);
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(_UIScript.FadeInPrompts());
        _UIScript.PromptPossessShades();
        References.playerScript._possessionLocked = false;
        yield return new WaitWhile(() => !References.playerScript.IsPossessing);
        StartCoroutine(_UIScript.FadeOutPrompts());

        References.playerScript.possessedObject.lockMovement = true;

        GameObject sunglasses = References.playerScript.possessedObject.gameObject;
        print(sunglasses);

        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        References.playerScript.possessedObject.lockMovement = false;

        //Scene #14: Move and place down Sunglasses

        StartCoroutine(_UIScript.FadeInPrompts());
        _UIScript.PromptMoveWithArrowKeys();
        yield return new WaitWhile(() => Vector2.Distance(References.playerScript.possessedObject.transform.position, _coolGrave.transform.position ) > 0.5f);
        StartCoroutine(_UIScript.FadeOutPrompts());
        References.playerScript.possessedObject.lockMovement = true;
        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        yield return new WaitForSeconds(0.2f);

        StartCoroutine(_UIScript.FadeInPrompts());
        _UIScript.PromptDepossessShades();
        References.playerScript._depossessionLocked = false;
        yield return new WaitWhile(() => References.playerScript.IsPossessing);
        StartCoroutine(_UIScript.FadeOutPrompts());
        References.playerScript.LockMovement();

        yield return new WaitForSeconds(0.2f);
        References.playerScript._depossessionLocked = true;
        References.playerScript._possessionLocked = true;

        //Scene #15: Cool ghost thanks you, move to the next location to talk more about possession

        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);

        _coolGhost.TryCompleteQuest(sunglasses);
        _coolGhost.lootStolen = false;

        Instantiate(_allOfferingTypes[3], tutorialOffering.position, tutorialOffering.rotation);
        _tutorialGhost.MoveToNextPos();
        yield return new WaitForSeconds(0.1f);
        References.playerScript.MoveToNextPos();
        yield return new WaitWhile(() => !References.playerScript.reachedEndOfPath);
        yield return new WaitWhile(() => !_tutorialGhost.reachedEndOfPath);
        yield return new WaitForSeconds(0.25f);

        //Scene #16: Talk about possession and offerings, pick up offering

        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);

        References.playerScript.MoveToNextPos();
        yield return new WaitWhile(() => !References.playerScript.reachedEndOfPath);
        References.playerScript._interactionLocked = false;
        StartCoroutine(_UIScript.FadeInPrompts());
        _UIScript.PromptPickUp();

        yield return new WaitWhile(() => References.playerScript.collectedOfferings.Count == 0);
        StartCoroutine(_UIScript.FadeOutPrompts());
        _UIScript.InventoryHidden = false;
        _UIScript.ShowPlayerUI();

        _coolGrave.InitHappiness(250f);
        _coolGrave.RaiseHappiness(35f);
        _coolGhost.happiness.gameObject.SetActive(true);

        //Scene #17: Bring offering to cool ghost

        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);

        _tutorialGhost.MoveToNextPos();
        yield return new WaitForSeconds(0.1f);
        References.playerScript.MoveToNextPos();
        yield return new WaitWhile(() => !References.playerScript.reachedEndOfPath);
        yield return new WaitWhile(() => !_tutorialGhost.reachedEndOfPath);
        yield return new WaitForSeconds(0.25f);

        StartCoroutine(_UIScript.FadeInPrompts());
        _UIScript.PromptPlaceDown();

        //Scene #18: Cool ghost thanks you for the meal

        yield return new WaitWhile(() => References.playerScript.collectedOfferings.Count != 0);
        StartCoroutine(_UIScript.FadeOutPrompts());
        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        yield return new WaitForSeconds(0.25f);

        //Scene #19: Move to Final Location, tutorial is effectively done, tutorial ghost moves up a bit to get emotional

        _tutorialGhost.MoveToNextPos();
        yield return new WaitForSeconds(0.1f);
        References.playerScript.MoveToNextPos();
        yield return new WaitWhile(() => !References.playerScript.reachedEndOfPath);
        yield return new WaitWhile(() => !_tutorialGhost.reachedEndOfPath);
        yield return new WaitForSeconds(0.25f);
        _coolGhost.DistanceFromPlayerToActivate = 0f;

        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        yield return new WaitForSeconds(0.25f);

        _tutorialGhost.MoveToNextPos();
        yield return new WaitWhile(() => !_tutorialGhost.reachedEndOfPath);

        //  FINAL SCENE! Play final dialogue, relinquish the lock on the player and then finally start the game!

        TriggerDialogue(TutorialConversations[tutorialIndex]);
        tutorialIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        _tutorialGhost.animator.SetBool("HatOn", true);
        References.playerScript.playerAnimator.SetBool("HatOn", true);

        StartCoroutine(_tutorialGhost.FadeOut());
        yield return new WaitForSeconds(1f);

        _isPlayingTutorial = false;

        _cameraScript.Zoom(15, 0.2f);
        References.playerScript.UnlockMovement();

        //unlock all our abilities
        References.playerScript._screamLocked = References.playerScript._dashLocked = References.playerScript._possessionLocked
            = References.playerScript._depossessionLocked = References.playerScript._interactionLocked = false;
        DayNightLighting.freezeDayNight = false;

        //  Reset UI
        _UIScript.proximityButtonsEnabled = true;
        _UIScript.portraitHidden = false;
        _UIScript.DashMeterHidden = false;
        _UIScript.ScreamMeterHidden = false;
        _UIScript.SundialHidden = false;
        _UIScript.TimeDisplayHidden = false;
        _UIScript.InventoryHidden = false;
        _UIScript.ShowPlayerUI();

        //  Reset graves to default values again
        EnableGraveghostFadein();
        _kittyGrave.InitMaxHealth(100f);
        _kittyGrave.TakeDamage(50f);
        _coolGrave.InitMaxHealth(100f);
        _coolGrave.TakeDamage(60f);
        _kittyGrave.InitHappiness(250f);
        _coolGhost.timesGraveWasDestroyed = 0;
        _kitty.timesGraveWasDestroyed = 0;

        _coolGhost._graveItem = null;
        SpawnQuestItems();
        SpawnOfferings();
    }

    private IEnumerator PlayEnding()
    {
        //Setup the end

        print("end");
        DialogueManager.instance.EndDialogue(); //end any possible dialogue we are in

        if (References.playerScript.IsPossessing)
        {
            StartCoroutine(References.playerScript.DepossessObject());
        }
        _isPlayingTutorial = true;
        References.playerScript.LockMovement();
        References.playerScript._screamLocked = References.playerScript._dashLocked = References.playerScript._possessionLocked
        = References.playerScript._depossessionLocked = References.playerScript._interactionLocked = true;
        References.playerScript.lastMovementDir = References.playerScript.movement = new Vector2(0, -1);

        _UIScript.HidePlayerUI();
        _UIScript.DashMeterHidden = true;
        _UIScript.ScreamMeterHidden = true;
        _UIScript.proximityButtonsEnabled = false;
        _UIScript.InventoryHidden = true;
        _UIScript.portraitHidden = true;
        _UIScript.TimeDisplayHidden = true;

        DisableGraveghostFadein();
        foreach (GameObject robberObj in _graveRobbers)
        {
            GraveRobber robber = robberObj.GetComponent<GraveRobber>();
            StartCoroutine(robber.Flee());
        }


        //zoom in for dialogue


        yield return new WaitForSeconds(1f);
        _cameraScript.Zoom(30, 0.5f);

        //Scene 1: We can talk! Move to next pos

        TriggerDialogue(EndingConversations[EndingConversationIndex]);
        EndingConversationIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        yield return new WaitForSeconds(1f);
        References.playerScript.positionIndex = 11;
        References.playerScript.MoveToNextPos();
        yield return new WaitWhile(() => !References.playerScript.reachedEndOfPath);
        yield return new WaitForSeconds(0.5f);

        //Scene 2: reminisce, and look around

        TriggerDialogue(EndingConversations[EndingConversationIndex]);
        EndingConversationIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        yield return new WaitForSeconds(0.5f);
        References.playerScript.lastMovementDir = new Vector2(-1, -1);
        yield return new WaitForSeconds(0.5f);
        References.playerScript.lastMovementDir = new Vector2(0, -1);
        yield return new WaitForSeconds(0.5f);
        References.playerScript.lastMovementDir = new Vector2(1, -1);
        yield return new WaitForSeconds(0.5f);
        References.playerScript.lastMovementDir = new Vector2(0, -1);
        yield return new WaitForSeconds(0.2f);

        //Scene 3: Talk about new caretaker appearing, move there

        TriggerDialogue(EndingConversations[EndingConversationIndex]);
        EndingConversationIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        References.playerScript.MoveToNextPos();
        yield return new WaitWhile(() => !References.playerScript.reachedEndOfPath);
        yield return new WaitForSeconds(0.5f);

        //Scene 4: Talk to next ghost

        References.playerScript.lastMovementDir = new Vector2(1, 0);
        TriggerDialogue(EndingConversations[EndingConversationIndex]);
        EndingConversationIndex++;
        yield return new WaitWhile(() => DialogueManager.playingConversation);

        _endingGhost.GetComponentInChildren<Animator>().SetBool("Appear", true);
        StartCoroutine(_endingGhost.FadeIn());
        //fade to black so we can show the endscreen

        StartCoroutine(_UIScript.FadeToBlack());
        for(int i = 0; i < 256; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        EndOfGame();
    }
    private void EndOfGame()
    {
        StartCoroutine(_UIScript.FadeFromBlack());
        print("VICTORY");
        EndingMusic.Play();
        _UIScript.ShowEndScreen();
    }
}
