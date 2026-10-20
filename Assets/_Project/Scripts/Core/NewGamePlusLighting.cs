using UnityEngine;

// En New Game+ el mapa se ve más iluminado
public class NewGamePlusLighting : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _ambientIntensity = 1.1f;

    private void Start()
    {
        if (!GameSession.IsNewGamePlus) return;

        RenderSettings.ambientIntensity = _ambientIntensity;
        DynamicGI.UpdateEnvironment();
    }
}