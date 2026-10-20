using UnityEngine;
using UnityEngine.UIElements;

[DefaultExecutionOrder(1000)] // después de Cinemachine, para que la burbuja no tiemble
public class FairyPopupPosition : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Transform target;
    [SerializeField] private Camera targetCamera;

    [Header("Posicion")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f); // altura de la cabeza
    [SerializeField] private Vector2 screenOffset = new Vector2(0f, -15f);    // px de panel; y negativo = más arriba


    private VisualElement popupPanel;

    private void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (uiDocument == null)
        {
            Debug.LogError("FairyPopupPosition: falta UIDocument.");
            enabled = false;
            return;
        }

        popupPanel = uiDocument.rootVisualElement.Q<VisualElement>("PopupPanel");

        if (popupPanel == null)
        {
            Debug.LogError("FairyPopupPosition: no se encontró PopupPanel.");
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (target == null || targetCamera == null || popupPanel == null)
            return;

        Vector3 worldPosition = target.position + worldOffset;
        Vector3 screenPosition = targetCamera.WorldToScreenPoint(worldPosition);

        // detrás de la cámara se oculta sin tocar display, que lo maneja el manager
        bool inFront = screenPosition.z > 0f;
        popupPanel.style.visibility = inFront ? Visibility.Visible : Visibility.Hidden;

        if (!inFront)
            return;

        Vector2 screenPoint = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
        Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(uiDocument.rootVisualElement.panel, screenPoint);

        // left/top son el punto de anclaje; el translate del UXML centra la burbuja arriba de ese punto
        popupPanel.style.left = panelPosition.x + screenOffset.x;
        popupPanel.style.top = panelPosition.y + screenOffset.y;
    }
}