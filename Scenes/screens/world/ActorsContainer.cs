using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ActorsContainer : Node2D
{
    private const int DurationWeightCache = 200;

    private readonly PackedScene outfielderPrefab = GD.Load<PackedScene>("res://Scenes/Characters/Outfielder.tscn");
    private readonly PackedScene goalkeeperPrefab = GD.Load<PackedScene>("res://Scenes/Characters/Goalkeeper.tscn"); private PackedScene sparkPrefab = GD.Load<PackedScene>("res://scenes/spark/spark.tscn");

    [Export] public Ball Ball { get; set; }
    [Export] public Goal GoalHome { get; set; }
    [Export] public Goal GoalAway { get; set; }

    private Node2D kickoffs;
    private Node2D spawns;

    private bool isCheckingForKickoffReadiness = false;
    private List<PlayerCharacter> squadHome = new();
    private List<PlayerCharacter> squadAway = new();
    private ulong timeSinceLastCacheRefresh = Time.GetTicksMsec();

    private ulong lastAutoSwitchTime = 0;
    private const int AUTO_SWITCH_INTERVAL = 300;  // ms between switch attempts
    private const float MIN_SWITCH_DELTA = 40f;    // how much closer candidate must be

    public GameManager gameManager;

    public GameEvents gameEvents;

    public DataLoader dataLoader;

    public override void _ExitTree()
    {
        if (gameEvents != null)
        {
            gameEvents.TeamResetEventTriggered -= OnTeamReset;
            gameEvents.ImpactReceived -= OnImpactReceived;
        }
    }


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

        squadHome = SpawnPlayers(gameManager.currentMatch.TeamHome, GoalHome);
        GoalHome.Initialize(gameManager.currentMatch.TeamHome);
        spawns.Scale = new Vector2(-1, spawns.Scale.Y);
        kickoffs.Scale = new Vector2(-1, kickoffs.Scale.Y);

        squadAway = SpawnPlayers(gameManager.currentMatch.TeamAway, GoalAway);
        GoalAway.Initialize(gameManager.currentMatch.TeamAway);

        SetupControlSchemes();
    }

    public override void _Process(double delta)
    {
        TryAutoSwitchDefender();

        if (Time.GetTicksMsec() - timeSinceLastCacheRefresh > DurationWeightCache)
        {
            timeSinceLastCacheRefresh = Time.GetTicksMsec();
            SetOnDutyWeights();
        }

        if (isCheckingForKickoffReadiness)
            CheckForKickoffReadiness();
    }

    private List<PlayerCharacter> SpawnPlayers(int teamID, Goal ownGoal)
    {
        var playerNodes = new List<PlayerCharacter>();
        var players = dataLoader.GetSquad(teamID);
        var targetGoal = ownGoal == GoalAway ? GoalHome : GoalAway;

        for (int i = 0; i < players.Count; i++)
        {
            Vector2 playerPosition = spawns.GetChild<Node2D>(i).GlobalPosition;
            var playerData = players[i] as PlayerResource;
            Vector2 kickoffPosition = i > 3 ? kickoffs.GetChild<Node2D>(i - 4).GlobalPosition : playerPosition;

            var player = SpawnPlayer(playerPosition, kickoffPosition, ownGoal, targetGoal, playerData, teamID);
            playerNodes.Add(player);
        }

        return playerNodes;
    }

    private PlayerCharacter SpawnPlayer(Vector2 position, Vector2 kickoffPos, Goal ownGoal, Goal targetGoal, PlayerResource data, int teamID)
    {
        PlayerCharacter player;

        // 1. Determine which prefab to use based on the player's role
        if (data.Role == PlayerCharacter.Role.GOALIE)
        {
            player = goalkeeperPrefab.Instantiate<GoalKeeper>();
        }
        else
        {
            player = outfielderPrefab.Instantiate<Outfielder>();
        }

        // 2. Initialize the common fields exactly like before!
        // Because both types are PlayerCharacters, this method works seamlessly on either.
        player.Initialize(position, kickoffPos, Ball, ownGoal, targetGoal, data, teamID);

        // 3. Add to the scene tree and return
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
        if (Ball.Carrier != null && (Ball.Carrier.controlScheme == PlayerCharacter.ControlScheme.P1 ||
            Ball.Carrier.controlScheme == PlayerCharacter.ControlScheme.P2))
        {
            // Already controlled by human — no swap needed
            return;
        }

        if (Ball.Carrier != null &&
            Ball.Carrier.teamID == gameManager.playerSetup[0] &&
            Ball.Carrier.controlScheme != PlayerCharacter.ControlScheme.P1 &&
            Ball.Carrier.controlScheme != PlayerCharacter.ControlScheme.P2)
        {
            var squad1 = Ball.Carrier.teamID == squadHome[0].teamID ? squadHome : squadAway;
            var squad2 = Ball.Carrier.teamID == squadHome[0].teamID ? squadHome : squadAway;
            var currentHuman1 = squad1.Find(p => p.controlScheme == PlayerCharacter.ControlScheme.P1);
            var currentHuman2 = squad1.Find(p => p.controlScheme == PlayerCharacter.ControlScheme.P2);

            if (Ball.Carrier == currentHuman1 || Ball.Carrier == currentHuman2)
                return;

            currentHuman1?.SetControlScheme(PlayerCharacter.ControlScheme.CPU);
            if (currentHuman1 != null) Ball.Carrier.SetControlScheme(PlayerCharacter.ControlScheme.P1);
            if (currentHuman2 != null) Ball.Carrier.SetControlScheme(PlayerCharacter.ControlScheme.P2);
            return;
        }


        var squad = requester.teamID == squadHome[0].teamID ? squadHome : squadAway;
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
        int p1Team = gameManager.playerSetup[0];

        if (gameManager.IsCoop())
        {
            var playerSquad = squadHome[0].teamID == p1Team ? squadHome : squadAway;
            playerSquad[4].SetControlScheme(PlayerCharacter.ControlScheme.P1);
            playerSquad[5].SetControlScheme(PlayerCharacter.ControlScheme.P2);
        }
        else if (gameManager.IsSinglePlayer())
        {
            var playerSquad = squadHome[0].teamID == p1Team ? squadHome : squadAway;
            playerSquad[5].SetControlScheme(PlayerCharacter.ControlScheme.P1);
        }
        else // versus
        {
            var p1Squad = squadHome[0].teamID == p1Team ? squadHome : squadAway;
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

    private void TryAutoSwitchDefender()
    {
        if (Time.GetTicksMsec() - lastAutoSwitchTime < AUTO_SWITCH_INTERVAL)
            return;

        if (Ball?.Carrier == null || Ball.Carrier.teamID == -1)
            return;

        int playerTeam = gameManager.playerSetup[0];
        if (Ball.Carrier.teamID == playerTeam)
            return; // Human team is attacking

        var squad = squadHome[0].teamID == playerTeam ? squadHome : squadAway;

        // Handle both P1 and P2
        var humans = squad.FindAll(p =>
            p.controlScheme == PlayerCharacter.ControlScheme.P1 ||
            p.controlScheme == PlayerCharacter.ControlScheme.P2);

        var cpuDefenders = squad.FindAll(p =>
            p.controlScheme == PlayerCharacter.ControlScheme.CPU &&
            p.role != PlayerCharacter.Role.GOALIE);

        if (cpuDefenders.Count == 0 || humans.Count == 0)
            return;

        // Sort CPU defenders by proximity to ball
        cpuDefenders.Sort((a, b) =>
            a.Position.DistanceSquaredTo(Ball.Position)
            .CompareTo(b.Position.DistanceSquaredTo(Ball.Position)));

        foreach (var human in humans)
        {
            var bestCandidate = cpuDefenders.FirstOrDefault();
            if (bestCandidate == null)
                continue;

            float currentDist = human.Position.DistanceTo(Ball.Position);
            float candidateDist = bestCandidate.Position.DistanceTo(Ball.Position);

            if (candidateDist + MIN_SWITCH_DELTA < currentDist)
            {
                var oldScheme = human.controlScheme;
                human.SetControlScheme(PlayerCharacter.ControlScheme.CPU);
                bestCandidate.SetControlScheme(oldScheme);
                lastAutoSwitchTime = Time.GetTicksMsec();
                // GD.Print($"[AutoSwitch] {oldScheme} now controls {bestCandidate.Name}");
            }
        }
    }
}