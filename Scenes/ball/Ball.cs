using Godot;
using System;

public partial class Ball : AnimatableBody2D
{
    public const float BOUNCINESS = 0.8f;
    private const int DISTANCE_HIGH_PASS = 90;
    private const int DURATION_TUMBLE_LOCK = 200;
    private const int DURATION_PASS_LOCK = 500;
    private const float KICKOFF_PASS_DISTANCE = 30f;
    private const float TUMBLE_HEIGHT_VELOCITY = 3f;

    public enum State { CARRIED, FREEFORM, SHOT }

    [Export] public float FrictionAir { get; set; }
    [Export] public float FrictionGround { get; set; }

    private AnimationPlayer animationPlayer;
    private Sprite2D ballSprite;
    private Area2D playerDetectionArea;
    private Area2D playerProximityArea;
    private RayCast2D scoringRaycast;
    private GpuParticles2D shotParticles;

    public PlayerCharacter Carrier = null;
    public BallState CurrentState = null;
    public float Height = 0f;
    public float HeightVelocity = 0f;
    public Vector2 spawnPosition = Vector2.Zero;
    private BallStateFactory stateFactory = new BallStateFactory();
    public Vector2 Velocity = Vector2.Zero;
    public GameEvents gameEvents;

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        ballSprite = GetNode<Sprite2D>("BallSprite");
        playerDetectionArea = GetNode<Area2D>("PlayerDetectionArea");
        playerProximityArea = GetNode<Area2D>("PlayerProximityArea");
        scoringRaycast = GetNode<RayCast2D>("ScoringRaycast");
        shotParticles = GetNode<GpuParticles2D>("ShotParticles");

        AddChild(stateFactory);
        SwitchState(State.FREEFORM);
        spawnPosition = Position;
    }

    public override void _Process(double delta)
    {
        ballSprite.Position = Vector2.Up * Height;
        scoringRaycast.Rotation = Velocity.Angle();
    }

    public void SwitchState(State state, BallStateData data = null)
    {
        if (CurrentState != null)
        {
            CurrentState.QueueFree();
        }

        CurrentState = stateFactory.GetFreshState(state);
        CurrentState.Name = "BallStateMachine";

        CurrentState.Setup(this, data ?? new BallStateData(), playerDetectionArea, Carrier, animationPlayer, ballSprite, shotParticles);
        AddChild(CurrentState);
        CurrentState.StateTransitionRequested += SwitchState;
    }

    public void Shoot(Vector2 shotVelocity)
    {
        Velocity = shotVelocity;
        Carrier = null;
        SwitchState(State.SHOT);
    }

    public void Tumble(Vector2 tumbleVelocity)
    {
        Velocity = tumbleVelocity;
        Carrier = null;
        HeightVelocity = TUMBLE_HEIGHT_VELOCITY;
        SwitchState(State.FREEFORM, BallStateData.Build().SetLockDuration(DURATION_TUMBLE_LOCK));
    }

    public void PassTo(Vector2 destination, int lockDuration = DURATION_PASS_LOCK)
    {
        Vector2 direction = Position.DirectionTo(destination);
        float distance = Position.DistanceTo(destination);
        float intensity = Mathf.Sqrt(2 * distance * FrictionGround);

        Velocity = intensity * direction;

        if (distance > DISTANCE_HIGH_PASS)
        {
            HeightVelocity = BallState.Gravity * distance / (1.85f * intensity);
        }

        Carrier = null;
        SwitchState(State.FREEFORM, BallStateData.Build().SetLockDuration(lockDuration));
    }

    public void Stop()
    {
        Velocity = Vector2.Zero;
    }

    public bool CanAirInteract()
    {
        return CurrentState != null && CurrentState.CanAirInteract();
    }

    public bool CanAirConnect(float airConnectMinHeight, float airConnectMaxHeight)
    {
        return Height >= airConnectMinHeight && Height <= airConnectMaxHeight;
    }

    public bool IsHeadedForScoringArea(Area2D scoringArea)
    {
        if (!scoringRaycast.IsColliding())
            return false;

        return scoringRaycast.GetCollider() == scoringArea;
    }

    public int GetProximityTeammatesCount(string country)
    {
        var players = playerProximityArea.GetOverlappingBodies();
        int count = 0;

        foreach (var body in players)
        {
            if (body is PlayerCharacter p && p.country == country)
                count++;
        }

        return count;
    }

    private void OnTeamReset()
    {
        Position = spawnPosition;
        Velocity = Vector2.Zero;
        Height = 0f;
        SwitchState(State.FREEFORM);
    }

    private void OnKickoffStarted()
    {
        PassTo(spawnPosition + Vector2.Down * KICKOFF_PASS_DISTANCE, 0);
    }

    public override void _EnterTree()
    {
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        gameEvents.TeamResetEventTriggered += OnTeamReset;
        gameEvents.KickoffStarted += OnKickoffStarted;
    }


    public override void _ExitTree()
    {
        if (gameEvents != null)
        {
            gameEvents.KickoffStarted -= OnKickoffStarted;
            gameEvents.TeamResetEventTriggered -= OnTeamReset; // ✅ add this
        }   
    }
}
