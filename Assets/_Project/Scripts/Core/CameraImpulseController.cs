using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraImpulseController : MonoBehaviour
{
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private PlayerCarrier _carrier;

    [Header("Aterrizaje")]
    [SerializeField] private float _maxLandForce = 0.2f;
    [SerializeField] private float _maxLandImpact = 20f; // impacto con el que llega a la fuerza máxima

    [Header("Objetos")]
    [SerializeField] private float _maxItemForce = 0.3f;
    [SerializeField] private float _heavyWeight = 20f;   // peso con el que llega a la fuerza máxima
    [SerializeField] private float _minItemWeight = 3f;  // por debajo de este peso no sacude

    private CinemachineImpulseSource _source;

    private void Awake() => _source = GetComponent<CinemachineImpulseSource>();

    private void OnEnable()
    {
        _movement.Landed += OnLanded;
        _carrier.ItemStored += OnItemMoved;
        _carrier.ItemDropped += OnItemMoved;
    }

    private void OnDisable()
    {
        _movement.Landed -= OnLanded;
        _carrier.ItemStored -= OnItemMoved;
        _carrier.ItemDropped -= OnItemMoved;
    }

    private void OnLanded(float impact)
    {
        _source.GenerateImpulseWithForce(Mathf.Clamp01(impact / _maxLandImpact) * _maxLandForce);
    }

    private void OnItemMoved(float weight)
    {
        if (weight < _minItemWeight) return;
        _source.GenerateImpulseWithForce(Mathf.Clamp01(weight / _heavyWeight) * _maxItemForce);
    }
}