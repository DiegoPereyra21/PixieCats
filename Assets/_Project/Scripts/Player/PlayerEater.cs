using UnityEngine;

public class PlayerEater : MonoBehaviour
{
    [SerializeField] private PlayerInputReader _input;
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private PlayerFlight _flight;
    [SerializeField] private PlayerStats _stats;
    [SerializeField] private float _maxDistance = 3f; // alejarse más que esto cancela

    public bool IsEating => _target != null;

    private EdibleMushroom _target;
    private float _elapsed;

    public void StartEating(EdibleMushroom mushroom)
    {
        if (IsEating) return;

        _target = mushroom;
        _elapsed = 0f;
    }

    private void Update()
    {
        if (!IsEating) return;

        if (!CanKeepEating())
        {
            _target.SetEatProgress(0f);
            _target = null;
            return;
        }

        _elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(_elapsed / _target.EatTime);
        _target.SetEatProgress(progress);
        if (progress < 1f) return;

        _stats.Wellbeing.Add(_stats.Wellbeing.Max * _target.HealPercent);
        _target.Consume();
        _target = null;
    }

    // hay que mantener E, quieto, en el suelo y cerca del hongo
    private bool CanKeepEating()
    {
        if (!_target.gameObject.activeInHierarchy) return false;
        if (!_input.InteractHeld || _input.Move.sqrMagnitude > 0.01f) return false;
        if (_flight.IsFlying || !_movement.IsGrounded) return false;

        Vector3 offset = _target.transform.position - transform.position;
        offset.y = 0f;
        return offset.sqrMagnitude <= _maxDistance * _maxDistance;
    }
}