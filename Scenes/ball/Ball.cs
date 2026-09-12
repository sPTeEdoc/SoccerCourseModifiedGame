using Godot;
using System;

public partial class Ball : AnimatableBody2D
{
    public const float BOUNCINESS = 0.65f; // â Reduced from 0.8 (less pinball-like)
    private const int DISTANCE_HIGH_PASS = 140; // â Increased from 120
    private const int DURATION_TUMBLE_LOCK = 200;
    private const int DURATION_PASS_LOCK = 400; // â Reduced from 500
    private const float KICKOFF_PASS_DISTANCE = 89f;
    private const float TUMBLE_HEIGHT_VELOCITY = 3f;

    // â CRITICAL: Adjust these to match what you declared in the inspector
    [Export] public float FrictionAir { get; set; } = 25f;  // Was 35
    [Export] public float FrictionGround { get; set; } = 420f; // Was 350

    public enum State { CARRIED, FREEFORM, SHOT }

    private AnimationPlayer animationPlayer;
    private Sprite2D ballSprite;
    private Sprite2D shadowSprite;
    private CollisionShape2D collisionShape;
    private Area2D playerDetectionArea;
    private Area2D playerProximityArea;
    private RayCast2D scoringRaycast;
    private GpuParticles2D shotParticles;

    public PlayerCharacter Carrier { get; set; } = null;
    public BallState CurrentState = null;
    public float Height = 0f;
    public float HeightVelocity = 0f;
    public Vector2 spawnPosition = Vector2.Zero;
    private BallStateFactory stateFactory = new BallStateFactory();
    public Vector2 Velocity = Vector2.Zero;
    public GameEvents gameEvents;
    public float CrossbarHeight { get; set; } = 32f;
    public bool IsInNet { get; set; } = false;

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        ballSprite = GetNode<Sprite2D>("BallSprite");
        shadowSprite = GetNode<Sprite2D>("ShadowSprite");
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
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

    public void PassTo(Vector2 destination, int lockDuration = DURATION_PASS_LOCK, PlayerCharacter receiver = null)
    {
        Vector2 direction = (destination - Position).Normalized();
        float rawDistance = Position.DistanceTo(destination);
        float effectiveDistance = Mathf.Max(rawDistance, 60f); // Minimum pass distance

        // â SENSIBLE SOCCER PHYSICS: Use CONSTANT velocity, not deceleration model
        // Passes should be FAST and direct, not physics-simulated rolling
        float basePassSpeed = 280f; // Was solving for deceleration - now fixed speed

        // â Distance scaling - longer passes slightly faster (not exponential)
        float distanceMultiplier = 1f + Mathf.Clamp((effectiveDistance - 100f) / 300f, 0f, 0.35f);
        float passSpeed = basePassSpeed * distanceMultiplier;

        // â Passing skill affects ACCURACY, not speed
        if (Carrier != null)
        {
            float passingSkill = Carrier.GameAttributes.Passing;
            float skillFactor = Mathf.InverseLerp(50f, 99f, passingSkill);

            // â Accuracy deviation (not power bonus)
            float maxDeviation = Mathf.Lerp(8f, 1.5f, skillFactor); // 8Â° to 1.5Â° error
            float angleError = (GD.Randf() * 2f - 1f) * maxDeviation;
            direction = direction.Rotated(Mathf.DegToRad(angleError));
        }

        Velocity = direction * passSpeed;

        // â GROUND PASSES BY DEFAULT - only lob if distance > 180px
        bool shouldLob = rawDistance > 180f;

        if (shouldLob)
        {
            // â LOW, FAST lob (not slow floaty arc)
            HeightVelocity = 4.5f; // Was 6-8, now much flatter
            Height = 2f; // Start slightly elevated
        }
        else
        {
            // Ground pass - zero height
            HeightVelocity = 0f;
            Height = 0f;
        }

        // Store passer reference BEFORE clearing Carrier
        PlayerCharacter passer = Carrier;
        Carrier = null;

        // Transition to freeform state
        SwitchState(State.FREEFORM, BallStateData.Build()
            .SetLockDuration(lockDuration)
            .SetPassReceiver(receiver));

        // Prevent immediate re-capture by passer
        if (CurrentState is BallStateFreeform && passer != null)
        {
            AddCollisionExceptionWith(passer);
        }
    }

    public void Stop()
    {
        Velocity = Vector2.Zero;
    }

    public bool CanAirInteract() => CurrentState != null && CurrentState.CanAirInteract();

    public bool CanAirConnect(float airConnectMinHeight, float airConnectMaxHeight)
    {
        return Height >= airConnectMinHeight && Height <= airConnectMaxHeight;
    }

    public bool IsHeadedForScoringArea(Area2D scoringArea)
    {
        if (!scoringRaycast.IsColliding()) return false;
        return scoringRaycast.GetCollider() == scoringArea;
    }

    public int GetProximityTeammatesCount(int teamID)
    {
        var players = playerProximityArea.GetOverlappingBodies();
        int count = 0;
        foreach (var body in players)
        {
            if (body is PlayerCharacter p && p.TeamID == teamID)
                count++;
        }
        return count;
    }

    private void SetInteractionsEnabled(bool enabled)
    {
        playerDetectionArea.SetDeferred(Area2D.PropertyName.Monitoring, enabled);
        playerDetectionArea.SetDeferred(Area2D.PropertyName.Monitorable, enabled);

        if (collisionShape != null)
            collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, !enabled);
    }

    private void OnTeamReset()
    {

        Position = spawnPosition;
        Velocity = Vector2.Zero;
        Height = 0f;
        SetInteractionsEnabled(false);
        SwitchState(State.FREEFORM);
    }

    private void OnKickoffStarted()
    {
        GD.Print("I'm kicking off now!");
        SetInteractionsEnabled(true);
        Velocity = Vector2.Zero;
        HeightVelocity = 0;
        PassTo(spawnPosition + Carrier.heading.Normalized() * KICKOFF_PASS_DISTANCE, 0);
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
            gameEvents.TeamResetEventTriggered -= OnTeamReset;
        }
    }
}