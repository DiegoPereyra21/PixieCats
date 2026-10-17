using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private MissionRulesSO _rules;

    public MissionRulesSO Rules => _rules;

    // se crean al primer uso, así otros scripts pueden suscribirse en su OnEnable sin importar el orden
    public Stat Wellbeing => _wellbeing ??= new Stat(_rules.StartWellbeing, 0f, _rules.StartWellbeing);
    public Stat Reputation => _reputation ??= new Stat(_rules.StartReputation, 0f, _rules.MaxReputation);

    private Stat _wellbeing;
    private Stat _reputation;
}