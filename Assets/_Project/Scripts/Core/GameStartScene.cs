using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// Escena de inicio de partida: muestra los textos de GameStart.asset uno por uno
// (click o cualquier tecla para seguir, Esc para saltear todo) y después pasa al juego
public class GameStartScene : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private NarrativeDialogueData _texts;
    [SerializeField] private NarrativeDialogueData _newGamePlusTexts; // si queda vacío, New Game+ usa los mismos textos
    [SerializeField] private string _nextScene = "Gameplay Katze";

    [Header("UXML")]
    [SerializeField] private string _labelName = "GameStartText"; // si no existe usa el primer Label de la escena
    [SerializeField] private string _hintName = "GameStartHint";  // opcional: label con el aviso para continuar
    [SerializeField] private string _hintText = "Click para continuar";

    [Header("Texto")]
    [SerializeField, Min(10f)] private float _fontSize = 46f;
    [SerializeField] private Color _textColor = Color.white;
    [SerializeField, Range(0f, 1f)] private float _boxAlpha = 0.6f; // oscuridad de la caja detrás del texto
    [SerializeField, Range(40f, 100f)] private float _boxWidthPercent = 84f;

    [Header("Ritmo")]
    [SerializeField, Min(1f)] private float _charsPerSecond = 45f; // velocidad de escritura
    [SerializeField, Min(0f)] private float _minLineTime = 0.5f;   // hasta que pase no se puede seguir a la próxima línea
    [SerializeField, Min(0f)] private float _autoAdvanceSeconds;   // 0 = solo con click o tecla
    [SerializeField, Min(0.05f)] private float _fadeTime = 0.35f;

    private VisualElement _box;
    private Label _label;
    private Label _hint;
    private bool _skipAll;

    private IEnumerator Start()
    {
        // el juego se carga en segundo plano mientras se leen los textos
        AsyncOperation load = SceneManager.LoadSceneAsync(_nextScene);
        load.allowSceneActivation = false;

        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();

        if (_uiDocument != null)
        {
            VisualElement root = _uiDocument.rootVisualElement;
            _label = root.Q<Label>(_labelName) ?? root.Q<Label>();
            _hint = root.Q<Label>(_hintName);

            if (_label != null) BuildTextBox(root);
        }

        NarrativeDialogueData texts = GameSession.IsNewGamePlus && _newGamePlusTexts != null ? _newGamePlusTexts : _texts;

        if (_label == null || texts == null || texts.Lines == null || texts.Lines.Length == 0)
        {
            Debug.LogWarning("GameStartScene: falta el label, el asset de textos o no tiene líneas; paso directo al juego.", this);
            load.allowSceneActivation = true;
            yield break;
        }

        foreach (NarrativeDialogueLine line in texts.Lines)
        {
            yield return ShowLine(line.Text);

            if (_skipAll) break;
        }

        load.allowSceneActivation = true;
    }

    // el texto va centrado en una caja oscura con márgenes, así no se sale de la pantalla y se lee sobre cualquier fondo
    private void BuildTextBox(VisualElement root)
    {
        _box = new VisualElement();
        _box.pickingMode = PickingMode.Ignore;
        _box.style.position = Position.Absolute;
        _box.style.left = Length.Percent((100f - _boxWidthPercent) * 0.5f);
        _box.style.width = Length.Percent(_boxWidthPercent);
        _box.style.top = Length.Percent(50f);
        _box.style.translate = new Translate(0f, Length.Percent(-50f));
        _box.style.paddingTop = 28;
        _box.style.paddingBottom = 28;
        _box.style.paddingLeft = 40;
        _box.style.paddingRight = 40;
        _box.style.backgroundColor = new Color(0f, 0f, 0f, _boxAlpha);
        _box.style.borderTopLeftRadius = 18;
        _box.style.borderTopRightRadius = 18;
        _box.style.borderBottomLeftRadius = 18;
        _box.style.borderBottomRightRadius = 18;
        _box.style.opacity = 0f;

        _label.RemoveFromHierarchy();
        _label.pickingMode = PickingMode.Ignore;
        _label.enableRichText = true;
        _label.style.position = Position.Relative;
        _label.style.left = StyleKeyword.Auto;
        _label.style.right = StyleKeyword.Auto;
        _label.style.top = StyleKeyword.Auto;
        _label.style.bottom = StyleKeyword.Auto;
        _label.style.width = Length.Percent(100f);
        _label.style.marginLeft = 0;
        _label.style.marginRight = 0;
        _label.style.marginTop = 0;
        _label.style.marginBottom = 0;
        _label.style.fontSize = _fontSize;
        _label.style.unityFontStyleAndWeight = FontStyle.Bold;
        _label.style.unityTextAlign = TextAnchor.MiddleCenter;
        _label.style.whiteSpace = WhiteSpace.Normal;
        _label.style.color = _textColor;
        _label.text = string.Empty;

        _box.Add(_label);
        root.Add(_box);

        if (_hint != null)
        {
            _hint.text = _hintText;
            _hint.style.display = DisplayStyle.None;
            _hint.style.position = Position.Absolute;
            _hint.style.left = 0;
            _hint.style.right = 0;
            _hint.style.top = StyleKeyword.Auto;
            _hint.style.bottom = Length.Percent(6f);
            _hint.style.fontSize = _fontSize * 0.6f;
            _hint.style.unityTextAlign = TextAnchor.MiddleCenter;
            _hint.style.color = _textColor;
            _hint.style.textShadow = new TextShadow { offset = new Vector2(2f, 2f), blurRadius = 4f, color = new Color(0f, 0f, 0f, 0.9f) };
        }
    }

    private IEnumerator ShowLine(string text)
    {
        text = string.IsNullOrEmpty(text) ? string.Empty : text;

        // el texto completo ocupa su lugar desde el principio (las letras que faltan van transparentes): la caja no salta mientras se escribe
        _label.text = Partial(text, 0);
        if (_hint != null) _hint.style.display = DisplayStyle.None;

        yield return Fade(0f, 1f);

        // escritura; un click o tecla la completa
        float shown = 0f;

        while (shown < text.Length)
        {
            if (SkipPressed()) { _skipAll = true; yield break; }
            if (AdvancePressed()) break;

            shown += _charsPerSecond * Time.deltaTime;
            _label.text = Partial(text, Mathf.Min(text.Length, Mathf.FloorToInt(shown)));
            yield return null;
        }

        _label.text = text;
        if (_hint != null) _hint.style.display = DisplayStyle.Flex;

        // espera para seguir
        float waited = 0f;

        while (true)
        {
            if (SkipPressed()) { _skipAll = true; yield break; }

            waited += Time.deltaTime;

            if (waited >= _minLineTime && AdvancePressed()) break;
            if (_autoAdvanceSeconds > 0f && waited >= _autoAdvanceSeconds) break;

            yield return null;
        }

        if (_hint != null) _hint.style.display = DisplayStyle.None;

        yield return Fade(1f, 0f);
    }

    private static string Partial(string text, int visible)
    {
        if (visible >= text.Length) return text;

        return text.Substring(0, visible) + "<color=#00000000>" + text.Substring(visible) + "</color>";
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < _fadeTime)
        {
            elapsed += Time.deltaTime;
            _box.style.opacity = Mathf.Lerp(from, to, elapsed / _fadeTime);
            yield return null;
        }

        _box.style.opacity = to;
    }

    private static bool AdvancePressed()
    {
        bool key = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool click = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        return key || click;
    }

    private static bool SkipPressed() => Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
}