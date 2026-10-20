
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum EndingType
{
    Fae, // no volver a casa
    Depression, // perder reputacion
    Happy, // final dificil y feliz
    Sacrifice, // morir
    Community, // te salvan los aldeanos
    OneWithTheForest, // fallaste en ayudar a otros , pero estas bien con vos misma
    OneWithTheDust // la vida cae a 0
}

public class EndingHUD : MonoBehaviour
{
    public static EndingHUD Instance { get; private set; }

    [Header("Pantalla de finales")]
    [SerializeField] private UIDocument endingsHUD;

    [Header("HUDs para ocultar")]
    [SerializeField] private List<UIDocument> hudsToHide;

    private VisualElement happyEnding;
    private VisualElement depressionEnding;
    private VisualElement faeEnding;
    private VisualElement sacrificeEnding;
    private VisualElement communityEnding;
    private VisualElement oneWithTheForestEnding;
    private VisualElement oneWithTheDustEnding;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        var root = endingsHUD.rootVisualElement;

        happyEnding = root.Q<VisualElement>("HappyEnding");
        depressionEnding = root.Q<VisualElement>("DepressionEnding");
        faeEnding = root.Q<VisualElement>("FaeEnding");
        sacrificeEnding = root.Q<VisualElement>("SacrificeEnding");
        communityEnding = root.Q<VisualElement>("CommunityEnding");
        oneWithTheForestEnding = root.Q<VisualElement>("OneWithTheForestEnding");
        oneWithTheDustEnding = root.Q<VisualElement>("OneWithTheDustEnding");

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

        switch (ending)
        {
            case EndingType.Fae:
                faeEnding.style.display = DisplayStyle.Flex;
                break;

            case EndingType.Depression:
                depressionEnding.style.display = DisplayStyle.Flex;
                break;

            case EndingType.Happy:
                happyEnding.style.display = DisplayStyle.Flex;
                break;

            case EndingType.Sacrifice:
                sacrificeEnding.style.display = DisplayStyle.Flex;
                break;

            case EndingType.Community:
                sacrificeEnding.style.display = DisplayStyle.Flex;
                break;

            case EndingType.OneWithTheForest:
                sacrificeEnding.style.display = DisplayStyle.Flex;
                break;

            case EndingType.OneWithTheDust:
                sacrificeEnding.style.display = DisplayStyle.Flex;
                break;
        }
    }

    private void HideAllEndings()
    {
        happyEnding.style.display = DisplayStyle.None;
        depressionEnding.style.display = DisplayStyle.None;
        faeEnding.style.display = DisplayStyle.None;
        sacrificeEnding.style.display = DisplayStyle.None;
        communityEnding.style.display = DisplayStyle.None;
        oneWithTheForestEnding.style.display = DisplayStyle.None;
        oneWithTheDustEnding.style.display = DisplayStyle.None;
    }
    
}