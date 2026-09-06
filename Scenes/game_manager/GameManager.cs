using Godot;
using System;

public partial class GameManager : Node
{
    private const int DURATION_IMPACT_PAUSE = 100;
    public float DURATION_GAME_SEC = 1 * 30;
    public float IN_GAME_MINUTES_PER_HALF = 20;

    public enum State { IN_PLAY, SCORED, RESET, KICKOFF, OVERTIME, GAMEOVER }

    public Match currentMatch = null;
    public GameState currentState = null;
    public int[] playerSetup = new int[] { 0, -2 };
    public GameStateFactory stateFactory = new GameStateFactory();
    public float TimePlayed;
    public ulong timeSincePaused = Time.GetTicksMsec();
    public GameEvents gameEvents;
    public float minutesPerSecond
    {
        get 
        {
            return IN_GAME_MINUTES_PER_HALF / DURATION_GAME_SEC;
        }
    }

    public float TimeElapsed
    {
        get
        {
            return TimePlayed * minutesPerSecond 
                     + ((currentMatch.Half - 1) * IN_GAME_MINUTES_PER_HALF);
        }
    }

    public bool GoalJustScored { get; set; } = false;
    public bool PlayTilWinnerIsScored { get; set; } = false;
    public bool OT_IS_ENDLESS_GOLDEN_GOAL_AFTER_SECOND_HALF_CONCLUDES = true;
    public float InjuryTime { get; set; } = 0;

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
        TimePlayed = 0;
        InjuryTime = 0;
        SwitchState(State.RESET);
    }

    public void StartHalf()
    {
        TimePlayed = 0;
        InjuryTime = 0;
        SwitchState(State.RESET);
    }

    public void SwitchState(State state, GameStateData data = null)
    {
        data ??= new GameStateData();

        // 🧹 Clean up the current state
        if (currentState != null)
        {
            currentState.StateTransitionRequested -= SwitchState; // 🔌 Disconnect to prevent dangling callables
            currentState.QueueFree(); // 🧨 Dispose safely
            currentState = null;      // 🚫 Null out to ensure no accidental reuse
        }

        // 🌱 Create and set up new state
        currentState = stateFactory.GetFreshState(state);
        currentState.Setup(this, data);
        currentState.Name = $"GameStateMachine: {state}";

        // 🔁 Connect signal only if valid
        currentState.StateTransitionRequested += SwitchState;

        // 🐣 Add to scene tree safely
        CallDeferred("add_child", currentState);
    }

    public bool IsCoop() => playerSetup[0] == playerSetup[1];
    public bool IsSinglePlayer() => playerSetup[1] == -2;
    public bool IsTimeUp() => TimePlayed >= DURATION_GAME_SEC + InjuryTime;

    public int GetWinningTeam()
    {
        return currentMatch.Winner;
    }

    public void IncreaseScore(int intScoredOn)
    {
        currentMatch.IncreaseScore(intScoredOn, TimeElapsed);
        gameEvents.EmitScoreChanged(intScoredOn);
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