using UnityEngine;
using UnityEngine.UIElements;

[DefaultExecutionOrder(1000)] // después de Cinemachine, para que el marcador no tiemble
public class QuestMarkerHUD : MonoBehaviour
{
    private static readonly Color Gold = new Color32(235, 185, 60, 255);
    private static readonly Color Mint = new Color32(120, 220, 190, 255);

    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private Transform _player;
    [SerializeField] private Camera _camera; // si está vacía usa Camera.main

    [Header("Marcador")]
    [SerializeField] private float _heightOffset = 3f;    // altura sobre el NPC a la que se ancla el ícono
    [SerializeField] private float _edgeMargin = 90f;     // px de panel entre el marcador y el borde
    [SerializeField] private float _hideDistance = 6f;    // más cerca que esto se oculta (ya se ve el resaltado)
    [SerializeField] private float _followSmoothTime = 0.06f;

    [Header("Animación")]
    [SerializeField] private float _bobAmplitude = 5f;    // vaivén del ícono sobre el NPC (px)
    [SerializeField] private float _bobSpeed = 3f;
    [SerializeField] private float _arrowPulse = 5f;      // cuánto late la flecha hacia afuera (px)

    private QuestGiver[] _givers;
    private VisualElement _root;
    private VisualElement _marker;
    private VisualElement _pivot;
    private VisualElement _arrow;
    private Label _icon;
    private Label _distanceLabel;

    private Color _accent = Gold;
    private Vector2 _current;
    private Vector2 _velocity;
    private bool _hasPosition;
    private int _lastDistance = -1;

    private void Start()
    {
        if (_camera == null) _camera = Camera.main;
        _givers = FindObjectsByType<QuestGiver>(FindObjectsSortMode.None);

        VisualElement document = _uiDocument.rootVisualElement;
        document.pickingMode = PickingMode.Ignore;

        _root = document.Q("QuestMarkerRoot");
        _marker = document.Q("Marker");
        _pivot = document.Q("Pivot");
        _arrow = document.Q("Arrow");
        _icon = document.Q<Label>("BadgeIcon");
        _distanceLabel = document.Q<Label>("DistanceLabel");

        if (_root == null || _marker == null || _pivot == null || _arrow == null || _icon == null || _distanceLabel == null)
        {
            Debug.LogError("QuestMarkerHUD: faltan elementos en el UXML. Revisá los nombres.", this);
            enabled = false;
            return;
        }

        _arrow.generateVisualContent += DrawArrow;
    }

    private void OnDisable()
    {
        if (_arrow != null) _arrow.generateVisualContent -= DrawArrow;
    }

    private void LateUpdate()
    {
        QuestGiver target = FindTarget();
        bool dialogueOpen = NarrativeDialogueManager.Instance != null && NarrativeDialogueManager.Instance.IsDialogueOpen;
        float distance = target != null ? Vector3.Distance(_player.position, target.transform.position) : 0f;

        bool visible = target != null && !dialogueOpen && distance > _hideDistance;
        _marker.EnableInClassList("marker--hidden", !visible);

        if (!visible)
        {
            _hasPosition = false; // al reaparecer salta directo a su lugar, sin deslizarse
            return;
        }

        float w = _root.resolvedStyle.width;
        float h = _root.resolvedStyle.height;
        if (float.IsNaN(w) || float.IsNaN(h) || w <= 0f || h <= 0f) return;

        Vector3 world = target.transform.position + Vector3.up * _heightOffset;
        Vector3 viewport = _camera.WorldToViewportPoint(world);
        bool behind = viewport.z < 0f;

        Vector2 center = new Vector2(w, h) * 0.5f;
        Vector2 point = new Vector2(viewport.x * w, (1f - viewport.y) * h);
        if (behind) point = center * 2f - point; // detrás de la cámara: se refleja para apuntar al lado correcto

        Vector2 dir = point - center;
        if (dir.sqrMagnitude < 1f) dir = new Vector2(0f, 1f); // justo detrás: abajo

        // si el punto cae fuera del rectángulo útil, se lleva hasta el borde en esa misma dirección
        float limitX = center.x - _edgeMargin;
        float limitY = center.y - _edgeMargin;
        float scale = Mathf.Min(limitX / Mathf.Max(Mathf.Abs(dir.x), 0.001f), limitY / Mathf.Max(Mathf.Abs(dir.y), 0.001f));

        bool offscreen = behind || scale < 1f;
        if (offscreen) point = center + dir * scale;
        else point.y += Mathf.Sin(Time.time * _bobSpeed) * _bobAmplitude;

        _current = _hasPosition ? Vector2.SmoothDamp(_current, point, ref _velocity, _followSmoothTime) : point;
        _hasPosition = true;

        _marker.style.left = _current.x;
        _marker.style.top = _current.y;
        _marker.EnableInClassList("marker--onscreen", !offscreen);

        if (offscreen)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            _pivot.style.rotate = new Rotate(new Angle(angle, AngleUnit.Degree));
            _arrow.style.translate = new Translate(Mathf.Sin(Time.time * 8f) * _arrowPulse, Length.Percent(-50f));
        }

        ApplyState(target.State == MissionState.InProgress, Mathf.RoundToInt(distance));
    }

    private QuestGiver FindTarget()
    {
        QuestGiver best = null;
        float bestSqr = float.MaxValue;

        foreach (QuestGiver giver in _givers)
        {
            if (!giver.CanInteract) continue;

            float sqr = (giver.transform.position - _player.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                best = giver;
                bestSqr = sqr;
            }
        }

        return best;
    }

    private void ApplyState(bool deliver, int distance)
    {
        _marker.EnableInClassList("marker--deliver", deliver);
        _icon.text = deliver ? "?" : "!";

        Color accent = deliver ? Mint : Gold;
        if (accent != _accent)
        {
            _accent = accent;
            _arrow.MarkDirtyRepaint();
        }

        if (distance != _lastDistance)
        {
            _lastDistance = distance;
            _distanceLabel.text = distance + " m";
        }
    }

    // punta de flecha con muesca, apuntando a la derecha (el pivot la rota hacia el NPC)
    private void DrawArrow(MeshGenerationContext context)
    {
        Painter2D painter = context.painter2D;
        Rect rect = _arrow.contentRect;

        painter.fillColor = _accent;
        painter.strokeColor = _accent;
        painter.lineWidth = 4f;
        painter.lineJoin = LineJoin.Round;

        painter.BeginPath();
        painter.MoveTo(new Vector2(2f, 2f));
        painter.LineTo(new Vector2(rect.width - 2f, rect.height * 0.5f));
        painter.LineTo(new Vector2(2f, rect.height - 2f));
        painter.LineTo(new Vector2(rect.width * 0.3f, rect.height * 0.5f));
        painter.ClosePath();
        painter.Fill();
        painter.Stroke();
    }
}