using UnityEngine;
using UnityEngine.UIElements;

// Barras de reputación y bienestar del HUD (#ReputationFill y #WellbeingFill)
public class PlayerStatusHUD : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private PlayerStats _stats;

    private VisualElement _reputationFill;
    private VisualElement _wellbeingFill;

    private void Start()
    {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();

        VisualElement root = _uiDocument.rootVisualElement;
        _reputationFill = root.Q<VisualElement>("ReputationFill");
        _wellbeingFill = root.Q<VisualElement>("WellbeingFill");

        if (_reputationFill == null || _wellbeingFill == null)
        {
            Debug.LogError("PlayerStatusHUD: faltan #ReputationFill o #WellbeingFill en el UXML.", this);
            return;
        }

        _stats.Reputation.Changed += OnReputationChanged;
        _stats.Wellbeing.Changed += OnWellbeingChanged;

        SetFill(_reputationFill, _stats.Reputation);
        SetFill(_wellbeingFill, _stats.Wellbeing);
    }

    private void OnDestroy()
    {
        if (_stats == null) return;

        _stats.Reputation.Changed -= OnReputationChanged;
        _stats.Wellbeing.Changed -= OnWellbeingChanged;
    }

    private void OnReputationChanged(float value, float delta) => SetFill(_reputationFill, _stats.Reputation);
    private void OnWellbeingChanged(float value, float delta) => SetFill(_wellbeingFill, _stats.Wellbeing);

    private static void SetFill(VisualElement fill, Stat stat) => fill.style.width = Length.Percent(stat.Normalized * 100f);
}