using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TransitionScene : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private TransitionTextsSO _texts;
    [SerializeField] private string _nextScene = "Gameplay";
    [SerializeField] private float _duration = 3f; // segundos mínimos de pantalla negra

    private IEnumerator Start()
    {
        TransitionKind kind = GameSession.NextTransition;
        GameSession.SetTransition(TransitionKind.Normal);
        ShowText(kind);

        // carga la escena en segundo plano y la activa recién al cumplirse el tiempo
        AsyncOperation load = SceneManager.LoadSceneAsync(_nextScene);
        load.allowSceneActivation = false;

        yield return new WaitForSeconds(_duration);

        load.allowSceneActivation = true;
    }

    private void ShowText(TransitionKind kind)
    {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();

        Label label = _uiDocument != null ? _uiDocument.rootVisualElement.Q<Label>("TransitionText") : null;

        if (label == null || _texts == null)
        {
            Debug.LogError("TransitionScene: falta el UIDocument, el label 'TransitionText' o el asset de textos.", this);
            return;
        }

        label.text = _texts.GetText(kind);
    }
}