using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class PlayerCharacter : CharacterBody2D
{
    // Signals
    [Signal]
    public delegate void SwapRequestedEventHandler(PlayerCharacter player);

    public void EmitSwapRequest(PlayerCharacter requestor)
    {
        EmitSignal("SwapRequested", requestor);
    }

    // Constants
    public const float BallControlHeightMax = 10.0f;
    public const float Gravity = 8.0f;
    public const float WalkAnimThreshold = 0.6f;

    private Vector2 passTargetPosition;

    // Enums
    public enum ControlScheme { CPU, P1, P2 }
    public enum Role { GOALIE, DEFENSE, MIDFIELD, OFFENSE }
    public enum SkinColor { LIGHT, MEDIUM, DARK }
    public enum State
    {
        MOVING, TACKLING, RECOVERING, PREPPING_SHOT, SHOOTING, PASSING, HEADER, RECEIVING_PASS,
        VOLLEY_KICK, BICYCLE_KICK, CHEST_CONTROL, HURT, DIVING,
        CELEBRATING, MOURNING, RESETING, ENTRANCE,
        PREENTRANCE
    }

    // Control Sprites Map
    private readonly Dictionary<ControlScheme, Texture2D> controlSchemeMap = new()
    {
        { ControlScheme.CPU, GD.Load<Texture2D>("res://assets/art/props/cpu.png") },
        { ControlScheme.P1, GD.Load<Texture2D>("res://assets/art/props/1p.png") },
        { ControlScheme.P2, GD.Load<Texture2D>("res://assets/art/props/2p.png") }
    };

    // Exported Fields
    [Export] public Ball ball;
    [Export] public ControlScheme controlScheme;
    [Export] public ArenaGoal ownGoal;
    [Export] public float power;
    [Export] public float speed;
    [Export] public ArenaGoal targetGoal;
    [Export] public float pushForce = 50.0f;

    // Node References
    public AnimationPlayer animationPlayer;
    public Area2D ballDetectionArea;
    public Sprite2D controlSprite;
    public CollisionShape2D goalieHandsCollider;
    public Area2D opponentDetectionArea;
    public Area2D permanentDamageEmitterArea;
    public Sprite2D playerSprite;
    public Node2D rootParticles;
    public GpuParticles2D runParticles;
    public Area2D tackleDamageEmitterArea;
    public Area2D teammateDetectionArea;

    // Internal State
    public AIBehaviorFactory aiBehaviorFactory = new();
    public AIBehavior currentAIBehavior = null;
    public PlayerStateFactory stateFactory = new();
    public PlayerState currentState = null;
    public Vector2 heading = Vector2.Up;
    public float height = 0f;
    public float heightVelocity = 0f;
    public Vector2 kickoffPosition = Vector2.Zero;
    public Vector2 preentracePosition = Vector2.Zero;
    public Vector2 entrancePosition = Vector2.Zero;
    public Vector2 spawnPosition = Vector2.Zero;
    public float weightOnDutySteering = 0f;

    public int playerID = 0;
    public int TeamID = -1;
    public string fullname = "";
    public Role role = Role.MIDFIELD;
    public string skinColor = "#9c7250";
    public string hairColor = "#231709";

    public GameEvents gameEvents;
    public GameManager gameManager;
    public DataLoader dataLoader;
    public AnimatedSprite2D animatedSprite2D;
    public Vector2 _bufferedDirection = Vector2.Down;
    private bool _materialDuplicated { get; set; } = false;
    public bool IsReadyToGoToKickoffSpots() => currentState != null && currentState.IsReadyToGoToKickoffSpots();
    public bool IsReadyForKickoff() => currentState != null && currentState.IsReadyForKickoff();
    public bool IsKickingOffPlayer = false;
    public bool InputLocked { get; set; } = false;
    public static float PASS_DISTANCE { get; set; } = 140f; // Increased default open-field pass distance

    public override void _Ready()
    {
        animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        ballDetectionArea = GetNode<Area2D>("BallDetectionArea");
        controlSprite = GetNode<Sprite2D>("AnimatedSprite2D/ControlSprite");
        goalieHandsCollider = GetNode<CollisionShape2D>("GoalieHands/GoalieHandsCollider");
        opponentDetectionArea = GetNode<Area2D>("OpponentDetectionArea");
        permanentDamageEmitterArea = GetNode<Area2D>("PermanentDamageEmitterArea");
        tackleDamageEmitterArea = GetNode<Area2D>("TackleDamageEmitterArea");
        teammateDetectionArea = GetNode<Area2D>("TeammateDetectionArea");
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        gameManager = GetNode<GameManager>("/root/GameManager");
        dataLoader = GetNode<DataLoader>("/root/DataLoader");
        rootParticles = GetNode<Node2D>("RootParticles");
        runParticles = GetNode<GpuParticles2D>("RootParticles/RunParticles");

        SetControlTexture();
        SetupAIBehavior();
        SetShaderProperties();

        gameEvents.TeamScored += OnTeamScored;
        gameEvents.GameOver += OnGameOver;

        tackleDamageEmitterArea.BodyEntered += OnTacklePlayer;
        permanentDamageEmitterArea.BodyEntered += OnTacklePlayer;

        if (goalieHandsCollider != null)
        {
            goalieHandsCollider.Disabled = (role != Role.GOALIE);
        }

        if (!GameManagement.Instance.IsOnPracticeField)
            CallDeferred(nameof(InitializePreEntrance));
    }

    public override void _ExitTree()
    {
        if (gameEvents != null)
        {
            gameEvents.TeamScored -= OnTeamScored;
            gameEvents.GameOver -= OnGameOver;
        }
    }

    private void InitializePreEntrance()
    {
        SwitchState(State.PREENTRANCE,
            PlayerStateData.Build().SetPreEntrancePosition(preentracePosition));
    }

    public override void _Process(double delta)
    {
        SetSpriteVisibility();
        ProcessGravity((float)delta);
        ResolvePlayerPush();
        MoveAndSlide();
    }

    public void ResolvePlayerPush()
    {
        int collisionCount = GetSlideCollisionCount();
        for (int i = 0; i < collisionCount; i++)
        {
            KinematicCollision2D collision = GetSlideCollision(i);
            if (collision.GetCollider() is PlayerCharacter otherPlayer)
            {
                if (!CanBePushed() || !otherPlayer.CanBePushed()) continue;
                Vector2 pushDirection = collision.GetNormal();
                Velocity += pushDirection * pushForce;
            }
        }
    }

    public bool CanBePushed()
    {
        if (currentState == null) return false;
        string stateName = currentState.Name.ToString().ToUpper();
        return !stateName.Contains("HURT")
            && !stateName.Contains("TACKLING")
            && !stateName.Contains("DIVING");
    }

    private void SetShaderProperties()
    {
        if (animatedSprite2D == null)
            animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        if (animatedSprite2D?.Material is ShaderMaterial sharedMat)
        {
            if (!_materialDuplicated)
            {
                sharedMat = (ShaderMaterial)sharedMat.Duplicate();
                animatedSprite2D.Material = sharedMat;
                _materialDuplicated = true;
            }

            sharedMat.SetShaderParameter("is_goalkeeper", role == Role.GOALIE);
            sharedMat.SetShaderParameter("skin_color", Color.FromHtml(skinColor));
            sharedMat.SetShaderParameter("hair_color", Color.FromHtml(hairColor));

            var jersey = dataLoader.GetJerseyColor(TeamID);
            var shorts = dataLoader.GetShortsColor(TeamID);
            var socks = dataLoader.GetSocks(TeamID);
            var keeper_jersey = dataLoader.GetKeeperJerseyColor(TeamID);
            var keeper_shorts = dataLoader.GetKeeperShorts(TeamID);
            var keeper_socks = dataLoader.GetKeeperSocks(TeamID);

            if (jersey != null && !string.IsNullOrEmpty(jersey))
            {
                if (role != Role.GOALIE)
                {
                    sharedMat.SetShaderParameter("jersey_color", Color.FromHtml(jersey));
                    sharedMat.SetShaderParameter("shorts", Color.FromHtml(shorts));
                    sharedMat.SetShaderParameter("socks", Color.FromHtml(socks));
                }
                else
                {
                    sharedMat.SetShaderParameter("keeper_jersey", Color.FromHtml(keeper_jersey));
                    sharedMat.SetShaderParameter("keeper_shorts", Color.FromHtml(keeper_shorts));
                    sharedMat.SetShaderParameter("keeper_socks", Color.FromHtml(keeper_socks));
                }
            }
            else
            {
                sharedMat.SetShaderParameter("team_color", new Color(1, 1, 1));
            }
        }
    }

    public void Initialize(Vector2 contextPosition, Vector2 contextKickoffPosition, Ball contextBall,
        PlayerResource contextPlayerData, int contextTeamID, bool homeTeam)
    {
        Position = contextPosition;
        kickoffPosition = contextKickoffPosition;
        ball = contextBall;
        speed = contextPlayerData.Speed;
        power = contextPlayerData.Power;
        role = contextPlayerData.Role;
        skinColor = contextPlayerData.SkinColor;
        fullname = contextPlayerData.FullName;
        heading = homeTeam ? Vector2.Down : Vector2.Up;
        TeamID = contextTeamID;
        playerID = GameManagement.Instance.PlayerID++;
    }

    public void Initialize(Vector2 contextPosition, Vector2 contextKickoffPosition, Ball contextBall,
        ArenaGoal contextOwnGoal, ArenaGoal contextTargetGoal, PlayerResource contextPlayerData, int contextTeamID,
        Vector2 contextPreentrancePosition, Vector2 contextEntrancePosition)
    {
        Position = contextPreentrancePosition;
        kickoffPosition = contextKickoffPosition;
        spawnPosition = contextPosition;

        ball = contextBall;
        ownGoal = contextOwnGoal;
        targetGoal = contextTargetGoal;
        speed = contextPlayerData.Speed;
        power = contextPlayerData.Power;
        role = contextPlayerData.Role;
        skinColor = contextPlayerData.SkinColor;
        fullname = contextPlayerData.FullName;

        // FIX: Compare Y coordinates for vertical field orientation instead of X
        heading = targetGoal.Position.Y < Position.Y ? Vector2.Up : Vector2.Down;

        TeamID = contextTeamID;
        // playerID = GameManagement.Instance.PlayerID++;
        preentracePosition = contextPreentrancePosition;
        entrancePosition = contextEntrancePosition;
    }

    private void SetupAIBehavior()
    {
        currentAIBehavior = aiBehaviorFactory.GetAIBehavior(role);
        currentAIBehavior.Setup(this, ball, opponentDetectionArea, teammateDetectionArea);
        currentAIBehavior.Name = "AI Behavior";
        AddChild(currentAIBehavior);
    }

    public void SwitchState(State state, PlayerStateData stateData = null)
    {
        stateData ??= new PlayerStateData();

        if (currentState != null)
        {
            var callback = new Callable(this, nameof(SwitchStateWrapped));
            currentState.Disconnect("StateTransitionRequested", callback);
            currentState.QueueFree();
        }

        currentState = stateFactory.GetFreshState(state, this);
        currentState.Setup(this, stateData, ball,
            teammateDetectionArea, ballDetectionArea, ownGoal, targetGoal,
            tackleDamageEmitterArea, currentAIBehavior);

        currentState.Name = $"PlayerStateMachine: {state}";
        currentState.Connect("StateTransitionRequested", new Callable(this, nameof(SwitchStateWrapped)));
        GetNode<Node>("PlayerStateMachine").CallDeferred("add_child", currentState);
    }

    private void SwitchStateWrapped(int nextState, PlayerStateData data)
    {
        SwitchState((PlayerCharacter.State)nextState, data);
    }

    public void SetMovementAnimation()
    {
        float velLength = Velocity.Length();
        Vector2 rawInput = Vector2.Zero;
        Vector2 movementDir = Vector2.Zero;

        if (controlScheme != ControlScheme.CPU)
            rawInput = KeyUtils.GetInputVector(controlScheme);

        if (velLength > 0.1f)
            movementDir = Velocity.Normalized();
        else
            movementDir = _bufferedDirection;

        if (controlScheme != ControlScheme.CPU)
        {
            if (rawInput != Vector2.Zero)
            {
                float angle = Mathf.Round(rawInput.Angle() / (Mathf.Pi / 4f)) * (Mathf.Pi / 4f);
                heading = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                _bufferedDirection = heading;
            }
        }
        else
        {
            if (movementDir != Vector2.Zero)
            {
                float angle = Mathf.Round(movementDir.Angle() / (Mathf.Pi / 4f)) * (Mathf.Pi / 4f);
                heading = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                _bufferedDirection = heading;
            }
        }

        Vector2 visualDirection = movementDir;
        float snappedAngle = Mathf.Round(visualDirection.Angle() * 180f / MathF.PI / 45f) * 45f;

        int angleCheck = (int)snappedAngle;
        if (angleCheck == -180) angleCheck = 180;

        string animPrefix = velLength < 1f ? "idle_" : "run_";
        string directionStr = "south";

        if (angleCheck == 0) directionStr = "east";
        else if (angleCheck == -45) directionStr = "northeast";
        else if (angleCheck == -90) directionStr = "north";
        else if (angleCheck == -135) directionStr = "northwest";
        else if (angleCheck == 180) directionStr = "west";
        else if (angleCheck == 135) directionStr = "southwest";
        else if (angleCheck == 90) directionStr = "south";
        else if (angleCheck == 45) directionStr = "southeast";

        animatedSprite2D.Play(animPrefix + directionStr);
    }

    private void ProcessGravity(float delta)
    {
        if (height > 0f)
        {
            heightVelocity -= Gravity * delta;
            height += heightVelocity;
            if (height <= 0f) height = 0f;
        }
        animatedSprite2D.Position = Vector2.Up * height;
    }

    public void SetHeading()
    {
        if (controlScheme != ControlScheme.CPU)
        {
            Vector2 input = KeyUtils.GetInputVector(controlScheme);
            if (input != Vector2.Zero)
                heading = input;
        }
        else if (Velocity != Vector2.Zero)
        {
            heading = Velocity.Normalized();
        }

        // FIX: Always sync detection areas with heading direction
        if (heading != Vector2.Zero)
        {
            float rotAngle = heading.Angle();
            if (teammateDetectionArea != null) teammateDetectionArea.Rotation = rotAngle;
            if (opponentDetectionArea != null) opponentDetectionArea.Rotation = rotAngle;
        }
    }

    public void FaceTowardsTargetGoal()
    {
        if (!IsFacingTargetGoal())
            heading *= -1;
    }

    public void FaceDirectionOfBall()
    {
        if (!IsFacingBall(ball, 45))
            heading *= -1;
    }

    public virtual void FlipSprites()
    {
        // FIX: Flip sprite horizontally when moving left (West)
        bool facingLeft = heading.X < -0.1f;
        animatedSprite2D.FlipH = facingLeft;

        float scale = facingLeft ? -1f : 1f;
        rootParticles.Scale = new Vector2(scale, rootParticles.Scale.Y);
    }

    public void SetControlScheme(ControlScheme scheme)
    {
        controlScheme = scheme;
        SetControlTexture();
    }

    private void SetSpriteVisibility()
    {
        controlSprite.Visible = HasBall() || controlScheme != ControlScheme.CPU;
        runParticles.Emitting = Velocity.Length() == speed;
    }

    public void GetHurt(Vector2 hurtOrigin)
    {
        SwitchState(State.HURT, PlayerStateData.Build().SetHurtDirection(hurtOrigin));
    }

    public bool HasBall() => ball.Carrier == this;

    private void SetControlTexture()
    {
        controlSprite.Texture = controlSchemeMap[controlScheme];
    }

    public void GetPassRequest(PlayerCharacter player)
    {
        if (HasBall() && currentState != null && currentState.CanPass())
        {
            SwitchState(State.PASSING, PlayerStateData.Build().SetPassTarget(player));
        }
    }

    public bool IsFacingTargetGoal()
    {
        Vector2 directionToGoal = Position.DirectionTo(targetGoal.Position);
        return heading.Dot(directionToGoal) > 0;
    }

    public bool IsFacingBall(Ball ball, float angleThresholdDegrees = 45f)
    {
        Vector2 toBall = (ball.Position - Position).Normalized();
        float dot = heading.Dot(toBall);
        float angle = Mathf.RadToDeg(Mathf.Acos(dot));
        return angle <= angleThresholdDegrees;
    }

    public bool CanCarryBall() => currentState != null && currentState.CanCarryBall();

    public void OnAnimationComplete()
    {
        currentState?.OnAnimationComplete();
    }

    private void OnTeamScored(int teamScoredOn)
    {
        gameManager.currentMatch.TeamKickingOff = teamScoredOn;
        gameManager.GoalJustScored = true;
        if (TeamID == teamScoredOn)
            SwitchState(State.MOURNING);
        else
            SwitchState(State.CELEBRATING);
    }

    private void OnGameOver(int winningTeam)
    {
        if (TeamID == winningTeam)
            SwitchState(State.CELEBRATING);
        else
            SwitchState(State.MOURNING);
    }

    public void ControlBall()
    {
        if (ball.Height > BallControlHeightMax)
            SwitchState(State.CHEST_CONTROL);
    }

    public Vector2 GetPassTarget() => passTargetPosition;

    protected virtual void SubclassReady() { }

    public virtual void OnTacklePlayer(Node other)
    {
        if (ball.Carrier is not null)
        {
            if (other is PlayerCharacter player &&
                player != this &&
                player.TeamID != TeamID &&
                ball.Carrier.playerID == player.playerID)
            {
                Vector2 direction = Position.DirectionTo(player.Position);
                player.GetHurt(direction);
            }
        }
    }

    public void SetBufferedDirection(Vector2 direction)
    {
        _bufferedDirection = direction.Normalized();
    }

    /// <summary>
    /// Swaps goals and mirrors all tactical anchor positions across the center of the pitch.
    /// </summary>
    public void SwapSides(Vector2 pitchCenter)
    {
        // 1. Swap Goal References
        (ownGoal, targetGoal) = (targetGoal, ownGoal);

        // 2. Mirror tactical points 180 degrees around pitch center
        spawnPosition = 2 * pitchCenter - spawnPosition;
        kickoffPosition = 2 * pitchCenter - kickoffPosition;
        preentracePosition = 2 * pitchCenter - preentracePosition;
        entrancePosition = 2 * pitchCenter - entrancePosition;

        // 3. Update heading and facing direction towards new target goal
        heading = targetGoal.Position.Y < Position.Y ? Vector2.Up : Vector2.Down;
    }

    /// <summary>
    /// Sends player off-field to preentrance position for halftime.
    /// </summary>
    public void MoveToPreEntrance()
    {
        SwitchState(State.PREENTRANCE,
            PlayerStateData.Build().SetPreEntrancePosition(preentracePosition));
    }

    /// <summary>
    /// Sends player from off-field back to kickoff/reset spot for new half.
    /// </summary>
    public void MoveToResetPosition()
    {
        SwitchState(State.RESETING,
            PlayerStateData.Build().SetResetPosition(kickoffPosition));
    }
}