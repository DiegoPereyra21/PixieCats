
using UnityEngine;

public class CharacterIdentity : MonoBehaviour
{
    // sirve para asociar una identidad a un gameobject
    [Header("Identidad del personaje")]
    [SerializeField] private DialogueCharacterData characterData;

    public DialogueCharacterData CharacterData => characterData;

    public string CharacterId => characterData != null ? characterData.CharacterId : string.Empty;

    public string CharacterName => characterData != null ? characterData.CharacterName : string.Empty;

    public Sprite NormalPortrait => characterData != null ? characterData.NormalPortrait : null;
    public Sprite AngryPortrait => characterData != null ? characterData.AngryPortrait : null;
    public Sprite HappyPortrait => characterData != null ? characterData.HappyPortrait : null;
    public Sprite SadPortrait => characterData != null ? characterData.SadPortrait : null;
}
