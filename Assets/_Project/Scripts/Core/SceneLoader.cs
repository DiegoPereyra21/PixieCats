using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string _menuScene = "Menu";
    [SerializeField] private string _gameplayScene = "Gameplay";
    [SerializeField] private string _transitionScene = "Transition";

    public void LoadTransition() => SceneManager.LoadScene(_transitionScene);
    public void LoadMenu() => SceneManager.LoadScene(_menuScene);
    public void LoadGameplay() => SceneManager.LoadScene(_gameplayScene);
    [SerializeField] private string _gameStartScene = "GameStart";

    public void LoadGameStart() => SceneManager.LoadScene(_gameStartScene);
    public void Quit()
    {
        Application.Quit(); // no hace nada en WebGL
    }
}