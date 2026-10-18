using System;
using UnityEngine;

[RequireComponent(typeof(InteractionHighlight))]
public class QuestGiver : MonoBehaviour, IInteractable, IGuideTarget
{
    [SerializeField] private MissionManager _manager;
    [SerializeField] private MissionDataSO _mission;
    [SerializeField] private SpawnArea _spawnArea;

    public MissionState State => _manager.GetState(_mission);
    public bool CanInteract => State == MissionState.Available || State == MissionState.InProgress;
    public bool IsBlocked => false;
    public bool WasDeliveredLate => _manager.WasLate(_mission); // para un diálogo distinto si se entregó tarde
    public bool IsGuideActive => CanInteract;
    public Vector3 GuidePosition => transform.position;

    // para los diálogos: cambia el estado de este NPC / se interactúa con él (con el estado previo a la acción)
    public event Action<MissionState> StateChanged;
    public event Action<MissionState> Interacted;

    private InteractionHighlight _highlight;
    private MissionState _lastState;
    private NarrativeDialogueData _pendingIntro; // diálogo abierto que termina en aceptar la misión

    private void Awake()
    {
        _highlight = GetComponent<InteractionHighlight>();
        if (_highlight == null) _highlight = gameObject.AddComponent<InteractionHighlight>();
    }

    private void OnEnable()
    {
        _lastState = State;
        _manager.StateChanged += OnManagerStateChanged;
    }

    private void OnDisable()
    {
        _manager.StateChanged -= OnManagerStateChanged;
        CancelPendingIntro();
    }

    public void Interact(PlayerInteractor interactor)
    {
        MissionState state = State;
        Interacted?.Invoke(state);

        if (state == MissionState.Available) BeginAccept();
        else if (state == MissionState.InProgress) _manager.TryDeliver(_mission, interactor.Carrier);
    }

    public void SetHighlighted(bool highlighted) => _highlight.Set(highlighted);

    // abre el diálogo de presentación del NPC y acepta la misión al cerrarlo; sin diálogo acepta directo
    private void BeginAccept()
    {
        DialogueCharacterData requester = _mission.Requester;
        NarrativeDialogueData intro = requester != null && requester.HasIntroDialogue ? requester.IntroDialogue : null;
        NarrativeDialogueManager dialogues = NarrativeDialogueManager.Instance;

        if (intro == null || dialogues == null)
        {
            Accept();
            return;
        }

        _pendingIntro = intro;
        _manager.PauseTimer(); // el tiempo para aceptar no corre mientras habla
        dialogues.DialogueEnded += OnDialogueEnded;

        if (!dialogues.StartDialogue(intro))
        {
            CancelPendingIntro();
            Accept();
        }
    }

    private void OnDialogueEnded(NarrativeDialogueData dialogue)
    {
        if (dialogue != _pendingIntro) return;

        CancelPendingIntro();
        Accept();
    }

    private void CancelPendingIntro()
    {
        if (_pendingIntro == null) return;

        _pendingIntro = null;
        _manager.ResumeTimer();

        if (NarrativeDialogueManager.Instance != null)
            NarrativeDialogueManager.Instance.DialogueEnded -= OnDialogueEnded;
    }

    private void Accept() => _manager.TryAccept(_mission, _spawnArea);

    private void OnManagerStateChanged()
    {
        MissionState state = State;
        if (state == _lastState) return;

        _lastState = state;
        StateChanged?.Invoke(state);
    }
}