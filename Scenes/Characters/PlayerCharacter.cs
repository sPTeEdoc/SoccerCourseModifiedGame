using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class PlayerCharacter : CharacterBody2D
{
    // 🚩 Signals
    [Signal]
    public delegate void SwapRequestedEventHandler(PlayerCharacter player);

    public void EmitSwapRequest(PlayerCharacter requestor)
    {
        EmitSignal("SwapRequested", requestor);
    }

    // 📌 Constants
    public const float BallControlHeightMax = 10.0f;
    public const float Gravity = 8.0f;
    public const float WalkAnimThreshold = 0.6f;

    // 🎮 Enums
    public enum ControlScheme { CPU, P1, P2 }
    public enum Role { GOALIE, DEFENSE, MIDFIELD, OFFENSE }
    public enum SkinColor { LIGHT, MEDIUM, DARK }
    public enum State
    {
        MOVING, TACKLING, RECOVERING, PREPPING_SHOT, SHOOTING, PASSING, HEADER,
        VOLLEY_KICK, BICYCLE_KICK, CHEST_CONTROL, HURT, DIVING,
        CELEBRATING, MOURNING, RESETING
    }

    // 🧩 Control Sprites Map
    private readonly Dictionary<ControlScheme, Texture2D> controlSchemeMap = new()
    {
        { ControlScheme.CPU, GD.Load<Texture2D>("res://assets/art/props/cpu.png") },
        { ControlScheme.P1, GD.Load<Texture2D>("res://assets/art/props/1p.png") },
        { ControlScheme.P2, GD.Load<Texture2D>("res://assets/art/props/2p.png") }
    };

    // 🎯 Exported Fields
    [Export] public Ball ball;
    [Export] public ControlScheme controlScheme;
    [Export] public Goal ownGoal;
    [Export] public float power;
    [Export] public float speed;
    [Export] public Goal targetGoal;

    // 🧠 Node References
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

    // 🧠 Internal State
    public AIBehaviorFactory aiBehaviorFactory = new();
    public AIBehavior currentAIBehavior = null;
    public PlayerStateFactory stateFactory = new();
    public PlayerState currentState = null;

    public Vector2 heading = Vector2.Right;
    public float height = 0f;
    public float heightVelocity = 0f;
    public Vector2 kickoffPosition = Vector2.Zero;
    public Vector2 spawnPosition = Vector2.Zero;
    public float weightOnDutySteering = 0f;

    public string country = "";
    public string fullname = "";
    public Role role = Role.MIDFIELD;
    public SkinColor skinColor = SkinColor.MEDIUM;

    public GameEvents gameEvents;
    public GameManager gameManager;
    public DataLoader dataLoader;
    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        ballDetectionArea = GetNode<Area2D>("BallDetectionArea");
        controlSprite = GetNode<Sprite2D>("PlayerSprite/ControlSprite");
        goalieHandsCollider = GetNode<CollisionShape2D>("GoalieHands/GoalieHandsCollider");
        opponentDetectionArea = GetNode<Area2D>("OpponentDetectionArea");
        permanentDamageEmitterArea = GetNode<Area2D>("PermanentDamageEmitterArea");
        playerSprite = GetNode<Sprite2D>("PlayerSprite");
        rootParticles = GetNode<Node2D>("RootParticles");
        runParticles = GetNode<GpuParticles2D>("RootParticles/RunParticles");
        tackleDamageEmitterArea = GetNode<Area2D>("TackleDamageEmitterArea");
        teammateDetectionArea = GetNode<Area2D>("TeammateDetectionArea");
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        gameManager = GetNode<GameManager>("/root/GameManager");
        dataLoader = GetNode<DataLoader>("/root/DataLoader");

        SetControlTexture();
        SetupAIBehavior();
        SetShaderProperties();

        permanentDamageEmitterArea.Monitoring = role == Role.GOALIE;
        goalieHandsCollider.Disabled = role != Role.GOALIE;

        tackleDamageEmitterArea.BodyEntered += OnTacklePlayer;
        permanentDamageEmitterArea.BodyEntered += OnTacklePlayer;

        spawnPosition = Position;

        gameEvents.TeamScored += OnTeamScored;
        gameEvents.GameOver += OnGameOver;

        var initialPosition = country == gameManager.currentMatch.CountryHome ? kickoffPosition : spawnPosition;
        CallDeferred(nameof(InitializeResetState));
    }

    public override void _ExitTree()
    {
        if (gameEvents != null)
        {
            gameEvents.TeamScored -= OnTeamScored;
            gameEvents.GameOver -= OnGameOver;
        }
    }

    private void InitializeResetState()
    {
        var initialPosition = country == gameManager.currentMatch.CountryHome ? kickoffPosition : spawnPosition;
        SwitchState(State.RESETING, PlayerStateData.Build().SetResetPosition(initialPosition));
    }

    public override void _Process(double delta)
    {
        FlipSprites();
        SetSpriteVisibility();
        ProcessGravity((float)delta);
        MoveAndSlide();
    }

    private void SetShaderProperties()
    {
        var shaderMat = playerSprite.Material as ShaderMaterial;
        if (shaderMat != null)
        {
            shaderMat.SetShaderParameter("skin_color", (int)skinColor);
        }

        var countries = dataLoader.GetCountries();
        int countryColor = countries.FindIndex(c => c == country);
        countryColor = Mathf.Clamp(countryColor, 0, countries.Count - 1);

        shaderMat?.SetShaderParameter("team_color", countryColor);
    }

    public void Initialize(Vector2 contextPosition, Vector2 contextKickoffPosition, Ball contextBall,
        Goal contextOwnGoal, Goal contextTargetGoal, PlayerResource contextPlayerData, string contextCountry)
    {
        Position = contextPosition;
        kickoffPosition = contextKickoffPosition;
        ball = contextBall;
        ownGoal = contextOwnGoal;
        targetGoal = contextTargetGoal;
        speed = contextPlayerData.Speed;
        power = contextPlayerData.Power;
        role = contextPlayerData.Role;
        skinColor = contextPlayerData.SkinColor;
        fullname = contextPlayerData.FullName;
        heading = targetGoal.Position.X < Position.X ? Vector2.Left : Vector2.Right;
        country = contextCountry;
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

        // 🔌 Disconnect before freeing the old state
        if (currentState != null)
        {
            currentState.Cleanup(); // 🌟 Abstracted cleanup

            var callback = new Callable(this, nameof(SwitchStateWrapped));
            currentState.Disconnect("StateTransitionRequested", callback); // ❌ don't check IsConnected

            currentState.QueueFree();
        }

        // 🧱 Create and set up the new state
        currentState = stateFactory.GetFreshState(state, this);
        currentState.Setup(this, stateData, animationPlayer, ball,
            teammateDetectionArea, ballDetectionArea, ownGoal, targetGoal,
            tackleDamageEmitterArea, currentAIBehavior);

        currentState.Name = $"PlayerStateMachine: {state}";

        // 🔁 Reconnect the signal with the fresh state
        currentState.Connect("StateTransitionRequested", new Callable(this, nameof(SwitchStateWrapped)));

        // 🐣 Add the new state to the tree after setup
        GetNode<Node>("PlayerStateMachine").CallDeferred("add_child", currentState);
    }


    private void SwitchStateWrapped(int nextState, PlayerStateData data)
    {
        SwitchState((PlayerCharacter.State)nextState, data);
    }

    public void SetMovementAnimation()
    {
        float velLength = Velocity.Length();
        if (velLength < 1)
            animationPlayer.Play("idle");
        else if (velLength < speed * WalkAnimThreshold)
            animationPlayer.Play("walk");
        else
            animationPlayer.Play("run");
    }

    private void ProcessGravity(float delta)
    {
        if (height > 0f)
        {
            heightVelocity -= Gravity * delta;
            height += heightVelocity;
            if (height <= 0f)
                height = 0f;
        }
        playerSprite.Position = Vector2.Up * height;
    }

    public void SetHeading()
    {
        if (Velocity.X > 0) heading = Vector2.Right;
        else if (Velocity.X < 0) heading = Vector2.Left;
    }

    public void FaceTowardsTargetGoal()
    {
        if (!IsFacingTargetGoal())
            heading *= -1;
    }

    private void FlipSprites()
    {
        bool facingRight = heading == Vector2.Right;

        playerSprite.FlipH = !facingRight;
        float scale = facingRight ? 1 : -1;

        tackleDamageEmitterArea.Scale = new Vector2(scale, tackleDamageEmitterArea.Scale.Y);
        opponentDetectionArea.Scale = new Vector2(scale, opponentDetectionArea.Scale.Y);
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
    public bool IsReadyForKickoff() => currentState != null && currentState.IsReadyForKickoff();

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

    public bool CanCarryBall() => currentState != null && currentState.CanCarryBall();

        private void OnTacklePlayer(Node other)
    {
        if (other is PlayerCharacter player &&
            player != this &&
            player.country != country &&
            ball.Carrier == player)
        {
            Vector2 direction = Position.DirectionTo(player.Position);
            player.GetHurt(direction);
        }
    }

    public void OnAnimationComplete()
    {
        currentState?.OnAnimationComplete();
    }

    private void OnTeamScored(string teamScoredOn)
    {
        if (country == teamScoredOn)
            SwitchState(State.MOURNING);
        else
            SwitchState(State.CELEBRATING);
    }

    private void OnGameOver(string winningTeam)
    {
        if (country == winningTeam)
            SwitchState(State.CELEBRATING);
        else
            SwitchState(State.MOURNING);
    }

    public void ControlBall()
    {
        if (ball.Height > BallControlHeightMax)
            SwitchState(State.CHEST_CONTROL);
    }
}
