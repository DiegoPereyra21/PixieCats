
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewDialogueCharacter",
    menuName = "Dialogue/Character"
)]
public class DialogueCharacterData : ScriptableObject
{
    [Header("Informacion del personaje")]
    [SerializeField] private string characterId;
    [SerializeField] private string characterName;

    [Header("Retratos")]
    [SerializeField] private Sprite normalPortrait;
    [SerializeField] private Sprite angryPortrait;
    [SerializeField] private Sprite happyPortrait;
    [SerializeField] private Sprite sadPortrait;

    [Header("Primer dialogo")]
    [SerializeField] private bool hasIntroDialogue = true;
    [SerializeField] private NarrativeDialogueData introDialogue;

    public string CharacterId => characterId;
    public string CharacterName => characterName;
    public Sprite NormalPortrait => normalPortrait;
    public Sprite AngryPortrait => angryPortrait;
    public Sprite HappyPortrait => happyPortrait;
    public Sprite SadPortrait => sadPortrait;

    public bool HasIntroDialogue => hasIntroDialogue;
    public NarrativeDialogueData IntroDialogue => introDialogue;

    public Sprite GetPortrait(PortraitEmotion emotion)
    {
        if (emotion == PortraitEmotion.Angry && angryPortrait != null) return angryPortrait;
        return normalPortrait;
    }
}
