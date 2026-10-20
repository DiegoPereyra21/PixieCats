using UnityEngine;

public enum TransitionKind { Normal, MissedDawn }

// Datos que sobreviven a la recarga de Gameplay (cada vuelta por el círculo de hongos)
public static class GameSession
{
    public static bool HasStats { get; private set; }
    public static float Wellbeing { get; private set; }
    public static float Reputation { get; private set; }

    // qué mensaje muestra la próxima pantalla de transición
    public static TransitionKind NextTransition { get; private set; }

    public static void SaveStats(float wellbeing, float reputation)
    {
        Wellbeing = wellbeing;
        Reputation = reputation;
        HasStats = true;
    }

    public static void SetTransition(TransitionKind kind) => NextTransition = kind;

    // partida nueva: bienestar y reputación vuelven a los valores de las reglas
    public static void Reset()
    {
        HasStats = false;
        NextTransition = TransitionKind.Normal;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay() => Reset(); // por si el dominio no se recarga al dar Play
}