using UnityEngine;

// Datos que sobreviven a la recarga de Gameplay (cada vuelta por el círculo de hongos)
public static class GameSession
{
    public static bool HasStats { get; private set; }
    public static float Wellbeing { get; private set; }
    public static float Reputation { get; private set; }

    // final alcanzado que la pantalla de transición debe mostrar (null = transición normal)
    public static EndingType? PendingEnding { get; private set; }

    public static void SaveStats(float wellbeing, float reputation)
    {
        Wellbeing = wellbeing;
        Reputation = reputation;
        HasStats = true;
    }

    public static void SetEnding(EndingType ending) => PendingEnding = ending;

    // partida nueva: bienestar y reputación vuelven a los valores de las reglas y se borra el final
    public static void Reset()
    {
        HasStats = false;
        PendingEnding = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay() => Reset(); // por si el dominio no se recarga al dar Play
}