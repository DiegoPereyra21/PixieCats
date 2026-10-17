
using UnityEngine;
using UnityEngine.UIElements;

public class FairyPopupPosition : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Transform target;
    [SerializeField] private Camera targetCamera;

    [Header("Posicion")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Vector2 screenOffset = new Vector2(0f, -15f);

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

        if (screenPosition.z <= 0f)
        {
            popupPanel.style.display = DisplayStyle.None;
            return;
        }

        Vector2 screenPoint = new Vector2(screenPosition.x, Screen.height - screenPosition.y);

        Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(uiDocument.rootVisualElement.panel, screenPoint);

        popupPanel.style.left = panelPosition.x + screenOffset.x;
        popupPanel.style.top = panelPosition.y - screenOffset.y;
    }
}
