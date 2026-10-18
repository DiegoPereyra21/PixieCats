using UnityEngine;

public class QuestBeacon : MonoBehaviour
{
    [SerializeField] private QuestGiver _giver;
    [SerializeField] private Transform _player;
    [SerializeField] private ParticleSystem[] _systems;

    [Header("Distancia")]
    [SerializeField] private float _hideDistance = 12f;  // más cerca que esto desaparece
    [SerializeField] private float _fullDistance = 30f;  // desde acá se ve al 100 %
    [SerializeField] private float _sizeDistance = 25f;  // desde acá las partículas crecen para verse de lejos
    [SerializeField] private float _maxSizeScale = 5f;
    [SerializeField] private float _fadeSmoothTime = 0.4f;

    private float[] _baseRates;
    private float[] _baseSizes;
    private float _alpha;
    private float _alphaVelocity;
    private bool _active;

    private void Awake()
    {
        _baseRates = new float[_systems.Length];
        _baseSizes = new float[_systems.Length];

        for (int i = 0; i < _systems.Length; i++)
        {
            _baseRates[i] = _systems[i].emission.rateOverTimeMultiplier;
            _baseSizes[i] = _systems[i].main.startSizeMultiplier;
        }
    }

    private void OnEnable()
    {
        _giver.StateChanged += OnStateChanged;
        _active = _giver.CanInteract;
    }

    private void OnDisable() => _giver.StateChanged -= OnStateChanged;

    private void OnStateChanged(MissionState state) => _active = _giver.CanInteract;

    private void Update()
    {
        float distance = Vector3.Distance(_player.position, transform.position);
        float target = _active ? Mathf.InverseLerp(_hideDistance, _fullDistance, distance) : 0f;
        _alpha = Mathf.SmoothDamp(_alpha, target, ref _alphaVelocity, _fadeSmoothTime);

        float sizeScale = Mathf.Clamp(distance / _sizeDistance, 1f, _maxSizeScale);

        for (int i = 0; i < _systems.Length; i++)
        {
            ParticleSystem.EmissionModule emission = _systems[i].emission;
            emission.enabled = _alpha > 0.01f;
            emission.rateOverTimeMultiplier = _baseRates[i] * _alpha;

            ParticleSystem.MainModule main = _systems[i].main;
            main.startSizeMultiplier = _baseSizes[i] * sizeScale;
        }
    }
}