using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveGhost : MonoBehaviour
{
    private UnityEngine.Experimental.Rendering.Universal.Light2D GhostGlow; //the glow of our ghost
    private GameObject player; //reference to the player in the scene
    private SpriteRenderer ghostRenderer; //the sprite rendered of our ghost
    private float ghostSpriteOpacity; //the current opacity of our ghost
    private float ghostSpriteMaxOpacity; //the max opacity our ghost should achieve
    private float fadeAmount; //the amount to change our opacity every frame
    public Gravestone grave;

    public HealthBarScript happiness;

    public GameObject _graveItem;
    public bool lootStolen;

    public bool hasQuest = false;
    public String QuestName;
    public GameObject _questItemPrefab;
    private GameObject questItem;
    public Transform questItemLocation;
    public Transform itemDropOffPos;
    public bool QuestComplete;
    private bool uiQuestGiven;
    public int timesGraveWasDestroyed;

    public float DistanceFromPlayerToActivate = 3f;

    //CONVERSATIONS

    public int _conversationIndex;
    public ConversationScriptObj[] _conversations;
    public ConversationScriptObj QuestHasBeenGiven;
    public ConversationScriptObj _questItemReturnedConvo;

    public int _postQuestConversationIndex;
    public ConversationScriptObj[] _PostQuestConversations;
    public ConversationScriptObj[] _RandomConversations;

    private void Awake()
    {
        GhostGlow = GetComponentInChildren<UnityEngine.Experimental.Rendering.Universal.Light2D>();
        ghostRenderer = GetComponentInChildren<SpriteRenderer>();
        happiness = GetComponentInChildren<HealthBarScript>();

        if(_questItemPrefab)
        {
            hasQuest = true;
        }
    }

    public void SpawnQuestItem()
    {
        if (_questItemPrefab && questItemLocation)
        {
            questItem = Instantiate(_questItemPrefab, questItemLocation.transform);
        }
    }

    private void Start()
    {
        ghostSpriteOpacity = 0; //set our ghost to be hidden by default
        ghostSpriteMaxOpacity = 0.6f; //our ghost should always be slightly opaque, so keep this value below 0
        ghostRenderer.color = new Color(1, 1, 1, ghostSpriteOpacity); //apply the ghostopacity value
        GhostGlow.intensity = 0;
        fadeAmount = 0.05f;
        _conversationIndex = 0;
        _postQuestConversationIndex = 0;

        happiness.healthbarbackground.color = happiness.healthbar.color = ghostRenderer.color = new Color(1, 1, 1, ghostSpriteOpacity);

    }
    private void FixedUpdate()
    {
        if(player is object)
        {
            if (Vector2.Distance(player.transform.position, transform.position) <= DistanceFromPlayerToActivate && !References.playerScript.IsPossessing) //as long as we close to the player, show the ghost
            {
                if (ghostSpriteOpacity < ghostSpriteMaxOpacity) //as long as we havent reached our max opacity, make less opaque
                {
                    FadeIn();
                }
            }
            else if (ghostSpriteOpacity > 0 || References.playerScript.IsPossessing) //otherwise slowly fadeout
            {
                FadeOut();
            }
        }
        if(questItem && itemDropOffPos)
        {
            if (Vector2.Distance(questItem.transform.position, itemDropOffPos.position) < 0.5 && !QuestComplete && !References.playerScript.possessedObject.isPossessed)
            {
                StartCoroutine(TriggerQuestComplete());
            }
        }
    }

   
    public void SetPlayerReference()
    {
        player = References.Player;
    }

    public void PlayConversation()
    {
        if(LevelSceneManager.instance.timeSinceLastDialogueStarted >= LevelSceneManager.instance.timeUntilNextDialogueCanBeStarted)
        {
            if (!QuestComplete)
            {
                if (_conversations.Length > 0)
                {
                    if (_conversationIndex < _conversations.Length)
                    {
                        if (_conversations[_conversationIndex])
                        {
                            LevelSceneManager.instance.TriggerDialogue(_conversations[_conversationIndex]);
                            _conversationIndex++;
                        }
                    }
                    else
                    {
                        if (QuestHasBeenGiven)
                        {
                            LevelSceneManager.instance.TriggerDialogue(QuestHasBeenGiven);
                            if(!uiQuestGiven)
                            {
                                LevelSceneManager.instance._UIScript.QuestChecklist.GetComponent<UIQuestManager>().ShowQuest(this);
                                uiQuestGiven = true;
                            }
                        }
                    }
                }
            }
            else
            {
                if (_PostQuestConversations.Length > 0 && _postQuestConversationIndex < _PostQuestConversations.Length)
                {
                    if (_PostQuestConversations[_postQuestConversationIndex])
                    {
                        LevelSceneManager.instance.TriggerDialogue(_PostQuestConversations[_postQuestConversationIndex]);
                        _postQuestConversationIndex++;
                    }
                }
                else
                {
                    if (_RandomConversations.Length > 0)
                        LevelSceneManager.instance.TriggerDialogue(_RandomConversations[UnityEngine.Random.Range(0, _RandomConversations.Length)]);
                }
            }
        }
    }
    private IEnumerator TriggerQuestComplete()
    {
        print("completing quest");

        QuestComplete = true;
        PossessableObject questPossessable = questItem.GetComponent<PossessableObject>();
        References.playerScript.LockMovement();
        LevelSceneManager.instance.TriggerDialogue(_questItemReturnedConvo);
        LevelSceneManager.instance._UIScript.QuestChecklist.GetComponent<UIQuestManager>().CheckOffQuest(this);
        QuestComplete = true;
        yield return new WaitWhile(() => DialogueManager.playingConversation);
        References.playerScript.UnlockMovement();
        questPossessable.ReturnPossessableToGhost();
        grave.RaiseHappiness(100);

        if(_questItemPrefab.tag == "FishermanGhostQuestItem")
        {
            LevelSceneManager.catGhostQuestItem.SetActive(true);
            print("spawning fish");
        }
    }

    public void FadeIn()
    {
        if(ghostSpriteOpacity > ghostSpriteMaxOpacity) //make sure we dont exceed the max alpha value we want
        {
            ghostSpriteOpacity = ghostSpriteMaxOpacity;
            return;
        }
        ghostSpriteOpacity += fadeAmount;
        ghostRenderer.color = new Color(1, 1, 1, ghostSpriteOpacity);
        happiness.healthbarbackground.color = happiness.healthbar.color = ghostRenderer.color = new Color(1, 1, 1, ghostSpriteOpacity);
        GhostGlow.intensity = ghostSpriteOpacity / ghostSpriteMaxOpacity; //swing between 0 and 1, depending on our fade
    }
    public void FadeOut()
    {
        if (ghostSpriteOpacity < 0)
        {
            ghostSpriteOpacity = 0;
            GhostGlow.intensity = 0; //this value becomes negative if we dont set it to 0
            return;
        }
        ghostSpriteOpacity -= fadeAmount;
        ghostRenderer.color = new Color(1, 1, 1, ghostSpriteOpacity);
        happiness.healthbarbackground.color = happiness.healthbar.color = ghostRenderer.color = new Color(1, 1, 1, ghostSpriteOpacity);
        GhostGlow.intensity = Mathf.Max(0, ghostSpriteOpacity / ghostSpriteMaxOpacity);
    }

    public void TryCompleteQuest(GameObject newItem)
    {
        if(_questItemPrefab.tag == newItem.tag)
        {
            QuestComplete = true;
            newItem.GetComponent<PossessableObject>().ReturnPossessableToGhost();
        }
    }
}
