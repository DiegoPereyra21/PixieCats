
using UnityEngine;

[CreateAssetMenu(
    fileName = "FairyPopupDialogueTable",
    menuName = "Dialogue/Fairy Popup Dialogue Table"
)]
public class FairyPopupDialogueTable : ScriptableObject
{
    [SerializeField]
    private FairyPopupDialogueData[] dialogues;

    public FairyPopupDialogueData[] Dialogues => dialogues;
}
