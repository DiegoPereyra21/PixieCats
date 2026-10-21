using UnityEngine;
using UnityEngine.UIElements;

// Panel de la misión activa (esquina superior derecha): quién la pide, qué hay que juntar y cuánto tiempo queda para entregar.
// Reemplaza visualmente al #MissionPanel del UXML, que queda oculto
public class ActiveMissionHUD : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private MissionManager _manager;
    [SerializeField] private Font _font; // fuente del HUD (ej. Handlee-Regular)

    [Header("Tamaño")]
    [SerializeField, Range(0.8f, 2f)] private float _textScale = 1.3f; // agranda todos los textos
    [SerializeField, Min(200f)] private float _width = 360f;

    [Header("Aviso")]
    [SerializeField, Range(0f, 1f)] private float _warnFraction = 0.5f;    // el color pasa a ámbar
    [SerializeField, Range(0f, 1f)] private float _dangerFraction = 0.25f; // el color pasa a rojo y late

    private static readonly Color TextColor = new Color32(0x49, 0x35, 0x22, 255);
    private static readonly Color PanelColor = new Color32(0xE8, 0xD5, 0xA9, 255);
    private static readonly Color SafeColor = new Color32(0x8A, 0x9A, 0x50, 255);
    private static readonly Color WarnColor = new Color32(0xC6, 0x9A, 0x55, 255);
    private static readonly Color DangerColor = new Color32(0xB0, 0x3A, 0x2E, 255);

    private VisualElement _panel;
    private VisualElement _progressFill;
    private VisualElement _timeSection;
    private VisualElement _timeFill;
    private Label _name;
    private Label _title;
    private Label _objective;
    private Label _timeCaption;
    private Label _time;

    private MissionDataSO _shown;
    private int _count;
    private int _required;
    private float _progressShown;
    private float _alpha;
    private float _pop;

    private void Start()
    {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();

        if (_uiDocument == null || _manager == null)
        {
            Debug.LogError("ActiveMissionHUD: falta el UIDocument o el MissionManager.", this);
            enabled = false;
            return;
        }

        VisualElement root = _uiDocument.rootVisualElement;

        // el panel viejo del UXML se oculta: este lo reemplaza
        VisualElement oldPanel = root.Q<VisualElement>("MissionPanel");
        if (oldPanel != null) oldPanel.style.display = DisplayStyle.None;

        BuildUI(root.Q<VisualElement>("GameplayHUDRoot") ?? root);

        _manager.ProgressChanged += OnProgressChanged;
    }

    private void OnDestroy()
    {
        if (_manager != null) _manager.ProgressChanged -= OnProgressChanged;
    }

    private void OnProgressChanged(MissionDataSO mission, int count, int required)
    {
        if (count > _count) _pop = 1f;

        _count = count;
        _required = required;
    }

    private void BuildUI(VisualElement parent)
    {
        _panel = new VisualElement();
        _panel.pickingMode = PickingMode.Ignore;
        _panel.style.position = Position.Absolute;
        _panel.style.top = 0;
        _panel.style.right = 0;
        _panel.style.width = _width;
        _panel.style.paddingTop = 14;
        _panel.style.paddingBottom = 16;
        _panel.style.paddingLeft = 18;
        _panel.style.paddingRight = 18;
        _panel.style.backgroundColor = PanelColor;
        _panel.style.display = DisplayStyle.None;
        _panel.style.opacity = 0f;
        SetBorder(_panel, 2f, TextColor);
        SetRadius(_panel, 14f);

        Label tag = CreateLabel(13f, FontStyle.Bold, TextAnchor.MiddleLeft);
        tag.text = "MISIÓN";
        tag.style.color = WarnColor;
        tag.style.letterSpacing = 3;

        _name = CreateLabel(24f, FontStyle.Bold, TextAnchor.MiddleLeft);

        _title = CreateLabel(17f, FontStyle.Normal, TextAnchor.UpperLeft);
        _title.style.whiteSpace = WhiteSpace.Normal;
        _title.style.marginTop = 2;

        VisualElement divider = new VisualElement();
        divider.pickingMode = PickingMode.Ignore;
        divider.style.height = 2;
        divider.style.marginTop = 8;
        divider.style.marginBottom = 8;
        divider.style.backgroundColor = new Color(TextColor.r, TextColor.g, TextColor.b, 0.35f);
        SetRadius(divider, 1f);

        _objective = CreateLabel(18f, FontStyle.Bold, TextAnchor.MiddleLeft);

        _timeSection = new VisualElement();
        _timeSection.pickingMode = PickingMode.Ignore;
        _timeSection.style.marginTop = 10;

        VisualElement row = new VisualElement();
        row.pickingMode = PickingMode.Ignore;
        row.style.flexDirection = FlexDirection.Row;
        row.style.justifyContent = Justify.SpaceBetween;
        row.style.alignItems = Align.Center;

        _timeCaption = CreateLabel(14f, FontStyle.Bold, TextAnchor.MiddleLeft);
        _time = CreateLabel(26f, FontStyle.Bold, TextAnchor.MiddleRight);
        row.Add(_timeCaption);
        row.Add(_time);

        _timeSection.Add(row);
        _timeSection.Add(CreateBar(12f, out _timeFill));

        _panel.Add(tag);
        _panel.Add(_name);
        _panel.Add(_title);
        _panel.Add(divider);
        _panel.Add(_objective);
        _panel.Add(CreateBar(14f, out _progressFill));
        _panel.Add(_timeSection);

        parent.Add(_panel);
    }

    private Label CreateLabel(float size, FontStyle fontStyle, TextAnchor anchor)
    {
        Label label = new Label();
        label.pickingMode = PickingMode.Ignore;
        label.style.marginLeft = 0;
        label.style.marginRight = 0;
        label.style.marginTop = 0;
        label.style.marginBottom = 0;
        label.style.paddingLeft = 0;
        label.style.paddingRight = 0;
        label.style.paddingTop = 0;
        label.style.paddingBottom = 0;
        label.style.fontSize = size * _textScale;
        label.style.unityFontStyleAndWeight = fontStyle;
        label.style.unityTextAlign = anchor;
        label.style.whiteSpace = WhiteSpace.NoWrap;
        label.style.color = TextColor;

        if (_font != null) label.style.unityFontDefinition = FontDefinition.FromFont(_font);

        return label;
    }

    private static VisualElement CreateBar(float height, out VisualElement fill)
    {
        VisualElement bar = new VisualElement();
        bar.pickingMode = PickingMode.Ignore;
        bar.style.height = height;
        bar.style.marginTop = 4;
        bar.style.overflow = Overflow.Hidden;
        bar.style.backgroundColor = TextColor;
        SetRadius(bar, height / 2f);

        fill = new VisualElement();
        fill.pickingMode = PickingMode.Ignore;
        fill.style.height = Length.Percent(100f);
        SetRadius(fill, height / 2f - 1f);
        bar.Add(fill);

        return bar;
    }

    private void Update()
    {
        if (_panel == null) return;

        float dt = Time.unscaledDeltaTime;

        MissionDataSO mission = _manager.ActiveMission;
        bool visible = mission != null;

        _alpha = Mathf.MoveTowards(_alpha, visible ? 1f : 0f, dt / 0.25f);
        _panel.style.display = _alpha > 0f ? DisplayStyle.Flex : DisplayStyle.None;
        _panel.style.opacity = _alpha;

        // entra deslizándose desde la derecha
        _panel.style.translate = new Translate((1f - _alpha) * 40f, 0f);

        if (!visible)
        {
            _shown = null;
            _panel.style.scale = new Scale(Vector3.one);
            return;
        }

        int required = _required > 0 ? _required : mission.RequiredCount;
        float progress = Mathf.Clamp01(required > 0 ? (float)_count / required : 0f);

        if (mission != _shown)
        {
            _shown = mission;
            _progressShown = progress;

            SetText(_name, mission.Requester != null ? mission.Requester.CharacterName : mission.Title);
            SetText(_title, mission.Title);
            _timeSection.style.display = mission.TimeLimit > 0f ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // el relleno de lo juntado sube suave
        _progressShown = Mathf.Lerp(_progressShown, progress, 1f - Mathf.Exp(-10f * dt));
        _progressFill.style.width = Length.Percent(_progressShown * 100f);
        _progressFill.style.backgroundColor = _count >= required ? SafeColor : WarnColor;

        SetText(_objective, $"Recolectar: {_count}/{required}");

        _pop = Mathf.MoveTowards(_pop, 0f, dt / 0.25f);
        float scale = 1f + 0.05f * _pop;

        if (mission.TimeLimit > 0f)
        {
            bool late = _manager.IsActiveMissionLate;
            float fraction = Mathf.Clamp01(_manager.TimeRemaining / mission.TimeLimit);
            bool danger = !late && fraction <= _dangerFraction;

            if (late)
            {
                SetText(_timeCaption, "¡Tarde! Perdés reputación");
                SetText(_time, "00:00");
                _timeFill.style.width = Length.Percent(0f);
                _timeCaption.style.color = DangerColor;
                _time.style.color = DangerColor;
                _time.style.opacity = 1f;
                scale += 0.03f * (Mathf.Sin(Time.unscaledTime * 8f) + 1f) * 0.5f;
            }
            else
            {
                SetText(_timeCaption, "Tiempo para entregar");
                SetText(_time, FormatTime(_manager.TimeRemaining));
                _timeFill.style.width = Length.Percent(fraction * 100f);
                _timeFill.style.backgroundColor = danger ? DangerColor : fraction <= _warnFraction ? WarnColor : SafeColor;
                _timeCaption.style.color = TextColor;
                _time.style.color = danger ? DangerColor : TextColor;

                // durante un diálogo el tiempo no corre: se ve apagado
                _time.style.opacity = _manager.IsTimerPaused ? 0.5f : 1f;

                if (danger) scale += 0.04f * (Mathf.Sin(Time.unscaledTime * 8f) + 1f) * 0.5f;
            }
        }

        _panel.style.scale = new Scale(new Vector3(scale, scale, 1f));
    }

    private static void SetText(Label label, string text)
    {
        if (label.text != text) label.text = text;
    }

    private static string FormatTime(float seconds)
    {
        int total = Mathf.CeilToInt(seconds);
        return $"{total / 60:00}:{total % 60:00}";
    }

    private static void SetBorder(VisualElement element, float width, Color color)
    {
        element.style.borderTopWidth = width;
        element.style.borderBottomWidth = width;
        element.style.borderLeftWidth = width;
        element.style.borderRightWidth = width;
        element.style.borderTopColor = color;
        element.style.borderBottomColor = color;
        element.style.borderLeftColor = color;
        element.style.borderRightColor = color;
    }

    private static void SetRadius(VisualElement element, float radius)
    {
        element.style.borderTopLeftRadius = radius;
        element.style.borderTopRightRadius = radius;
        element.style.borderBottomLeftRadius = radius;
        element.style.borderBottomRightRadius = radius;
    }
}