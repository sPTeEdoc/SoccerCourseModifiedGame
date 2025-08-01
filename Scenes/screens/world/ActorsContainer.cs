using Godot;
using System;
using System.Collections.Generic;

public partial class ActorsContainer : Node2D
{
    private const int DurationWeightCache = 200;

    private PackedScene playerPrefab = GD.Load<PackedScene>("res://scenes/characters/PlayerCharacter.tscn");
    private PackedScene sparkPrefab = GD.Load<PackedScene>("res://scenes/spark/spark.tscn");

    [Export] public Ball Ball { get; set; }
    [Export] public Goal GoalHome { get; set; }
    [Export] public Goal GoalAway { get; set; }

    private Node2D kickoffs;
    private Node2D spawns;

    private bool isCheckingForKickoffReadiness = false;
    private List<PlayerCharacter> squadHome = new();
    private List<PlayerCharacter> squadAway = new();
    private ulong timeSinceLastCacheRefresh = Time.GetTicksMsec();

    public GameManager gameManager;

    public GameEvents gameEvents;

    public DataLoader dataLoader;

    public override void _Ready()
    {
        kickoffs = GetNode<Node2D>("KickOffs");
        spawns = GetNode<Node2D>("Spawns");

        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        gameManager = GetNode<GameManager>("/root/GameManager");
        dataLoader = GetNode<DataLoader>("/root/DataLoader");

        GoalHome = GetNode<Goal>("PitchObjects/GoalHome");
        GoalAway = GetNode<Goal>("PitchObjects/GoalAway");
        Ball = GetNode<Ball>("Ball");
        gameEvents.TeamResetEventTriggered += OnTeamReset;
        gameEvents.ImpactReceived += OnImpactReceived;

        squadHome = SpawnPlayers(gameManager.currentMatch.CountryHome, GoalHome);
        GoalHome.Initialize(gameManager.currentMatch.CountryHome);
        spawns.Scale = new Vector2(-1, spawns.Scale.Y);
        kickoffs.Scale = new Vector2(-1, kickoffs.Scale.Y);

        squadAway = SpawnPlayers(gameManager.currentMatch.CountryAway, GoalAway);
        GoalAway.Initialize(gameManager.currentMatch.CountryAway);

        SetupControlSchemes();
    }

    public override void _Process(double delta)
    {
        if (Time.GetTicksMsec() - timeSinceLastCacheRefresh > DurationWeightCache)
        {
            timeSinceLastCacheRefresh = Time.GetTicksMsec();
            SetOnDutyWeights();
        }

        if (isCheckingForKickoffReadiness)
            CheckForKickoffReadiness();
    }

    private List<PlayerCharacter> SpawnPlayers(string country, Goal ownGoal)
    {
        var playerNodes = new List<PlayerCharacter>();
        var players = dataLoader.GetSquad(country);
        var targetGoal = ownGoal == GoalAway ? GoalHome : GoalAway;

        for (int i = 0; i < players.Count; i++)
        {
            Vector2 playerPosition = spawns.GetChild<Node2D>(i).GlobalPosition;
            var playerData = players[i] as PlayerResource;
            Vector2 kickoffPosition = i > 3 ? kickoffs.GetChild<Node2D>(i - 4).GlobalPosition : playerPosition;

            var player = SpawnPlayer(playerPosition, kickoffPosition, ownGoal, targetGoal, playerData, country);
            playerNodes.Add(player);
        }

        return playerNodes;
    }

    private PlayerCharacter SpawnPlayer(Vector2 position, Vector2 kickoffPos, Goal ownGoal, Goal targetGoal, PlayerResource data, string country)
    {
        var player = playerPrefab.Instantiate<PlayerCharacter>();
        player.Initialize(position, kickoffPos, Ball, ownGoal, targetGoal, data, country);
        player.SwapRequested += OnPlayerSwapRequest;
        AddChild(player);
        return player;
    }

    private void SetOnDutyWeights()
    {
        foreach (var squad in new[] { squadAway, squadHome })
        {
            var cpuPlayers = squad.FindAll(p => p.controlScheme == PlayerCharacter.ControlScheme.CPU && p.role != PlayerCharacter.Role.GOALIE);
            cpuPlayers.Sort((p1, p2) =>
                p1.spawnPosition.DistanceSquaredTo(Ball.Position).CompareTo(p2.spawnPosition.DistanceSquaredTo(Ball.Position)));

            for (int i = 0; i < cpuPlayers.Count; i++)
                cpuPlayers[i].weightOnDutySteering = 1f - Ease(i / 10f, 0.1f);
        }
    }

    private void OnPlayerSwapRequest(PlayerCharacter requester)
    {
        var squad = requester.country == squadHome[0].country ? squadHome : squadAway;
        var cpuPlayers = squad.FindAll(p => p.controlScheme == PlayerCharacter.ControlScheme.CPU && p.role != PlayerCharacter.Role.GOALIE);
        cpuPlayers.Sort((p1, p2) =>
            p1.Position.DistanceSquaredTo(Ball.Position).CompareTo(p2.Position.DistanceSquaredTo(Ball.Position)));

        var closest = cpuPlayers[0];
        if (closest.Position.DistanceSquaredTo(Ball.Position) < requester.Position.DistanceSquaredTo(Ball.Position))
        {
            var oldScheme = requester.controlScheme;
            requester.SetControlScheme(PlayerCharacter.ControlScheme.CPU);
            closest.SetControlScheme(oldScheme);
        }
    }

    private void CheckForKickoffReadiness()
    {
        foreach (var squad in new[] { squadHome, squadAway })
        {
            foreach (var player in squad)
            {
                if (!player.IsReadyForKickoff())
                    return;
            }
        }

        SetupControlSchemes();
        isCheckingForKickoffReadiness = false;
        gameEvents.EmitSignal("KickoffReady");
    }

    private void SetupControlSchemes()
    {
        ResetControlSchemes();
        string p1Country = gameManager.playerSetup[0];

        if (gameManager.IsCoop())
        {
            var playerSquad = squadHome[0].country == p1Country ? squadHome : squadAway;
            playerSquad[4].SetControlScheme(PlayerCharacter.ControlScheme.P1);
            playerSquad[5].SetControlScheme(PlayerCharacter.ControlScheme.P2);
        }
        else if (gameManager.IsSinglePlayer())
        {
            var playerSquad = squadHome[0].country == p1Country ? squadHome : squadAway;
            playerSquad[5].SetControlScheme(PlayerCharacter.ControlScheme.P1);
        }
        else // versus
        {
            var p1Squad = squadHome[0].country == p1Country ? squadHome : squadAway;
            var p2Squad = p1Squad == squadAway ? squadHome : squadAway;
            p1Squad[5].SetControlScheme(PlayerCharacter.ControlScheme.P1);
            p2Squad[5].SetControlScheme(PlayerCharacter.ControlScheme.P2);
        }
    }

    private void ResetControlSchemes()
    {
        foreach (var squad in new[] { squadHome, squadAway })
        {
            foreach (var player in squad)
                player.SetControlScheme(PlayerCharacter.ControlScheme.CPU);
        }
    }

    private void OnTeamReset()
    {
        isCheckingForKickoffReadiness = true;
    }

    private void OnImpactReceived(Vector2 impactPos, bool isHighImpact)
    {
        var spark = sparkPrefab.Instantiate<Node2D>();
        spark.Position = impactPos;
        AddChild(spark);
    }

    private float Ease(float x, float curve) => x < 0.5f
        ? 0.5f * Mathf.Pow(2 * x, curve)
        : 1 - 0.5f * Mathf.Pow(2 * (1 - x), curve);
}