using UnityEngine;
using UnityEngine.Serialization;

public class QuestBeacon : MonoBehaviour
{
    [FormerlySerializedAs("_giver")]
    [SerializeField] private MonoBehaviour _target; // un QuestGiver o el MushroomPortal (cualquier IGuideTarget)
    [SerializeField] private Transform _player;
    [SerializeField] private ParticleSystem[] _systems;

    [Header("Distancia")]
    [SerializeField] private float _hideDistance = 12f;  // más cerca que esto desaparece
    [SerializeField] private float _fullDistance = 30f;  // desde acá se ve al 100 %
    [SerializeField] private float _sizeDistance = 25f;  // desde acá las partículas crecen para verse de lejos
    [SerializeField] private float _maxSizeScale = 5f;
    [SerializeField] private float _fadeSmoothTime = 0.4f;

    private IGuideTarget _guide;
    private float[] _baseRates;
    private float[] _baseSizes;
    private float _alpha;
    private float _alphaVelocity;

    private void Awake()
    {
        _guide = _target as IGuideTarget;

        if (_guide == null)
        {
            Debug.LogError("QuestBeacon: _target tiene que ser un componente que implemente IGuideTarget.", this);
            enabled = false;
            return;
        }

        _baseRates = new float[_systems.Length];
        _baseSizes = new float[_systems.Length];

        for (int i = 0; i < _systems.Length; i++)
        {
            _baseRates[i] = _systems[i].emission.rateOverTimeMultiplier;
            _baseSizes[i] = _systems[i].main.startSizeMultiplier;
        }
    }

    private void Update()
    {
        float distance = Vector3.Distance(_player.position, transform.position);
        float target = _guide.IsGuideActive ? Mathf.InverseLerp(_hideDistance, _fullDistance, distance) : 0f;
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