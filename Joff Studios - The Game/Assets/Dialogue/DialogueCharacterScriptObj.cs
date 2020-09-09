using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Dialogue/Character")]
public class DialogueCharacterScriptObj : ScriptableObject
{
    public string fullName;
    public Sprite portrait;
    public int width;
    public int height;
}
