using UnityEngine;

public class SfxOnEvent : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO _channel;
    [SerializeField] private AudioClip _clip;

    private void OnEnable() => _channel.OnEventRaised += Play;
    private void OnDisable() => _channel.OnEventRaised -= Play;

    private void Play()
    {
        // sin clip o sin AudioManager (Play directo en Gameplay) no hace nada
        if (_clip == null || AudioManager.Instance == null) return;
        AudioManager.Instance.PlaySfx(_clip);
    }
}