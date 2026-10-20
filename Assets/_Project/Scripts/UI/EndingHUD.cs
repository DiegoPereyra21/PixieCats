using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public enum EndingType
{
    Fae, // no volver a casa (2 noches)
    Depression, // perder reputacion
    Happy, // New Game+: sobrevivir hasta el final de la tercera noche
    Sacrifice, // reputacion al 100 con la vida baja
    Community, // New Game+: te salvan los aldeanos
    OneWithTheForest, // New Game+: fallaste en ayudar a otros , pero estas bien con vos misma
    OneWithTheDust // la vida cae a 0
}

public class EndingHUD : MonoBehaviour
{
    public static EndingHUD Instance { get; private set; }

    [Header("Pantalla de finales")]
    [SerializeField] private UIDocument endingsHUD;
    [SerializeField] private string menuScene = "Menu";

    [Header("HUDs para ocultar")]
    [SerializeField] private List<UIDocument> hudsToHide;

    // cada panel del UXML se llama como el final + "Ending" (FaeEnding, CommunityEnding...)
    private readonly Dictionary<EndingType, VisualElement> panels = new Dictionary<EndingType, VisualElement>();
    private Button backButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        var root = endingsHUD.rootVisualElement;

        foreach (EndingType type in Enum.GetValues(typeof(EndingType)))
        {
            VisualElement panel = root.Q<VisualElement>(type + "Ending");

            if (panel == null)
                Debug.LogError($"EndingHUD: falta el panel '{type}Ending' en Endings.uxml.");
            else
                panels[type] = panel;
        }

        backButton = root.Q<Button>("BackToMenuButton");

        if (backButton != null)
            backButton.clicked += OnBackToMenu;
        else
            Debug.LogWarning("EndingHUD: falta 'BackToMenuButton' en Endings.uxml.");

        HideAllEndings();
    }

    private void Start()
    {
        if (GameProgressManager.Instance == null)
        {
            Debug.LogError("EndingHUD: no se encontro GameProgressManager.");
            return;
        }

        GameProgressManager.Instance.RegisterEndingHUD(this);

        if (GameProgressManager.Instance.HasEndingTriggered)
        {
            Debug.LogWarning("El final ya se habia activado antes de registrar EndingHUD.");
        }
    }

    private void OnDestroy()
    {
        if (backButton != null)
            backButton.clicked -= OnBackToMenu;

        if (GameProgressManager.Instance != null)
            GameProgressManager.Instance.OnEndingTriggered -= ShowEnding;

        if (Instance == this)
            Instance = null;
    }

    public void ShowEnding(EndingType ending)
    {
        foreach (UIDocument hud in hudsToHide)
        {
            if (hud != null)
                hud.enabled = false;
        }

        endingsHUD.enabled = true;
        HideAllEndings();

        if (panels.TryGetValue(ending, out VisualElement panel))
            panel.style.display = DisplayStyle.Flex;

        if (backButton != null)
            backButton.style.display = DisplayStyle.Flex;
    }

    private void HideAllEndings()
    {
        foreach (VisualElement panel in panels.Values)
            panel.style.display = DisplayStyle.None;

        if (backButton != null)
            backButton.style.display = DisplayStyle.None;
    }

    private void OnBackToMenu()
    {
        // GameProgressManager persiste entre escenas: se destruye para que la próxima partida arranque limpia
        if (GameProgressManager.Instance != null)
            Destroy(GameProgressManager.Instance.gameObject);

        SceneManager.LoadScene(menuScene);
    }
}