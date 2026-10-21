using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private SceneLoader _sceneLoader;
    [Header("Sonidos del menú")]
    [SerializeField] private AudioClip _hoverClip;
    [SerializeField] private AudioClip _clickClip;

    private Button _startButton, _optionsButton, _exitButton, _backButton;
    private VisualElement _mainButtons, _optionsPanel;
    private Slider _musicSlider, _sfxSlider;
    private Toggle _fullscreenToggle;
    private Button _newGamePlusButton;
    private void OnEnable()
    {
        if (_uiDocument == null || _sceneLoader == null)
        {
            Debug.LogError("MainMenu: asigná UI Document y Scene Loader en el Inspector.", this);
            return;
        }

        var root = _uiDocument.rootVisualElement;
        _startButton = root.Q<Button>("StartButton");
        _optionsButton = root.Q<Button>("OptionsButton");
        _exitButton = root.Q<Button>("ExitButton");
        _backButton = root.Q<Button>("BackButton");
        _mainButtons = root.Q<VisualElement>("MainButtons");
        _optionsPanel = root.Q<VisualElement>("OptionsPanel");
        _musicSlider = root.Q<Slider>("MusicSlider");
        _sfxSlider = root.Q<Slider>("SfxSlider");
        _fullscreenToggle = root.Q<Toggle>("FullscreenToggle");
        _newGamePlusButton = root.Q<Button>("NewGamePlusButton");


        if (_startButton == null || _optionsButton == null || _exitButton == null ||
            _backButton == null || _mainButtons == null || _optionsPanel == null ||
            _musicSlider == null || _sfxSlider == null || _fullscreenToggle == null)
        {
            Debug.LogError("MainMenu: faltan elementos en Menu.uxml.", this);
            return;
        }

        RegisterSounds(_startButton);
        RegisterSounds(_optionsButton);
        RegisterSounds(_exitButton);
        RegisterSounds(_backButton);

        if (_newGamePlusButton != null)
        {
            // solo aparece cuando algún final lo desbloqueó
            _newGamePlusButton.style.display = GameSession.NewGamePlusUnlocked ? DisplayStyle.Flex : DisplayStyle.None;
            RegisterSounds(_newGamePlusButton);
            _newGamePlusButton.clicked += OnNewGamePlusClicked;
        }

        _startButton.clicked += OnStartClicked;
        _optionsButton.clicked += ShowOptions;
        _exitButton.clicked += _sceneLoader.Quit;
        _backButton.clicked += ShowMain;
        _musicSlider.RegisterValueChangedCallback(OnMusicChanged);
        _sfxSlider.RegisterValueChangedCallback(OnSfxChanged);
        _fullscreenToggle.RegisterValueChangedCallback(OnFullscreenChanged);
        _fullscreenToggle.SetValueWithoutNotify(Screen.fullScreen);
        ShowMain();
    }

    private void OnDisable()
    {
        foreach (var button in new[] { _startButton, _optionsButton, _exitButton, _backButton, _newGamePlusButton })
        {
            if (button == null) continue;
            button.UnregisterCallback<PointerEnterEvent>(OnButtonHover);
            button.UnregisterCallback<ClickEvent>(OnButtonClick);
        }
        if (_startButton != null) _startButton.clicked -= OnStartClicked;
        if (_optionsButton != null) _optionsButton.clicked -= ShowOptions;
        if (_exitButton != null && _sceneLoader != null) _exitButton.clicked -= _sceneLoader.Quit;
        if (_backButton != null) _backButton.clicked -= ShowMain;
        if (_musicSlider != null) _musicSlider.UnregisterValueChangedCallback(OnMusicChanged);
        if (_sfxSlider != null) _sfxSlider.UnregisterValueChangedCallback(OnSfxChanged);
        if (_fullscreenToggle != null) _fullscreenToggle.UnregisterValueChangedCallback(OnFullscreenChanged);
        if (_newGamePlusButton != null) _newGamePlusButton.clicked -= OnNewGamePlusClicked;
    }

    private void OnStartClicked()
    {
        GameSession.Reset();
        GameSession.SetNewGamePlus(false);
        _sceneLoader.LoadGameStart();
    }

    private void OnNewGamePlusClicked()
    {
        GameSession.Reset();
        GameSession.SetNewGamePlus(true);
        _sceneLoader.LoadGameStart();
    }

    private void RegisterSounds(Button button)
    {
        button.RegisterCallback<PointerEnterEvent>(OnButtonHover);
        button.RegisterCallback<ClickEvent>(OnButtonClick);
    }

    private void OnButtonHover(PointerEnterEvent evt)
    {
        if (AudioManager.Instance != null && _hoverClip != null)
            AudioManager.Instance.PlaySfx(_hoverClip);
    }

    private void OnButtonClick(ClickEvent evt)
    {
        if (AudioManager.Instance != null && _clickClip != null)
            AudioManager.Instance.PlaySfx(_clickClip);
    }

    private void ShowOptions()
    {
        _mainButtons.style.display = DisplayStyle.None;
        _optionsPanel.style.display = DisplayStyle.Flex;
    }

    private void ShowMain()
    {
        _optionsPanel.style.display = DisplayStyle.None;
        _mainButtons.style.display = DisplayStyle.Flex;
    }

    private void OnMusicChanged(ChangeEvent<float> evt)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(evt.newValue);
    }

    private void OnSfxChanged(ChangeEvent<float> evt)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetSfxVolume(evt.newValue);
    }

    private void OnFullscreenChanged(ChangeEvent<bool> evt)
    {
        Screen.fullScreen = evt.newValue;
    }
}
