using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueData
{
    [Header("Speaker Info")]
    public string speakerName;
    public Sprite speakerPortrait;
    
    [Header("Dialogue Content")]
    [TextArea(3, 10)]
    public string[] dialogueLines;
    
    [Header("Dialogue Type")]
    public DialogueType type;
    
    [Header("Player Options")]
    public PlayerChoice[] playerChoices;
}

[System.Serializable]
public class PlayerChoice
{
    [TextArea(2, 5)]
    public string choiceText;
    
    [Header("Response")]
    public DialogueData responseDialogue;
}

public enum DialogueType
{
    Normal,      // 普通对话，只有NPC说话
    WithChoices, // 带选择的对话
    Ending       // 结束对话
}

[System.Serializable]
public class DialogueSequence
{
    [Header("Dialogue Sequence")]
    public string sequenceName;
    public List<DialogueData> dialogues;
}
