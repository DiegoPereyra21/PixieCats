using UnityEngine;
using UnityEngine.UIElements;

public enum QuestArrowAnchor
{
    Player,         // la flecha gira alrededor del hada
    ScreenPosition  // la flecha queda fija en un punto de la pantalla
}

[DefaultExecutionOrder(1000)] // después de Cinemachine, para que la flecha no tiemble
public class QuestMarkerHUD : MonoBehaviour
{
    private static readonly Color Gold = new Color32(235, 185, 60, 255);
    private static readonly Color Outline = new Color32(15, 20, 40, 230);

    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private Transform _player;
    [SerializeField] private Camera _camera; // si está vacía usa Camera.main
    [Header("Imagen")]
    [SerializeField] private Sprite _arrowSprite; // si está vacía se dibuja la flecha por código

    [Header("Posición")]
    [SerializeField] private QuestArrowAnchor _anchorMode = QuestArrowAnchor.Player;
    [SerializeField] private float _anchorHeight = 1f;                         // altura sobre los pies del Player a la que se ancla
    [SerializeField] private float _orbitRadius = 110f;                        // px de panel entre el hada y la flecha (modo Player)
    [SerializeField] private Vector2 _screenAnchor = new Vector2(0.5f, 0.18f); // modo ScreenPosition (0,0 = arriba a la izquierda)

    [Header("Cuándo se muestra")]
    [SerializeField] private float _hideDistance = 6f; // más cerca que esto se oculta (ya se ve el resaltado)

    [Header("Animación")]
    [SerializeField] private float _angleSmoothTime = 0.1f;
    [SerializeField] private float _pulse = 4f; // cuánto late la flecha hacia afuera (px)

    private QuestGiver[] _givers;
    private VisualElement _root;
    private VisualElement _anchorElement;
    private VisualElement _pivot;
    private VisualElement _arrow;

    private float _heading;
    private float _headingVelocity;
    private bool _hasHeading;

    private void Start()
    {
        if (_camera == null) _camera = Camera.main;
        _givers = FindObjectsByType<QuestGiver>(FindObjectsSortMode.None);

        VisualElement document = _uiDocument.rootVisualElement;
        document.pickingMode = PickingMode.Ignore;

        _root = document.Q("QuestArrowRoot");
        _anchorElement = document.Q("ArrowAnchor");
        _pivot = document.Q("ArrowPivot");
        _arrow = document.Q("Arrow");

        if (_root == null || _anchorElement == null || _pivot == null || _arrow == null)
        {
            Debug.LogError("QuestMarkerHUD: faltan elementos en el UXML. Revisá los nombres.", this);
            enabled = false;
            return;
        }

        if (_arrowSprite != null) _arrow.style.backgroundImage = new StyleBackground(_arrowSprite);
        else _arrow.generateVisualContent += DrawArrow;
    }

    private void OnDisable()
    {
        if (_arrow != null) _arrow.generateVisualContent -= DrawArrow;
    }

    private void LateUpdate()
    {
        QuestGiver target = FindTarget();
        bool dialogueOpen = NarrativeDialogueManager.Instance != null && NarrativeDialogueManager.Instance.IsDialogueOpen;

        float w = _root.resolvedStyle.width;
        float h = _root.resolvedStyle.height;
        bool layoutReady = !float.IsNaN(w) && !float.IsNaN(h) && w > 0f && h > 0f;

        Vector3 toTarget = target != null ? target.transform.position - _player.position : Vector3.zero;

        if (target == null || dialogueOpen || !layoutReady || toTarget.magnitude <= _hideDistance)
        {
            Hide();
            return;
        }

        // brújula: ángulo horizontal entre hacia dónde mira la cámara y el NPC (0 = adelante, 90 = derecha)
        Vector3 flat = new Vector3(toTarget.x, 0f, toTarget.z);

        if (flat.sqrMagnitude > 0.25f)
        {
            Vector3 forward = Vector3.ProjectOnPlane(_camera.transform.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.ProjectOnPlane(_camera.transform.up, Vector3.up); // cámara mirando recto arriba o abajo

            float heading = Vector3.SignedAngle(forward, flat, Vector3.up);
            _heading = _hasHeading ? Mathf.SmoothDampAngle(_heading, heading, ref _headingVelocity, _angleSmoothTime) : heading;
            _hasHeading = true;
        }
        else if (!_hasHeading)
        {
            Hide(); // el NPC está justo arriba o abajo y todavía no hay dirección que mostrar
            return;
        }
        // si está justo arriba o abajo, conserva la última dirección

        Vector2 anchor;
        if (_anchorMode == QuestArrowAnchor.Player)
        {
            Vector3 viewport = _camera.WorldToViewportPoint(_player.position + Vector3.up * _anchorHeight);
            anchor = viewport.z > 0f
                ? new Vector2(viewport.x * w, (1f - viewport.y) * h)
                : new Vector2(w, h) * 0.5f;
        }
        else
        {
            anchor = new Vector2(_screenAnchor.x * w, _screenAnchor.y * h);
        }

        _anchorElement.style.left = anchor.x;
        _anchorElement.style.top = anchor.y;
        _anchorElement.EnableInClassList("arrow-anchor--hidden", false);

        // la flecha se dibuja apuntando a la derecha: arriba de la pantalla (adelante) = -90
        _pivot.style.rotate = new Rotate(new Angle(_heading - 90f, AngleUnit.Degree));

        float offset = _anchorMode == QuestArrowAnchor.Player ? _orbitRadius : -_arrow.contentRect.width * 0.5f;
        _arrow.style.translate = new Translate(offset + Mathf.Sin(Time.time * 6f) * _pulse, 0f);
    }

    private void Hide()
    {
        _anchorElement.EnableInClassList("arrow-anchor--hidden", true);
        _hasHeading = false; // al reaparecer salta directo a la dirección correcta
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

    // punta de flecha con muesca, apuntando a la derecha (el pivot la rota)
    private void DrawArrow(MeshGenerationContext context)
    {
        Painter2D painter = context.painter2D;
        Rect rect = _arrow.contentRect;
        const float margin = 6f;

        painter.lineJoin = LineJoin.Round;

        // primero el contorno oscuro (más grueso) y encima el relleno dorado
        DrawShape(painter, rect, margin, Outline, 10f);
        DrawShape(painter, rect, margin, Gold, 3f);
    }

    private static void DrawShape(Painter2D painter, Rect rect, float margin, Color color, float lineWidth)
    {
        painter.fillColor = color;
        painter.strokeColor = color;
        painter.lineWidth = lineWidth;

        painter.BeginPath();
        painter.MoveTo(new Vector2(margin, margin));
        painter.LineTo(new Vector2(rect.width - margin, rect.height * 0.5f));
        painter.LineTo(new Vector2(margin, rect.height - margin));
        painter.LineTo(new Vector2(rect.width * 0.35f, rect.height * 0.5f));
        painter.ClosePath();
        painter.Fill();
        painter.Stroke();
    }
}