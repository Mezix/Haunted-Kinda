using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    public UIScript ui;

    public Text _dialogueText;
    public GameObject _speakerLeft;
    public Image _speakerLeftImage;
    public GameObject _speakerRight;
    public Image _speakerRightImage;
    private bool playingConversation;

    public float speakerHeight;

    [SerializeField]
    private Queue<Line> sentences = new Queue<Line>();

    private void Awake()
    {
        instance = this;
        speakerHeight = _speakerRightImage.rectTransform.sizeDelta.y;
    }
    public void StartDialogue(ConversationScriptObj convo)
    {
        if(!playingConversation)
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
        }
        if (line.characterRight)
        {
            _speakerRight.SetActive(true);
            _speakerRightImage.sprite = line.characterRight.portrait;
            if (line.characterRight.width > 0 && line.characterRight.height > 0)
            {
                _speakerRightImage.rectTransform.sizeDelta = new Vector2(((float) line.characterRight.width / line.characterRight.height) * speakerHeight, speakerHeight);
            }
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        _dialogueText.text = "";
        foreach(char letter in sentence.ToCharArray())
        {
            _dialogueText.text += letter;
            yield return null;
        }
    }

    public void EndDialogue()
    {
        playingConversation = false;
        ui.TurnOffDialogue();
    }
}
