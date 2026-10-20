using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private MissionRulesSO _rules;

    public MissionRulesSO Rules => _rules;

    // se crean al primer uso, así otros scripts pueden suscribirse en su OnEnable sin importar el orden;
    // si viene de una vuelta anterior, arrancan con los valores guardados
    public Stat Wellbeing => _wellbeing ??= new Stat(GameSession.HasStats ? GameSession.Wellbeing : _rules.StartWellbeing, 0f, _rules.StartWellbeing);
    public Stat Reputation => _reputation ??= new Stat(GameSession.HasStats ? GameSession.Reputation : _rules.StartReputation, 0f, _rules.MaxReputation);

    private Stat _wellbeing;
    private Stat _reputation;
    private void Start() => GameProgressManager.Instance?.RegisterStats(this);

    // se guardan al descargar la escena
    private void OnDestroy()
    {
        if (_wellbeing == null || _reputation == null) return;
        GameSession.SaveStats(_wellbeing.Value, _reputation.Value);
    }
} 