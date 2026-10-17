
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewFairyPopupDialogue",
    menuName = "Dialogue/Fairy Popup Dialogue"
)]
public class FairyPopupDialogueData : ScriptableObject
{
    [Header("ID")]
    [SerializeField] private string dialogueId;

    [Header("Contenido")]
    [TextArea(2, 4)]
    [SerializeField] private string text;

    [Header("Bienestar requerido (%)")]
    [SerializeField, Range(0f, 100f)]
    private float minWellbeing = 0f;

    [SerializeField, Range(0f, 100f)]
    private float maxWellbeing = 100f;

    public string DialogueId => dialogueId;
    public string Text => text;
    public float MinWellbeing => minWellbeing;
    public float MaxWellbeing => maxWellbeing;
}
