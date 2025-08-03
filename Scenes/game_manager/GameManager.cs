using Godot;
using System;

public partial class GameManager : Node
{
    private const int DURATION_IMPACT_PAUSE = 100;
    private const float DURATION_GAME_SEC = 2 * 60;

    public enum State { IN_PLAY, SCORED, RESET, KICKOFF, OVERTIME, GAMEOVER }

    public Match currentMatch = null;
    public GameState currentState = null;
    public string[] playerSetup = new string[] { "FRANCE", "" };
    public GameStateFactory stateFactory = new GameStateFactory();
    public float timeLeft;
    public ulong timeSincePaused = Time.GetTicksMsec();
    public GameEvents gameEvents;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        gameEvents.ImpactReceived += OnImpactReceived;
    }

    public override void _Process(double delta)
    {
        if (GetTree().Paused && Time.GetTicksMsec() - timeSincePaused > DURATION_IMPACT_PAUSE)
        {
            GetTree().Paused = false;
        }
    }

    public void StartGame()
    {
        timeLeft = DURATION_GAME_SEC;
        SwitchState(State.RESET);
    }

    public void SwitchState(State state, GameStateData data = null)
    {
        data ??= new GameStateData();

        // Clean up the current state
        if (currentState != null)
        {
            currentState.StateTransitionRequested -= SwitchState; // Disconnect to prevent dangling callables
            currentState.QueueFree(); // Dispose safely
            RemoveChild(currentState);
            currentState = null;      // Null out to ensure no accidental reuse
        }

        // Create and set up new state
        currentState = stateFactory.GetFreshState(state);
        currentState.Setup(this, data);
        currentState.Name = $"GameStateMachine: {state}";

        // 🔁 Connect signal only if valid
        currentState.StateTransitionRequested += SwitchState;

        // 🐣 Add to scene tree safely
        CallDeferred("add_child", currentState);
    }

    public bool IsCoop() => playerSetup[0] == playerSetup[1];
    public bool IsSinglePlayer() => string.IsNullOrEmpty(playerSetup[1]);
    public bool IsTimeUp() => timeLeft <= 0;

    public string GetWinnerCountry()
    {
        return currentMatch.Winner;
    }

    public void IncreaseScore(string countryScoredOn)
    {
        currentMatch.IncreaseScore(countryScoredOn);
        gameEvents.EmitScoreChanged();
    }

    private void OnImpactReceived(Vector2 impactPosition, bool isHighImpact)
    {
        if (isHighImpact)
        {
            timeSincePaused = Time.GetTicksMsec();
            GetTree().Paused = true;
        }
    }
}