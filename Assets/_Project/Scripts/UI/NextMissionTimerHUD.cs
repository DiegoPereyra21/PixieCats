using UnityEngine;
using UnityEngine.UIElements;

// Cuenta regresiva para llegar a tiempo a la próxima misión (si se pasa, se pierde reputación)
public class NextMissionTimerHUD : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private MissionManager _manager;
    [SerializeField] private Font _font; // fuente del HUD (ej. Handlee-Regular)

    [Header("Aviso")]
    [SerializeField, Range(0f, 1f)] private float _warnFraction = 0.5f;    // el color pasa a ámbar
    [SerializeField, Range(0f, 1f)] private float _dangerFraction = 0.25f; // el color pasa a rojo y late
    [SerializeField, Min(0f)] private float _lateMessageTime = 3f;         // cuánto dura el aviso "Llegaste tarde"

    private static readonly Color TextColor = new Color32(0x49, 0x35, 0x22, 255);
    private static readonly Color PanelColor = new Color32(0xE8, 0xD5, 0xA9, 255);
    private static readonly Color SafeColor = new Color32(0x8A, 0x9A, 0x50, 255);
    private static readonly Color WarnColor = new Color32(0xC6, 0x9A, 0x55, 255);
    private static readonly Color DangerColor = new Color32(0xB0, 0x3A, 0x2E, 255);

    private VisualElement _panel;
    private VisualElement _barFill;
    private Label _title;
    private Label _time;
    private float _alpha;
    private float _lateTimer;

    private void Start()
    {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();

        if (_uiDocument == null || _manager == null)
        {
            Debug.LogError("NextMissionTimerHUD: falta el UIDocument o el MissionManager.", this);
            enabled = false;
            return;
        }

        BuildUI(_uiDocument.rootVisualElement);
        _manager.AcceptDeadlineMissed += OnDeadlineMissed;
    }

    private void OnDestroy()
    {
        if (_manager != null) _manager.AcceptDeadlineMissed -= OnDeadlineMissed;
    }

    private void OnDeadlineMissed(MissionDataSO next) => _lateTimer = _lateMessageTime;

    private void BuildUI(VisualElement root)
    {
        _panel = new VisualElement();
        _panel.pickingMode = PickingMode.Ignore;
        _panel.style.position = Position.Absolute;
        _panel.style.top = 14;
        _panel.style.left = Length.Percent(50f);
        _panel.style.alignItems = Align.Center;
        _panel.style.minWidth = 260;
        _panel.style.paddingTop = 8;
        _panel.style.paddingBottom = 10;
        _panel.style.paddingLeft = 22;
        _panel.style.paddingRight = 22;
        _panel.style.backgroundColor = PanelColor;
        _panel.style.display = DisplayStyle.None;
        _panel.style.opacity = 0f;
        SetBorder(_panel, 2f, TextColor);
        SetRadius(_panel, 14f);

        _title = CreateLabel(17f);
        _time = CreateLabel(38f);

        VisualElement bar = new VisualElement();
        bar.pickingMode = PickingMode.Ignore;
        bar.style.width = 220;
        bar.style.height = 10;
        bar.style.marginTop = 4;
        bar.style.overflow = Overflow.Hidden;
        bar.style.backgroundColor = TextColor;
        SetRadius(bar, 5f);

        _barFill = new VisualElement();
        _barFill.pickingMode = PickingMode.Ignore;
        _barFill.style.height = Length.Percent(100f);
        SetRadius(_barFill, 5f);
        bar.Add(_barFill);

        _panel.Add(_title);
        _panel.Add(_time);
        _panel.Add(bar);
        root.Add(_panel);
    }

    private Label CreateLabel(float size)
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
        label.style.fontSize = size;
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.whiteSpace = WhiteSpace.NoWrap;
        label.style.color = TextColor;

        if (_font != null) label.style.unityFontDefinition = FontDefinition.FromFont(_font);

        return label;
    }

    private void Update()
    {
        if (_panel == null) return;

        float dt = Time.unscaledDeltaTime;

        MissionDataSO next = _manager.NextMission;
        bool running = _manager.IsAcceptTimerRunning && next != null;

        if (running) _lateTimer = 0f;
        else if (_lateTimer > 0f) _lateTimer -= dt;

        bool late = !running && _lateTimer > 0f;
        bool visible = running || late;

        _alpha = Mathf.MoveTowards(_alpha, visible ? 1f : 0f, dt / 0.25f);
        _panel.style.display = _alpha > 0f ? DisplayStyle.Flex : DisplayStyle.None;
        _panel.style.opacity = _alpha;

        float scale = 1f;

        if (running)
        {
            float total = Mathf.Max(0.01f, next.AcceptTimeLimit);
            float remaining = _manager.AcceptTimeRemaining;
            float fraction = Mathf.Clamp01(remaining / total);
            bool danger = fraction <= _dangerFraction;

            SetText(_title, $"Próxima misión: {RequesterName(next)}");
            SetText(_time, FormatTime(remaining));

            _barFill.style.width = Length.Percent(fraction * 100f);
            _barFill.style.backgroundColor = danger ? DangerColor : fraction <= _warnFraction ? WarnColor : SafeColor;

            _title.style.color = TextColor;
            _time.style.color = danger ? DangerColor : TextColor;

            // durante un diálogo el tiempo no corre: se ve apagado
            _time.style.opacity = _manager.IsTimerPaused ? 0.5f : 1f;

            if (danger) scale += 0.04f * (Mathf.Sin(Time.unscaledTime * 8f) + 1f) * 0.5f;
        }
        else if (late)
        {
            SetText(_title, "¡Llegaste tarde! Perdés reputación");
            SetText(_time, "00:00");

            _barFill.style.width = Length.Percent(0f);
            _title.style.color = DangerColor;
            _time.style.color = DangerColor;
            _time.style.opacity = 1f;
        }

        _panel.style.translate = new Translate(Length.Percent(-50f), (1f - _alpha) * -20f);
        _panel.style.scale = new Scale(new Vector3(scale, scale, 1f));
    }

    private static void SetText(Label label, string text)
    {
        if (label.text != text) label.text = text;
    }

    private static string RequesterName(MissionDataSO mission) => mission.Requester != null ? mission.Requester.CharacterName : mission.Title;

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