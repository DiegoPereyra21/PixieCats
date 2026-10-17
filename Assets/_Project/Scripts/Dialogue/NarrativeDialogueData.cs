
using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewNarrativeDialogue",
    menuName = "Dialogue/Narrative Dialogue"
)]
public class NarrativeDialogueData : ScriptableObject
{
    [Header("ID")]
    [SerializeField] private string dialogueId;

    [Header("Lineas del dialogo")]
    [SerializeField] private NarrativeDialogueLine[] lines;

    public string DialogueId => dialogueId;
    public NarrativeDialogueLine[] Lines => lines;
}

[Serializable]
public class NarrativeDialogueLine
{
    [SerializeField] private DialogueCharacterData character;

    [TextArea(3, 6)]
    [SerializeField] private string text;

    public DialogueCharacterData Character => character;
    public string Text => text;
}
