using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    public LevelSceneManager level;
    public UIScript ui;

    public Text _dialogueText;

    public GameObject _speakerLeft;
    public Image _speakerLeftImage;
    public GameObject _speakerLeftImageBackground;
    public Text _speakerLeftName;
    public RectTransform _speakerLeftNameBG;
    public GameObject _speakerLeftNameObject;

    public Image _speakerRightImage;
    public GameObject _speakerRightImageBackground;
    public GameObject _speakerRight;
    public Text _speakerRightName;
    public RectTransform _speakerRightNameBG;
    public GameObject _speakerRightNameObject;
    public static bool playingConversation;

    public static int dialogueSpeed;
    private int FramesBetweenCharacters;

    public float speakerHeight;

    [SerializeField]
    private Queue<Line> sentences = new Queue<Line>();

    private void Awake()
    {
        instance = this;
        speakerHeight = _speakerRightImage.rectTransform.sizeDelta.y;
    }
    private void Start()
    {
        dialogueSpeed = 8; // => 0 frames between text, this is max dialogue speed
        FramesBetweenCharacters = 4 / dialogueSpeed;
    }
    public void StartDialogue(ConversationScriptObj convo)
    {
        ui.DialogueShown = true;
        if (!playingConversation)
        {
            playingConversation = true;
            sentences.Clear();

            foreach (Line line in convo.lines)
            {
                sentences.Enqueue(line);
            }
            DisplayNextSentence();
        }
    }

    public void DisplayNextSentence()
    {
        _speakerLeft.SetActive(false);
        _speakerRight.SetActive(false);
        _speakerLeftNameObject.SetActive(false);
        _speakerRightNameObject.SetActive(false);

        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        Line line = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(line.text));

        if (line.characterLeft)
        {
            _speakerLeft.SetActive(true);
            _speakerLeftImage.sprite = line.characterLeft.portrait;
            if(line.characterLeft.width > 0 && line.characterLeft.height > 0)
            {
                _speakerLeftImage.rectTransform.sizeDelta = new Vector2(((float) line.characterLeft.width/line.characterLeft.height) * speakerHeight, speakerHeight);
            }
            if(line.characterLeft.fullName != null)
            {
                _speakerLeftNameObject.SetActive(true);
                _speakerLeftName.text = line.characterLeft.fullName;
                _speakerLeftNameBG.sizeDelta = new Vector2(Mathf.Max(_speakerLeftImageBackground.GetComponent<RectTransform>().sizeDelta.x,
                                                                      30 + line.characterLeft.fullName.Length * 28), _speakerLeftNameBG.rect.height);
            }
        }
        if (line.characterRight)
        {
            _speakerRight.SetActive(true);
            _speakerRightImage.sprite = line.characterRight.portrait;
            if (line.characterRight.width > 0 && line.characterRight.height > 0)
            {
                _speakerRightImage.rectTransform.sizeDelta = new Vector2(((float) line.characterRight.width / line.characterRight.height) * speakerHeight, speakerHeight);
            }
            if (line.characterRight.fullName != null)
            {
                _speakerRightNameObject.SetActive(true);
                _speakerRightName.text = line.characterRight.fullName;
                _speakerRightNameBG.sizeDelta = new Vector2(Mathf.Max( _speakerRightImageBackground.GetComponent<RectTransform>().sizeDelta.x, 
                                                                       30 + line.characterRight.fullName.Length * 28), _speakerRightNameBG.rect.height);
            }
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        _dialogueText.text = "";
        foreach(char letter in sentence.ToCharArray())
        {
            _dialogueText.text += letter;
            for(int i = 0; i <= FramesBetweenCharacters; i++)
            {
                yield return new WaitForFixedUpdate();
            }
        }
    }

    public void EndDialogue()
    {
        playingConversation = false;
        ui.TurnOffDialogue();
        ui.DialogueShown = false;
        if(!LevelSceneManager._isPlayingTutorial)
        {
            References.playerScript.UnlockMovement();
            References.playerScript.UnlockAbilities();
        }
        level.timeSinceLastDialogueStarted = 0f;
    }
}
