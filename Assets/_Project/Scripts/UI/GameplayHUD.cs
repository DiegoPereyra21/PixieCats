using UnityEngine;
using UnityEngine.UIElements;

// HUD de juego: reloj de la noche con la hora en grande, la noche actual y un anillo que avanza hasta el amanecer
public class GameplayHUD : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Font font; // fuente del HUD (ej. Handlee-Regular)

    [Header("Reloj")]
    [SerializeField, Min(60f)] private float clockSize = 124f;
    [SerializeField, Range(0f, 1f)] private float warnProgress = 0.6f;    // el anillo pasa a ámbar
    [SerializeField, Range(0f, 1f)] private float dangerProgress = 0.85f; // el anillo pasa a rojo y el reloj late

    private const int MinutesPerDay = 24 * 60;
    private const int NightStartMinute = 18 * 60;
    private const int NightLengthMinutes = 12 * 60;

    private static readonly Color TextColor = new Color32(0x49, 0x35, 0x22, 255);
    private static readonly Color TrackColor = new Color32(0x49, 0x35, 0x22, 70);
    private static readonly Color DuskColor = new Color32(0x5B, 0x5F, 0xA8, 255);
    private static readonly Color WarnColor = new Color32(0xC6, 0x9A, 0x55, 255);
    private static readonly Color DangerColor = new Color32(0xB0, 0x3A, 0x2E, 255);

    private GameProgressManager progressManager;
    private Label timeLabel;
    private Label nightLabel;
    private VisualElement clock;
    private VisualElement ring;

    private float ringProgress;
    private Color ringColor;
    private float drawnProgress = -1f;

    private void Start()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        progressManager = GameProgressManager.Instance;

        if (uiDocument == null || progressManager == null)
        {
            Debug.LogError("GameHUD: falta UIDocument o GameProgressManager.");
            enabled = false;
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        timeLabel = root.Q<Label>("TimeLabel");
        clock = root.Q<VisualElement>("DayClock");
        ring = root.Q<VisualElement>("DayPhaseIndicator");

        if (timeLabel == null || clock == null || ring == null)
        {
            Debug.LogError("GameHUD: faltan TimeLabel, DayClock o DayPhaseIndicator en el UXML.");
            enabled = false;
            return;
        }

        SetupClock();
        UpdateClock();
    }

    private void Update()
    {
        if (progressManager == null || timeLabel == null)
            return;

        UpdateClock();
    }

    private void SetupClock()
    {
        clock.style.width = clockSize;
        clock.style.height = clockSize;

        StyleLabel(timeLabel, clockSize * 0.28f);

        nightLabel = new Label();
        nightLabel.pickingMode = PickingMode.Ignore;
        StyleLabel(nightLabel, clockSize * 0.115f);
        clock.Add(nightLabel);

        // el anillo del UXML pasa a dibujar el progreso de la noche
        ring.pickingMode = PickingMode.Ignore;
        ring.style.borderTopWidth = 0;
        ring.style.borderBottomWidth = 0;
        ring.style.borderLeftWidth = 0;
        ring.style.borderRightWidth = 0;
        ring.style.left = 6;
        ring.style.right = 6;
        ring.style.top = 6;
        ring.style.bottom = 6;
        ring.generateVisualContent += DrawRing;
    }

    private void StyleLabel(Label label, float size)
    {
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

        if (font != null) label.style.unityFontDefinition = FontDefinition.FromFont(font);
    }

    private void UpdateClock()
    {
        SetText(timeLabel, progressManager.CurrentTimeFormatted);

        float progress = NightProgress();
        bool danger = progress >= dangerProgress;

        SetText(nightLabel, danger ? "¡Amanece!" : $"Noche {progressManager.NightsPlayed + 1}");
        nightLabel.style.color = danger ? DangerColor : TextColor;
        timeLabel.style.color = danger ? DangerColor : TextColor;

        ringProgress = progress;
        ringColor = ColorFor(progress);

        if (Mathf.Abs(ringProgress - drawnProgress) > 0.002f)
        {
            drawnProgress = ringProgress;
            ring.MarkDirtyRepaint();
        }

        float throb = danger ? (Mathf.Sin(Time.unscaledTime * 6f) + 1f) * 0.5f : 0f;
        float scale = 1f + 0.04f * throb;
        clock.style.scale = new Scale(new Vector3(scale, scale, 1f));
    }

    // 0 a las 18:00, 1 a las 06:00
    private float NightProgress()
    {
        double minuteOfDay = progressManager.CurrentGameMinuteExact % MinutesPerDay;
        double sinceNightStart = (minuteOfDay - NightStartMinute + MinutesPerDay) % MinutesPerDay;

        return Mathf.Clamp01((float)(sinceNightStart / NightLengthMinutes));
    }

    private Color ColorFor(float progress)
    {
        if (progress < warnProgress)
            return Color.Lerp(DuskColor, WarnColor, progress / Mathf.Max(0.01f, warnProgress));

        if (progress < dangerProgress)
            return Color.Lerp(WarnColor, DangerColor, (progress - warnProgress) / Mathf.Max(0.01f, dangerProgress - warnProgress));

        return DangerColor;
    }

    private void DrawRing(MeshGenerationContext context)
    {
        Rect rect = ring.contentRect;
        float width = Mathf.Max(5f, clockSize * 0.065f);
        float radius = Mathf.Min(rect.width, rect.height) * 0.5f - width * 0.5f;

        if (radius <= 0f) return;

        Painter2D painter = context.painter2D;
        painter.lineWidth = width;
        painter.lineCap = LineCap.Round;

        // pista
        painter.strokeColor = TrackColor;
        painter.BeginPath();
        painter.Arc(rect.center, radius, 0f, 360f);
        painter.Stroke();

        // progreso de la noche: arranca arriba y avanza en sentido horario
        if (ringProgress > 0.002f)
        {
            painter.strokeColor = ringColor;
            painter.BeginPath();
            painter.Arc(rect.center, radius, -90f, -90f + 360f * ringProgress);
            painter.Stroke();
        }
    }

    private static void SetText(Label label, string text)
    {
        if (label.text != text) label.text = text;
    }
}