
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

public enum PortraitEmotion
{
    Normal,
    Angry
}

[Serializable]
public class NarrativeDialogueLine
{
    [SerializeField] private DialogueCharacterData character;
    [SerializeField] private PortraitEmotion emotion;

    [TextArea(3, 6)]
    [SerializeField] private string text;

    public DialogueCharacterData Character => character;
    public PortraitEmotion Emotion => emotion;
    public string Text => text;
}
