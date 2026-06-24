using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;

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
        CELEBRATING, MOURNING, RESETING, STANDING_TACKLE
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
    [Export] public Goal ownGoal;
    [Export] public float power;
    [Export] public float speed;
    [Export] public Goal targetGoal;

    // Node References
    public AnimationPlayer animationPlayer;
    public Area2D ballDetectionArea;
    public Sprite2D controlSprite;
    public Area2D opponentDetectionArea;
    public Sprite2D playerSprite;
    public Node2D rootParticles;
    public GpuParticles2D runParticles;
    public Area2D teammateDetectionArea;

    // Internal State
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

    public int playerID = 0;
    public int teamID = -1;
    public string fullname = "";
    public Role role = Role.MIDFIELD;
    public string skinColor = "#9c7250";

    public GameEvents gameEvents;
    public GameManager gameManager;
    public DataLoader dataLoader;

    public double passerRating = 0;
    public virtual string AnimPrefix => "";
    protected virtual string DefaultSpriteSheet => "res://assets/art/characters/nibley3.png";
    protected virtual string AlternateSpriteSheet => "res://assets/art/characters/nibley4.png";
    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        ballDetectionArea = GetNode<Area2D>("BallDetectionArea");
        controlSprite = GetNode<Sprite2D>("PlayerSprite/ControlSprite");
        opponentDetectionArea = GetNode<Area2D>("OpponentDetectionArea");
        playerSprite = GetNode<Sprite2D>("PlayerSprite");
        rootParticles = GetNode<Node2D>("RootParticles");
        runParticles = GetNode<GpuParticles2D>("RootParticles/RunParticles");
        teammateDetectionArea = GetNode<Area2D>("TeammateDetectionArea");
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        gameManager = GetNode<GameManager>("/root/GameManager");
        dataLoader = GetNode<DataLoader>("/root/DataLoader");

        SetControlTexture();
        SetupAIBehavior();
        SetShaderProperties();

        spawnPosition = Position;

        gameEvents.TeamScored += OnTeamScored;
        gameEvents.GameOver += OnGameOver;

        var initialPosition = teamID == gameManager.currentMatch.TeamHome ? kickoffPosition : spawnPosition;
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
        var initialPosition = teamID == gameManager.currentMatch.TeamHome ? kickoffPosition : spawnPosition;
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
        if (shaderMat == null) return;

        // 1. Set Skin Tone
        shaderMat.SetShaderParameter("skin_color", Color.FromHtml(skinColor));

        // 2. Set Team Color
        var jerseyA = dataLoader.GetJerseyColorA(teamID); // 'team' is likely a string like "FRANCE"
        var jerseyB = dataLoader.GetJerseyColorB(teamID); // 'team' is likely a string like "FRANCE"
        var jerseyC = dataLoader.GetJerseyColorC(teamID);
        var shorts = dataLoader.GetShortsColor(teamID); // 'team' is likely a string like "FRANCE"
        var socksColor = dataLoader.GetSocksColor(teamID); // 'team' is likely a string like "FRANCE"

        if (!String.IsNullOrWhiteSpace(jerseyC))
        {
            playerSprite.Texture = GD.Load<Texture2D>(AlternateSpriteSheet);
        }
        else
        {
            playerSprite.Texture = GD.Load<Texture2D>(DefaultSpriteSheet);
        }

        if (jerseyA != null && !string.IsNullOrEmpty(jerseyA))
        {
            shaderMat.SetShaderParameter("team_color", Color.FromHtml(jerseyA));
            shaderMat.SetShaderParameter("team_colorB", Color.FromHtml(jerseyB));
            shaderMat.SetShaderParameter("team_colorC", Color.FromHtml(jerseyC));
            shaderMat.SetShaderParameter("shorts", Color.FromHtml(shorts));
            shaderMat.SetShaderParameter("socks", Color.FromHtml(socksColor));
        }
        else
        {
            // Fallback color (e.g., White) if team isn't found
            shaderMat.SetShaderParameter("team_color", new Color(1, 1, 1));
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
        heading = homeTeam ? Vector2.Left : Vector2.Right;
        teamID = contextTeamID;
        playerID = GameManagement.PlayerID++;
    }

    public void Initialize(Vector2 contextPosition, Vector2 contextKickoffPosition, Ball contextBall,
        Goal contextOwnGoal, Goal contextTargetGoal, PlayerResource contextPlayerData, int contextTeamID)
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
        teamID = contextTeamID;
        playerID = GameManagement.PlayerID++;
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

        // Disconnect before freeing the old state
        if (currentState != null)
        {
            var callback = new Callable(this, nameof(SwitchStateWrapped));
            currentState.Disconnect("StateTransitionRequested", callback);
            currentState.QueueFree();
        }

        // Create and set up the new state
        currentState = stateFactory.GetFreshState(state, this);
        currentState.Setup(this, stateData);

        currentState.Name = $"PlayerStateMachine: {state}";

        // Reconnect the signal with the fresh state
        currentState.Connect("StateTransitionRequested", new Callable(this, nameof(SwitchStateWrapped)));

        // Add the new state to the tree after setup
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
            animationPlayer.Play($"{AnimPrefix}idle");
        else if (velLength < speed * WalkAnimThreshold)
            animationPlayer.Play($"{AnimPrefix}walk");
        else
            animationPlayer.Play($"{AnimPrefix}run");
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

    public void FaceDirectionOfBall()
    {
        if (!IsFacingBall(ball, 45))
            heading *= -1;
    }

    public virtual void FlipSprites() // Added 'virtual'
    {
        bool facingRight = heading == Vector2.Right;

        playerSprite.FlipH = !facingRight;
        float scale = facingRight ? 1 : -1;

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
        if (teamID == teamScoredOn)
            SwitchState(State.MOURNING);
        else
            SwitchState(State.CELEBRATING);
    }

    private void OnGameOver(int winningTeam)
    {
        if (teamID == winningTeam)
            SwitchState(State.CELEBRATING);
        else
            SwitchState(State.MOURNING);
    }

    public void ControlBall()
    {
        if (ball.Height > BallControlHeightMax)
            SwitchState(State.CHEST_CONTROL);
    }

    public void ReceiveIncomingPass(Vector2 targetPosition, double passerRating)
    {
        passTargetPosition = targetPosition;
        this.passerRating = passerRating;
        SwitchState(State.RECEIVING_PASS);
    }

    public Vector2 GetPassTarget()
    {
        return passTargetPosition;
    }

    // Virtual method for subclasses to override
    protected virtual void SubclassReady() { }

    public virtual void OnTacklePlayer(Node other)
    {
        if (other is PlayerCharacter player &&
            player != this &&
            player.teamID != teamID &&
            ball.Carrier == player)
        {
            Vector2 direction = Position.DirectionTo(player.Position);
            player.GetHurt(direction);
        }
    }
}
