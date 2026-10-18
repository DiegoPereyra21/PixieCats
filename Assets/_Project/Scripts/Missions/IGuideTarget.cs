using UnityEngine;

// Algo hacia lo que la flecha y la baliza guían al jugador
public interface IGuideTarget
{
    bool IsGuideActive { get; }
    Vector3 GuidePosition { get; }
}