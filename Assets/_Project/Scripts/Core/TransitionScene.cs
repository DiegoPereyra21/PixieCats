using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionScene : MonoBehaviour
{
    [SerializeField] private string _nextScene = "Gameplay";
    [SerializeField] private float _duration = 3f; // segundos mínimos de pantalla negra

    private IEnumerator Start()
    {
        // carga Gameplay en segundo plano y la activa recién al cumplirse el tiempo
        AsyncOperation load = SceneManager.LoadSceneAsync(_nextScene);
        load.allowSceneActivation = false;

        yield return new WaitForSeconds(_duration);

        load.allowSceneActivation = true;
    }
}