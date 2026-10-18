using UnityEngine;

[CreateAssetMenu(menuName = "UI/Transition Texts")]
public class TransitionTextsSO : ScriptableObject
{
    [SerializeField, TextArea(3, 8)] private string _default;           // transición normal
    [SerializeField, TextArea(3, 8)] private string _reputationMaxed;
    [SerializeField, TextArea(3, 8)] private string _reputationLost;
    [SerializeField, TextArea(3, 8)] private string _wellbeingDepleted;

    public string GetText(EndingType? ending)
    {
        string text = ending switch
        {
            EndingType.ReputationMaxed => _reputationMaxed,
            EndingType.ReputationLost => _reputationLost,
            EndingType.WellbeingDepleted => _wellbeingDepleted,
            _ => _default
        };

        // si un texto de final está vacío, se usa el normal
        return string.IsNullOrWhiteSpace(text) ? _default : text;
    }
}