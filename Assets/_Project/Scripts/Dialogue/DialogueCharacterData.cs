
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

    [Header("Retrato")]
    [SerializeField] private Sprite portrait;

    public string CharacterId => characterId;
    public string CharacterName => characterName;
    public Sprite Portrait => portrait;
}
