using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Barras de reputación y bienestar del HUD (#ReputationFill y #WellbeingFill):
// título arriba de cada barra y feedback al ganar o perder
public class PlayerStatusHUD : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private PlayerStats _stats;
    [SerializeField] private Font _font; // fuente de títulos y números (ej. Handlee-Regular)

    [Header("Feedback")]
    [SerializeField, Min(0f)] private float _minFeedback = 3f;          // cambios menores (desgaste de vida) no hacen efecto ni número
    [SerializeField, Range(0f, 1f)] private float _lowWellbeing = 0.3f; // bienestar en alerta

    private const float FloatingTime = 1f;
    private const float FloatingRise = 28f;

    private static readonly Color TextColor = new Color32(0x49, 0x35, 0x22, 255);
    private static readonly Color GainColor = new Color32(0x5E, 0x8C, 0x2A, 255);
    private static readonly Color LossColor = new Color32(0xB0, 0x3A, 0x2E, 255);
    private static readonly Color GainFlash = new Color32(0xFF, 0xF6, 0xC8, 255);
    private static readonly Color GhostGain = new Color32(0xF2, 0xE6, 0xA0, 255);
    private static readonly Color GhostLoss = new Color32(0xD2, 0x5A, 0x48, 255);

    private class BarView
    {
        public Stat Stat;
        public VisualElement Container, Fill, Ghost;
        public Label Title;
        public bool CanWarn;

        public Color BaseColor;
        public bool HasBase;
        public Color FlashColor;

        public float FillValue, GhostValue, GhostHold;
        public float Flash, Pop, Shake;
    }

    private class FloatingText
    {
        public Label Label;
        public float Age;
    }

    private readonly List<FloatingText> _floating = new List<FloatingText>();
    private BarView _reputation;
    private BarView _wellbeing;

    private void Start()
    {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();

        VisualElement root = _uiDocument.rootVisualElement;
        VisualElement reputationFill = root.Q<VisualElement>("ReputationFill");
        VisualElement wellbeingFill = root.Q<VisualElement>("WellbeingFill");

        if (reputationFill == null || wellbeingFill == null)
        {
            Debug.LogError("PlayerStatusHUD: faltan #ReputationFill o #WellbeingFill en el UXML.", this);
            return;
        }

        _reputation = CreateView(_stats.Reputation, reputationFill, "Reputación", false);
        _wellbeing = CreateView(_stats.Wellbeing, wellbeingFill, "Bienestar", true);

        _stats.Reputation.Changed += OnReputationChanged;
        _stats.Wellbeing.Changed += OnWellbeingChanged;
    }

    private void OnDestroy()
    {
        if (_stats == null) return;

        _stats.Reputation.Changed -= OnReputationChanged;
        _stats.Wellbeing.Changed -= OnWellbeingChanged;
    }

    private BarView CreateView(Stat stat, VisualElement fill, string title, bool canWarn)
    {
        VisualElement bar = fill.parent;
        VisualElement container = bar.parent;

        BarView view = new BarView
        {
            Stat = stat,
            Fill = fill,
            Container = container,
            CanWarn = canWarn,
            FillValue = stat.Normalized,
            GhostValue = stat.Normalized
        };

        // lugar para el título arriba de la barra
        container.style.marginTop = 20;

        view.Title = new Label(title);
        SetupLabel(view.Title, 15f);
        view.Title.style.left = 30;
        view.Title.style.top = -17;
        view.Title.style.color = TextColor;
        container.Add(view.Title);

        // franja detrás del relleno: muestra lo que se ganó o se acaba de perder
        VisualElement ghost = new VisualElement();
        ghost.pickingMode = PickingMode.Ignore;
        ghost.style.position = Position.Absolute;
        ghost.style.left = 0;
        ghost.style.top = 0;
        ghost.style.bottom = 0;
        ghost.style.borderTopLeftRadius = 5;
        ghost.style.borderBottomLeftRadius = 5;
        ghost.style.borderTopRightRadius = 5;
        ghost.style.borderBottomRightRadius = 5;
        bar.Insert(0, ghost);
        view.Ghost = ghost;

        ApplyWidths(view);
        return view;
    }

    private void SetupLabel(Label label, float size)
    {
        label.pickingMode = PickingMode.Ignore;
        label.style.position = Position.Absolute;
        label.style.marginLeft = 0;
        label.style.marginRight = 0;
        label.style.marginTop = 0;
        label.style.marginBottom = 0;
        label.style.paddingLeft = 0;
        label.style.paddingRight = 0;
        label.style.paddingTop = 0;
        label.style.paddingBottom = 0;
        label.style.fontSize = size;
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        label.style.whiteSpace = WhiteSpace.NoWrap;

        if (_font != null) label.style.unityFontDefinition = FontDefinition.FromFont(_font);
    }

    private void OnReputationChanged(float value, float delta) => OnStatChanged(_reputation, delta);
    private void OnWellbeingChanged(float value, float delta) => OnStatChanged(_wellbeing, delta);

    private void OnStatChanged(BarView view, float delta)
    {
        if (view == null) return;

        float target = view.Stat.Normalized;
        bool gain = delta > 0f;

        if (gain)
        {
            // la franja clara marca hasta dónde va a llegar el relleno
            view.GhostValue = target;
            view.Ghost.style.backgroundColor = GhostGain;
        }
        else
        {
            // la franja roja queda un instante mostrando lo perdido
            view.GhostValue = Mathf.Max(view.GhostValue, view.FillValue);
            view.GhostHold = 0.45f;
            view.Ghost.style.backgroundColor = GhostLoss;
        }

        if (Mathf.Abs(delta) < _minFeedback) return;

        view.FlashColor = gain ? GainFlash : LossColor;
        view.Flash = 1f;

        if (gain) view.Pop = 1f;
        else view.Shake = 1f;

        SpawnFloating(view, delta);
    }

    private void SpawnFloating(BarView view, float delta)
    {
        Label label = new Label((delta > 0f ? "+" : "") + Mathf.RoundToInt(delta));
        SetupLabel(label, 22f);
        label.style.right = 4;
        label.style.top = -18;
        label.style.color = delta > 0f ? GainColor : LossColor;
        view.Container.Add(label);

        _floating.Add(new FloatingText { Label = label });
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;

        UpdateView(_reputation, dt);
        UpdateView(_wellbeing, dt);
        UpdateFloating(dt);
    }

    private void UpdateView(BarView view, float dt)
    {
        if (view == null) return;

        float target = view.Stat.Normalized;

        // sube suave, baja de golpe
        if (view.FillValue < target)
        {
            view.FillValue = Mathf.Lerp(view.FillValue, target, 1f - Mathf.Exp(-6f * dt));
            if (target - view.FillValue < 0.0005f) view.FillValue = target;
        }
        else
        {
            view.FillValue = target;
        }

        // la franja de lo perdido espera un instante y se achica
        view.GhostHold -= dt;
        if (view.GhostValue > target && view.GhostHold <= 0f)
            view.GhostValue = Mathf.MoveTowards(view.GhostValue, target, 0.35f * dt);

        if (view.GhostValue < target) view.GhostValue = target;

        ApplyWidths(view);

        view.Flash = Mathf.MoveTowards(view.Flash, 0f, dt / 0.4f);
        view.Pop = Mathf.MoveTowards(view.Pop, 0f, dt / 0.3f);
        view.Shake = Mathf.MoveTowards(view.Shake, 0f, dt / 0.3f);

        bool low = view.CanWarn && target <= _lowWellbeing;
        float throb = low ? (Mathf.Sin(Time.unscaledTime * 6f) + 1f) * 0.5f : 0f;

        // el color base sale del USS; se toma una vez que ya tiene layout
        if (!view.HasBase)
        {
            Color resolved = view.Fill.resolvedStyle.backgroundColor;

            if (resolved.a > 0f)
            {
                view.BaseColor = resolved;
                view.HasBase = true;
            }
        }

        if (view.HasBase)
        {
            Color color = view.BaseColor;
            if (low) color = Color.Lerp(color, LossColor, 0.65f * throb);
            if (view.Flash > 0f) color = Color.Lerp(color, view.FlashColor, view.Flash);
            view.Fill.style.backgroundColor = color;
        }

        view.Title.style.color = low ? Color.Lerp(TextColor, LossColor, throb) : TextColor;

        float scale = 1f + 0.12f * view.Pop + 0.03f * throb;
        float shakeX = Mathf.Sin(Time.unscaledTime * 70f) * 4f * view.Shake;

        view.Container.style.scale = new Scale(new Vector3(scale, scale, 1f));
        view.Container.style.translate = new Translate(shakeX, 0f);
    }

    private void UpdateFloating(float dt)
    {
        for (int i = _floating.Count - 1; i >= 0; i--)
        {
            FloatingText floating = _floating[i];
            floating.Age += dt;

            float t = floating.Age / FloatingTime;

            if (t >= 1f)
            {
                floating.Label.RemoveFromHierarchy();
                _floating.RemoveAt(i);
                continue;
            }

            floating.Label.style.translate = new Translate(0f, -FloatingRise * t);
            floating.Label.style.opacity = 1f - t * t;
        }
    }

    private static void ApplyWidths(BarView view)
    {
        view.Fill.style.width = Length.Percent(view.FillValue * 100f);
        view.Ghost.style.width = Length.Percent(Mathf.Max(view.GhostValue, view.FillValue) * 100f);
    }
}