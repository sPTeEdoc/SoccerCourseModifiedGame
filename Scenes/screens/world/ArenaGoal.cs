using Godot;
using System;

public partial class ArenaGoal : Node2D
{
    // --- Editor Configuration ---
    [Export] public bool IsNorth { get; set; } = true; // True for North goal, False for South goal
    [Export] public float CrossbarHeight { get; set; } = 32f;

    // --- AI Targeting Markers ---
    // Place two Marker2D nodes in your scene at the posts and link them here
    [Export] public Marker2D LeftPost { get; set; }
    [Export] public Marker2D RightPost { get; set; }
    [Export] public float PostBuffer { get; set; } = 12.0f;
    [Export] private Area2D backNetAreaUpper;
    [Export] private Area2D backNetAreaLeft;
    [Export] private Area2D backNetAreaRight;
    [Export] private Area2D scoringArea;
    [Export] public TileMapLayer layer;
    [Export] private Area2D ballAtBackOfNet;
    [Export] private Area2D ballAwayFromBackOfNet;
    [Export] private Area2D crossBarPlus;
    public int teamID { get; set; } = -1;

    private GameEvents gameEvents;
    private SoundPlayer soundPlayer;
    [Export] private Node targets;

    public override void _Ready()
    {
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");
        targets = GetNode<Node>("Targets");

        backNetAreaUpper.BodyEntered += OnBallEnterBackNet;
        backNetAreaUpper.BodyExited += OnBallExitBackNet; // Add exit monitoring

        backNetAreaRight.BodyEntered += OnBallEnterBackNet;
        backNetAreaRight.BodyExited += OnBallExitBackNet; // Add exit monitoring

        backNetAreaLeft.BodyEntered += OnBallEnterBackNet;
        backNetAreaLeft.BodyExited += OnBallExitBackNet; // Add exit monitoring
        scoringArea.BodyEntered += OnBallEnterScoringArea;

        ballAtBackOfNet.BodyEntered += OnBallAtBackOfNet;
        ballAwayFromBackOfNet.BodyEntered += OnBallAwayFromBackOfNet;

        crossBarPlus.BodyEntered += OnBallAtCrossBar;
    }

    public void Initialize(int contextTeam)
    {
        teamID = contextTeam;
    }

    private void OnBallAtCrossBar(Node body)
    {
        if (body is Ball ball)
        {
            if (ball.Height <= ball.CrossbarHeight)
            {
                ball.CollisionMask = 33;
            }
            else
            {
                ball.CollisionMask = 161;
            }
        }
    }

    private void OnBallAwayFromBackOfNet(Node body)
    {
        if (body is Ball ball)
        {
            if (ball.Height <= ball.CrossbarHeight)
            {
                layer.ZIndex = 0;
            }
        }
    }

    private void OnBallAtBackOfNet(Node body)
    {
        if (body is Ball ball)
        {
            if (ball.Height <= ball.CrossbarHeight)
            {
                layer.ZIndex = 2;
            }
        }
    }

    private void OnBallEnterBackNet(Node body)
    {
        if (body is Ball ball)
        {
            if (ball.Height <= ball.CrossbarHeight)
            {
                ball.IsInNet = true; // Let the ball's custom dampening take over
            }
        }
    }

    private void OnBallExitBackNet(Node body)
    {
        if (body is Ball ball)
        {
            ball.IsInNet = false;
        }
    }

    public bool goalCounted = false;

    public Vector2 GetCenterTargetPosition()
    {
        if (LeftPost == null || RightPost == null) return GlobalPosition;

        // Midpoint between the two posts
        return (LeftPost.GlobalPosition + RightPost.GlobalPosition) / 2f;
    }

    public Vector2 GetRandomTargetPosition()
    {
        if (LeftPost == null || RightPost == null) return GlobalPosition;

        // Calculate line vector between posts
        Vector2 postToPost = RightPost.GlobalPosition - LeftPost.GlobalPosition;
        float distance = postToPost.Length();
        Vector2 direction = postToPost.Normalized();

        // Get a random offset between the posts, respecting our buffer zone
        double distancePostBuffer = (double)(distance - PostBuffer);
        float randomOffset = (float)GD.RandRange((double)PostBuffer, distancePostBuffer);
        Vector2 targetOnLine = LeftPost.GlobalPosition + (direction * randomOffset);

        // Nudge the target slightly deep into the net (e.g., 8 pixels)
        // so the ball clearly crosses the goal line on targeted shots
        float nudgeAmount = 8.0f;
        Vector2 netNudge = IsNorth ? new Vector2(0, -nudgeAmount) : new Vector2(0, nudgeAmount);

        return targetOnLine + netNudge;
    }

    private void OnBallEnterScoringArea(Node body)
    {
        if (body is Ball ball)
        {
            if (ball.Height <= ball.CrossbarHeight)
            {
                if (!goalCounted)
                {
                    goalCounted = true;
                    soundPlayer.Play(SoundPlayer.Sound.WHISTLE);
                    GD.Print($"GOAL! The ball went under the bar in the {(IsNorth ? "North" : "South")} goal.");
                    gameEvents.EmitSignal("TeamScored", teamID);
                }
            }
            else
            {
                GD.Print("MISS! The shot sailed over the crossbar.");
            }
        }
    }

    public Vector2 GetTopTargetPosition()
    {
        return targets.GetChild<Node2D>(0).GlobalPosition;
    }

    public Vector2 GetBottomTargetPosition()
    {
        int index = targets.GetChildCount() - 1;
        return targets.GetChild<Node2D>(index).GlobalPosition;
    }

    public Area2D GetScoringArea()
    {
        return scoringArea;
    }
}
