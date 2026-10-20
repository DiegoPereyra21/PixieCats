using UnityEngine;

[CreateAssetMenu(menuName = "UI/Transition Texts")]
public class TransitionTextsSO : ScriptableObject
{
    [SerializeField, TextArea(3, 8)] private string _default;
    [SerializeField, TextArea(3, 8)] private string _fae;
    [SerializeField, TextArea(3, 8)] private string _depression;
    [SerializeField, TextArea(3, 8)] private string _happy;
    [SerializeField, TextArea(3, 8)] private string _sacrifice;

    public string GetText(EndingType? ending)
    {
        string text = ending switch
        {
            EndingType.Fae => _fae,
            EndingType.Depression => _depression,
            EndingType.Happy => _happy,
            EndingType.Sacrifice => _sacrifice,
            _ => _default
        };

        // si un texto de final está vacío, se usa el normal
        return string.IsNullOrWhiteSpace(text) ? _default : text;
    }
}