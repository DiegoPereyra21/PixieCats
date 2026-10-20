using System.Linq;
using UnityEngine;

public class GuideTrail : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private ParticleSystem _trail;

    [Header("Forma")]
    [SerializeField] private float _trailLength = 10f;    // hasta dónde llega la estela desde el player
    [SerializeField] private float _startOffset = 3f;     // distancia desde el player donde empieza la estela
    [SerializeField] private float _hideDistance = 8f;    // más cerca del NPC que esto no se emite
    [SerializeField] private float _height = 0.8f;       // altura sobre los pies del player
    [SerializeField] private float _spread = 0.25f;

    [Header("Partículas")]
    [SerializeField] private float _particlesPerSecond = 40f;
    [SerializeField] private float _driftSpeed = 2f;      // avanzan hacia el NPC
    [SerializeField] private float _lifetime = 1.2f;

    private IGuideTarget[] _targets;
    private float _accumulator;

    private void Start()
    {
        _targets = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IGuideTarget>().ToArray();
        _trail.Play();
    }

    private void Update()
    {
        IGuideTarget target = FindTarget();
        if (target == null) return;

        Vector3 toTarget = target.GuidePosition - _player.position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        if (distance < _hideDistance) return;

        Vector3 dir = toTarget / distance;
        float length = Mathf.Min(_trailLength, distance - _hideDistance - _startOffset);
        if (length <= 0f) return;

        _accumulator += _particlesPerSecond * Time.deltaTime;

        while (_accumulator >= 1f)
        {
            _accumulator -= 1f;

            ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams
            {
                position = _player.position + dir * (_startOffset + Random.value * length) + Vector3.up * _height + Random.insideUnitSphere * _spread,
                velocity = dir * _driftSpeed,
                startLifetime = _lifetime
            };

            _trail.Emit(emit, 1);
        }
    }

    private IGuideTarget FindTarget()
    {
        IGuideTarget best = null;
        float bestSqr = float.MaxValue;

        foreach (IGuideTarget candidate in _targets)
        {
            if (!candidate.IsGuideActive) continue;

            float sqr = (candidate.GuidePosition - _player.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                best = candidate;
                bestSqr = sqr;
            }
        }

        return best;
    }
}