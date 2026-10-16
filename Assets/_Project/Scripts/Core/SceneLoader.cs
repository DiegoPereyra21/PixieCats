using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string _menuScene = "Menu";
    [SerializeField] private string _gameplayScene = "Gameplay";

    public void LoadMenu() => SceneManager.LoadScene(_menuScene);
    public void LoadGameplay() => SceneManager.LoadScene(_gameplayScene);

    public void Quit()
    {
        Application.Quit(); // no hace nada en WebGL
    }
}