using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private SceneLoader _sceneLoader;

    private Button _startButton;

    private void Start()
    {
        _startButton = _uiDocument.rootVisualElement.Q<Button>("StartButton");

        if (_startButton == null)
        {
            Debug.LogError("MainMenu: no se encontró 'StartButton' en el UXML.", this);
            return;
        }

        _startButton.clicked += OnStartClicked;
    }

    private void OnDisable()
    {
        if (_startButton != null) _startButton.clicked -= OnStartClicked;
    }

    private void OnStartClicked() => _sceneLoader.LoadTransition();
}