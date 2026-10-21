using UnityEngine;

// Pone la música de la escena al arrancar (el AudioManager persiste entre escenas)
public class SceneMusic : MonoBehaviour
{
    [SerializeField] private AudioClip _clip;

    private void Start()
    {
        if (AudioManager.Instance != null && _clip != null)
            AudioManager.Instance.PlayMusic(_clip);
    }
}