
using System;
using UnityEngine;

public enum FairySpawnLocation
{
    FairyKingdomPortal,
    HollowTree
}

public enum GameProgressPhase
{
    NightExploration,
    DayTransition
}

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }
    public GameProgressPhase Phase { get; private set; }
    public FairySpawnLocation NextSpawnLocation { get; private set; }
    public FairySpawnLocation CurrentSpawnLocation { get; private set; }
    public bool HasEndingTriggered { get; private set; }

    [Header("Estadisticas del jugador")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Ciclo nocturno")]
    [SerializeField, Min(0.1f)]
    private float nightDurationMinutes = 5f;

    [Header("Dia Inicial")]
    [SerializeField, Min(1)]
    private int initialDay = 1;

    private const int MinutesPerHour = 60;
    private const int MinutesPerDay = 24 * MinutesPerHour;
    private const int NightStartMinute = 18 * MinutesPerHour;
    private const int DawnMinute = 6 * MinutesPerHour;
    private const int NightDurationGameMinutes = 12 * MinutesPerHour;
    private double totalGameMinutes;
    private long nightEndMinute;
    private bool clockPaused;
    private bool hasReturnedToKingdomLastNight = true;
    private bool hasHeavyCarryPenalty;
    private Stat wellbeingStat;
    private Stat reputationStat;

    public int CurrentDay => (int)(CurrentGameMinute / MinutesPerDay) + 1;

    public long CurrentGameMinute => (long)Math.Floor(totalGameMinutes);
    public double CurrentGameMinuteExact => totalGameMinutes;

    public int CurrentHour => (int)((CurrentGameMinute % MinutesPerDay) / MinutesPerHour);

    public int CurrentMinute => (int)(CurrentGameMinute % MinutesPerHour);

    public string CurrentTimeFormatted => $"{CurrentHour:00}:{CurrentMinute:00}";

    public float NightDurationMinutes
    {
        get => nightDurationMinutes;
        set => nightDurationMinutes = Mathf.Max(0.1f, value);
    }

    public bool IsClockPaused => clockPaused;

    public bool HasHeavyCarryPenalty => hasHeavyCarryPenalty;

    public bool HasReturnedToKingdomLastNight => hasReturnedToKingdomLastNight;

    public int NightsWithoutReturning { get; private set; }

    // notifica el intervalo de tiempo transcurrido
    public event Action<long, long> OnTimeUpdated;

    public event Action<int, FairySpawnLocation> OnNightStarted;
    public event Action OnDawnReached;
    public event Action<int> OnFailedReturn;
    public event Action OnReturnedToKingdom;

    public event Action<EndingType> OnEndingTriggered;
    // public event Action OnFaeEndingTriggered;
    //public event Action OnHumanEndingTriggered;
    //public event Action OnDepressionEndingTriggered;
    //public event Action OnGoodEndingTriggered;

    public event Action<bool> OnCarryPenaltyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // la noche comienza a las a las 18:00
        totalGameMinutes = (long)(initialDay - 1) * MinutesPerDay + NightStartMinute;

        NextSpawnLocation = FairySpawnLocation.FairyKingdomPortal;
        Phase = GameProgressPhase.DayTransition;
    }

    private void Start()
    {
        if (Instance != this)
            return;

        SubscribeToStats();

        if (HasEndingTriggered)
            return;

        BeginNight();
    }

    private void OnDestroy()
    {
        if (wellbeingStat != null)
            wellbeingStat.Changed -= OnWellbeingChanged;

        if (reputationStat != null)
            reputationStat.Changed -= OnReputationChanged;

        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (Phase != GameProgressPhase.NightExploration)
            return;

        if (clockPaused)
            return;

        long previousMinute = CurrentGameMinute;

        // la noche completa representa 12 horas de juego
        //double gameMinutesPerSecond = NightDurationGameMinutes / (nightDurationMinutes * 60.0);

        // la noche completa representa 24 horas de juego - temp
        double gameMinutesPerSecond = NightDurationGameMinutes / (nightDurationMinutes * 120.0);

        totalGameMinutes += Time.deltaTime * gameMinutesPerSecond;

        if (totalGameMinutes >= nightEndMinute)
            totalGameMinutes = nightEndMinute;

        NotifyTimeUpdated(previousMinute);

        if (totalGameMinutes >= nightEndMinute)
            HandleDawn();
    }

    private void BeginNight()
    {
        if (HasEndingTriggered)
            return;

        Phase = GameProgressPhase.NightExploration;

        CurrentSpawnLocation = NextSpawnLocation;

        nightEndMinute = CurrentGameMinute + NightDurationGameMinutes;

        OnNightStarted?.Invoke(CurrentDay, CurrentSpawnLocation);
    }

    private void HandleDawn()
    {
        if (Phase != GameProgressPhase.NightExploration)
            return;

        Phase = GameProgressPhase.DayTransition;
        OnDawnReached?.Invoke();

        // el hada no llego al portal a tiempo
        hasReturnedToKingdomLastNight = false;
        NightsWithoutReturning++;

        NextSpawnLocation = FairySpawnLocation.HollowTree;
        SetHeavyCarryPenalty(true);

        OnFailedReturn?.Invoke(NightsWithoutReturning);

        if (NightsWithoutReturning >= 2)
        {
            TriggerEnding(EndingType.Fae);
            return;
        }

        if (HasEndingTriggered)
            return;

        // el hada pasa el dia refugiada en el arbol (el calendario sigue avanzando aunque no explore)
        AdvanceToNextNight();
        BeginNight();
    }

    public void ReturnThroughMushroomCircle()
    {
        if (HasEndingTriggered)
            return;

        if (Phase != GameProgressPhase.NightExploration)
            return;

        // el regreso es inmediato, aunque todavia no sean las 06:00 (hora limite)
        hasReturnedToKingdomLastNight = true;
        NextSpawnLocation = FairySpawnLocation.FairyKingdomPortal;

        SetHeavyCarryPenalty(false);

        Phase = GameProgressPhase.DayTransition;
        OnReturnedToKingdom?.Invoke();

        // el calendario avanza hasta las 18:00 del dia siguiente
        AdvanceToNextNight();
        BeginNight();
    }

    private void AdvanceToNextNight()
    {
        long currentMinute = CurrentGameMinute;

        long currentDayStart = (currentMinute / MinutesPerDay) * MinutesPerDay;

        int minuteOfDay = (int)(currentMinute % MinutesPerDay);

        long nextNightStart = currentDayStart + NightStartMinute;

        // si ya estamos explorando y volvemos al reino de las hadas, salteamos hasta el dia siguiente
        if (minuteOfDay >= NightStartMinute)
            nextNightStart += MinutesPerDay;

        AdvanceTo(nextNightStart);
    }

    private void AdvanceTo(long targetMinute)
    {
        long previousMinute = CurrentGameMinute;

        totalGameMinutes = Math.Max(totalGameMinutes, targetMinute);

        NotifyTimeUpdated(previousMinute);
    }

    public void SetClockPaused(bool paused)
    {
        clockPaused = paused;
    }

    private void NotifyTimeUpdated(long previousMinute)
    {
        long currentMinute = CurrentGameMinute;

        if (currentMinute <= previousMinute)
            return;

        OnTimeUpdated?.Invoke(previousMinute, currentMinute);
    }

    private void SetHeavyCarryPenalty(bool active)
    {
        if (hasHeavyCarryPenalty == active)
            return;

        hasHeavyCarryPenalty = active;
        OnCarryPenaltyChanged?.Invoke(active);
    }

    public static string FormatGameTime(long absoluteMinute)
    {
        int minuteOfDay = (int)(absoluteMinute % MinutesPerDay);

        int hour = minuteOfDay / MinutesPerHour;
        int minute = minuteOfDay % MinutesPerHour;

        return $"{hour:00}:{minute:00}";
    }

    private void TriggerEnding(EndingType ending)
    {
        if (HasEndingTriggered)
            return;

        HasEndingTriggered = true;
        SetClockPaused(true);

        OnEndingTriggered?.Invoke(ending);
    }

    public void RegisterEndingHUD(EndingHUD endingHUD)
    {
        OnEndingTriggered -= endingHUD.ShowEnding;
        OnEndingTriggered += endingHUD.ShowEnding;
    }

    private void SubscribeToStats()
    {
        if (playerStats == null)
        {
            Debug.LogError("GameProgressManager: falta asignar PlayerStats.");
            return;
        }

        wellbeingStat = playerStats.Wellbeing;
        reputationStat = playerStats.Reputation;

        wellbeingStat.Changed += OnWellbeingChanged;
        reputationStat.Changed += OnReputationChanged;

        OnWellbeingChanged(wellbeingStat.Value, 0f);
        OnReputationChanged(reputationStat.Value, 0f);
    }

    private void OnWellbeingChanged(float value, float delta)
    {
        if (value <= wellbeingStat.Min)
            TriggerEnding(EndingType.Sacrifice);
    }

    private void OnReputationChanged(float value, float delta)
    {
        if (value <= reputationStat.Min)
            TriggerEnding(EndingType.Depression);
    }
}
