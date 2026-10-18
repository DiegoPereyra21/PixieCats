
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

    public string CharacterId => characterId;
    public string CharacterName => characterName;
    public Sprite NormalPortrait => normalPortrait;
    public Sprite AngryPortrait => angryPortrait;
}
