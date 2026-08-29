using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class FullFieldActorsContainer : Node2D
{
    private const int DurationWeightCache = 200;

    private PackedScene playerPrefab = GD.Load<PackedScene>("res://scenes/characters/PlayerCharacter.tscn");
    private PackedScene sparkPrefab = GD.Load<PackedScene>("res://scenes/spark/spark.tscn");

    [Export] public Ball Ball { get; set; }
    [Export] public ArenaGoal NorthGoal { get; set; }
    [Export] public ArenaGoal SouthGoal { get; set; }

    private Node2D kickoffs;
    private Node2D spawns;
    private Node2D preentrance;
    private Node2D entrance;

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

        Ball = GetNode<Ball>("Ball");
        gameEvents.TeamResetEventTriggered += OnTeamReset;
        gameEvents.ImpactReceived += OnImpactReceived;

        gameManager.currentMatch = new Match(GameManagement.Instance.TeamsDictionary[2].TeamID,
            GameManagement.Instance.TeamsDictionary[3].TeamID);

        preentrance = GetNode<Node2D>("Preentrance");
        entrance = GetNode<Node2D>("Entrance");

        squadHome = SpawnPlayers(gameManager.currentMatch.HomeTeam, SouthGoal);
        NorthGoal.Initialize(gameManager.currentMatch.HomeTeam);

        squadAway = SpawnPlayers(gameManager.currentMatch.AwayTeam, NorthGoal);
        SouthGoal.Initialize(gameManager.currentMatch.AwayTeam);

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

    private List<PlayerCharacter> SpawnPlayers(int teamID, ArenaGoal ownGoal)
    {
        var playerNodes = new List<PlayerCharacter>();
        var players = dataLoader.GetSquad(teamID);
        var targetGoal = ownGoal == SouthGoal ? NorthGoal : SouthGoal;

        float halfwayY = 485f; // midpoint of pitch

        for (int i = 0; i < players.Count; i++)
        {
            var spawnNode = spawns.GetChild<Node2D>(i);
            Vector2 playerPosition = spawnNode.GlobalPosition;
            var entranceNode = entrance.GetChild<Node2D>(i);
            Vector2 entrancePosition = entranceNode.GlobalPosition;
            var preentranceNode = preentrance.GetChild<Node2D>(0);
            Vector2 preentrancePosition = preentranceNode.GlobalPosition;

            // If this is the away team, mirror vertically
            if (ownGoal == NorthGoal)
            {
                playerPosition.Y = 2 * halfwayY - playerPosition.Y;
                entrancePosition.Y = 2 * halfwayY - entrancePosition.Y;
                preentrancePosition.Y = 2 * halfwayY - preentrancePosition.Y;
            }

            var playerData = players[i] as PlayerResource;

            Vector2 kickoffPosition;
            if (i > 8)
            {
                // Pull from the dedicated kickoff nodes
                kickoffPosition = kickoffs.GetChild<Node2D>(i - 9).GlobalPosition;

                // Mirror vertically ONLY if this specific team is playing on the south side
                if (ownGoal == NorthGoal)
                {
                    kickoffPosition.Y = 2 * halfwayY - kickoffPosition.Y + 10;
                }
            }
            else
            {
                // Fall back to the player's standard spawn position 
                // (This has already been correctly mirrored above if they are TeamGoingSouth)
                kickoffPosition = playerPosition;
            }

            var player = SpawnPlayer(playerPosition, kickoffPosition, ownGoal, targetGoal, playerData, teamID, preentrancePosition, entrancePosition);
            if (i == 10)
                player.IsKickingOffPlayer = true;
            playerNodes.Add(player);
        }

        return playerNodes;
    }

    private PlayerCharacter SpawnPlayer(Vector2 position, Vector2 kickoffPos, ArenaGoal ownGoal, ArenaGoal targetGoal, PlayerResource data, int teamID,
        Vector2 preentrancePosition, Vector2 entrancePosition)
    {
        var player = playerPrefab.Instantiate<PlayerCharacter>();

        // 2. Initialize the common fields exactly like before!
        // Because both types are PlayerCharacters, this method works seamlessly on either.
        player.Initialize(position, kickoffPos, Ball, ownGoal, targetGoal, data, teamID, preentrancePosition, entrancePosition);

        player.SwapRequested += OnPlayerSwapRequest;

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
            Ball.Carrier.TeamID == gameManager.playerSetup[0] &&
            Ball.Carrier.controlScheme != PlayerCharacter.ControlScheme.P1 &&
            Ball.Carrier.controlScheme != PlayerCharacter.ControlScheme.P2)
        {
            var squad1 = Ball.Carrier.TeamID == squadHome[0].TeamID ? squadHome : squadAway;
            var squad2 = Ball.Carrier.TeamID == squadHome[0].TeamID ? squadHome : squadAway;
            var currentHuman1 = squad1.Find(p => p.controlScheme == PlayerCharacter.ControlScheme.P1);
            var currentHuman2 = squad1.Find(p => p.controlScheme == PlayerCharacter.ControlScheme.P2);

            if (Ball.Carrier == currentHuman1 || Ball.Carrier == currentHuman2)
                return;

            currentHuman1?.SetControlScheme(PlayerCharacter.ControlScheme.CPU);
            if (currentHuman1 != null) Ball.Carrier.SetControlScheme(PlayerCharacter.ControlScheme.P1);
            if (currentHuman2 != null) Ball.Carrier.SetControlScheme(PlayerCharacter.ControlScheme.P2);
            return;
        }


        var squad = requester.TeamID == squadHome[0].TeamID ? squadHome : squadAway;
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

    private async void CheckForKickoffReadiness()
    {
        // 1. Verify if EVERY player on both teams has completed their entrance
        foreach (var squad in new[] { squadHome, squadAway })
        {
            foreach (var player in squad)
            {
                if (!player.IsReadyForKickoff())
                    return; // Stop here if even one player is still walking in
            }
        }

        await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

        // 2. Everyone has arrived! Now transition all players to RESETING at the same time
        foreach (var squad in new[] { squadHome, squadAway })
        {
            foreach (var player in squad)
            {
                bool teamIsKickingOff = player.TeamID == gameManager.currentMatch.HomeTeam;
                Vector2 initialPosition =
                    teamIsKickingOff
                    ? player.kickoffPosition
                    : player.spawnPosition;
                player.SwitchState(PlayerCharacter.State.RESETING,
                    PlayerStateData.Build().SetResetPosition(initialPosition));
            }
        }

        // 3. Complete kickoff initialization sequence
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
            var playerSquad = squadHome[0].TeamID == p1Team ? squadHome : squadAway;
            playerSquad[4].SetControlScheme(PlayerCharacter.ControlScheme.P1);
            playerSquad[5].SetControlScheme(PlayerCharacter.ControlScheme.P2);
        }
        else if (gameManager.IsSinglePlayer())
        {
            var playerSquad = squadHome[0].TeamID == p1Team ? squadHome : squadAway;
            playerSquad[5].SetControlScheme(PlayerCharacter.ControlScheme.P1);
        }
        else // versus
        {
            var p1Squad = squadHome[0].TeamID == p1Team ? squadHome : squadAway;
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

        if (Ball?.Carrier == null || Ball.Carrier.TeamID == -1)
            return;

        int playerTeam = gameManager.playerSetup[0];
        if (Ball.Carrier.TeamID == playerTeam)
            return; // Human team is attacking

        var squad = squadHome[0].TeamID == playerTeam ? squadHome : squadAway;

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
