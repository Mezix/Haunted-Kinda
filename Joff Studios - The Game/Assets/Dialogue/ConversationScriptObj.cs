using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Line
{
    public DialogueCharacterScriptObj characterLeft;
    public DialogueCharacterScriptObj characterRight;

    [TextArea(2,5)]
    public string text;
}

[CreateAssetMenu(fileName = "New Conversation", menuName = "Dialogue/Conversation")]
public class ConversationScriptObj : ScriptableObject
{
    public DialogueCharacterScriptObj speakerLeft = null;
    public DialogueCharacterScriptObj speakerRight = null;
    public Line[] lines;
}
