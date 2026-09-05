using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class FullFieldActorsContainer : Node2D
{
    private const int DurationWeightCache = 200;

    private PackedScene playerPrefab = GD.Load<PackedScene>("res://scenes/characters/PlayerCharacter.tscn");
    private PackedScene sparkPrefab = GD.Load<PackedScene>("res://scenes/spark/spark.tscn");

    [Export] public Ball ball { get; set; }
    [Export] public ArenaGoal NorthGoal { get; set; }
    [Export] public ArenaGoal SouthGoal { get; set; }

    private Node2D kickoffs;
    private Node2D spawns;
    private Node2D preentrance;
    private Node2D entrance;

    private bool isCheckingForKickoffReadiness = false;
    private bool isHalfTransitioning = false;
    private List<PlayerCharacter> squadHome = new();
    private List<PlayerCharacter> squadAway = new();
    private ulong timeSinceLastCacheRefresh = Time.GetTicksMsec();

    private ulong lastAutoSwitchTime = 0;
    private const int AUTO_SWITCH_INTERVAL = 300;
    private const float MIN_SWITCH_DELTA = 40f;

    public GameManager gameManager;
    public GameEvents gameEvents;
    public DataLoader dataLoader;

    private bool EntrancesMade { get; set; } = false;
    public SoundPlayer soundPlayer;

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
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");
        kickoffs = GetNode<Node2D>("KickOffs");
        spawns = GetNode<Node2D>("Spawns");

        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        gameManager = GetNode<GameManager>("/root/GameManager");
        dataLoader = GetNode<DataLoader>("/root/DataLoader");

        ball = GetNode<Ball>("Ball");
        gameEvents.TeamResetEventTriggered += OnTeamReset;
        gameEvents.ImpactReceived += OnImpactReceived;

        gameManager.currentMatch = new Match(GameManagement.Instance.TeamsDictionary[2].TeamID,
            GameManagement.Instance.TeamsDictionary[3].TeamID);

        preentrance = GetNode<Node2D>("Preentrance");
        entrance = GetNode<Node2D>("Entrance");

        squadHome = SpawnPlayers(gameManager.currentMatch.HomeTeam, SouthGoal);
        NorthGoal.Initialize(gameManager.currentMatch.AwayTeam);

        squadAway = SpawnPlayers(gameManager.currentMatch.AwayTeam, NorthGoal);
        SouthGoal.Initialize(gameManager.currentMatch.HomeTeam);

        gameManager.playerSetup[0] = 3;
        gameManager.playerSetup[1] = -2;

        SetupControlSchemes();

        GameManagement.Instance.IsOnPracticeField = true;
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

    public async void StartHalfOverSequence()
    {
        if (isHalfTransitioning) return;
        isHalfTransitioning = true;
        isCheckingForKickoffReadiness = false;

        // 1. Instantly exit IN_PLAY so GameStateInPlay stops draining timeLeft during the transition
        gameManager.SwitchState(GameManager.State.RESET);

        // 2. Move players off the pitch to preentrance positions
        foreach (var squad in new[] { squadHome, squadAway })
        {
            foreach (var player in squad)
            {
                player.SwitchState(PlayerCharacter.State.PREENTRANCE,
                    PlayerStateData.Build().SetPreEntrancePosition(player.preentracePosition));
            }
        }

        // 3. Trigger UI to display scores and output statistics log
        gameEvents.EmitSignal("HalfOver");

        // 4. Increment half counter (swaps TeamKickingOff in Match.cs)
        gameManager.currentMatch.AdvanceHalf();

        // 5. Check if the match can end or goes into sudden death
        if (gameManager.currentMatch.Half > 2 && !gameManager.currentMatch.IsTied())
        {
            gameEvents.EmitSignal("GameOver", gameManager.currentMatch.Winner.ToString());
            isHalfTransitioning = false;
            return;
        }

        // 6. Delay showing scores/statistics
        await ToSignal(GetTree().CreateTimer(1.5f), SceneTreeTimer.SignalName.Timeout);

        // 7. Swap sides
        SwapSides();

        ball.Carrier = null;

        // Reset game timer for the new half (safe now because GameManager is in State.RESET)
        gameManager.timeLeft = gameManager.DURATION_GAME_SEC;

        // 8. Transition players to reset spots
        foreach (var squad in new[] { squadHome, squadAway })
        {
            foreach (var player in squad)
            {
                bool teamIsKickingOff = player.TeamID == gameManager.currentMatch.TeamKickingOff;
                Vector2 initialPosition = teamIsKickingOff
                    ? player.kickoffPosition
                    : player.spawnPosition;

                player.SwitchState(PlayerCharacter.State.RESETING,
                    PlayerStateData.Build().SetResetPosition(initialPosition));
            }
        }

        EntrancesMade = true;
        isHalfTransitioning = false;
        isCheckingForKickoffReadiness = true;
    }

    private void SwapSides()
    {
        // Re-assign goals for the teams
        NorthGoal.Initialize(gameManager.currentMatch.HomeTeam);
        SouthGoal.Initialize(gameManager.currentMatch.AwayTeam);

        // Update squad goal assignments and mirror positions
        UpdateSquadSidePositions(squadHome);
        UpdateSquadSidePositions(squadAway);
    }

    private void UpdateSquadSidePositions(List<PlayerCharacter> squad)
    {
        float halfwayY = 485f;

        for (int i = 0; i < squad.Count; i++)
        {
            var player = squad[i];

            // Swap goal references
            var oldOwn = player.ownGoal;
            player.ownGoal = player.targetGoal;
            player.targetGoal = oldOwn;

            // Recalculate target positions based on new goal side
            Vector2 playerPosition = spawns.GetChild<Node2D>(i).GlobalPosition;
            Vector2 entrancePosition = entrance.GetChild<Node2D>(i).GlobalPosition;
            Vector2 preentrancePosition = preentrance.GetChild<Node2D>(0).GlobalPosition;

            Vector2 kickoffPosition;
            if (i > 8)
            {
                kickoffPosition = kickoffs.GetChild<Node2D>(i - 9).GlobalPosition;
                if (player.ownGoal == NorthGoal)
                {
                    kickoffPosition.Y = 2 * halfwayY - kickoffPosition.Y + 10;
                }
            }
            else
            {
                kickoffPosition = playerPosition;
            }

            if (player.ownGoal == NorthGoal)
            {
                playerPosition.Y = 2 * halfwayY - playerPosition.Y;
                entrancePosition.Y = 2 * halfwayY - entrancePosition.Y;
                preentrancePosition.Y = 2 * halfwayY - preentrancePosition.Y;
            }

            player.spawnPosition = playerPosition;
            player.entrancePosition = entrancePosition;
            player.preentracePosition = preentrancePosition;
            player.kickoffPosition = kickoffPosition;
        }
    }

    private List<PlayerCharacter> SpawnPlayers(int teamID, ArenaGoal ownGoal)
    {
        var playerNodes = new List<PlayerCharacter>();
        List<PlayerResource> players = dataLoader.GetSquad(teamID);
        var targetGoal = ownGoal == SouthGoal ? NorthGoal : SouthGoal;

        float halfwayY = 485f;

        for (int i = 0; i < players.Count; i++)
        {
            var spawnNode = spawns.GetChild<Node2D>(i);
            Vector2 playerPosition = spawnNode.GlobalPosition;
            var entranceNode = entrance.GetChild<Node2D>(i);
            Vector2 entrancePosition = entranceNode.GlobalPosition;
            var preentranceNode = preentrance.GetChild<Node2D>(0);
            Vector2 preentrancePosition = preentranceNode.GlobalPosition;

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
                kickoffPosition = kickoffs.GetChild<Node2D>(i - 9).GlobalPosition;

                if (ownGoal == NorthGoal)
                {
                    kickoffPosition.Y = 2 * halfwayY - kickoffPosition.Y + 10;
                }
            }
            else
            {
                kickoffPosition = playerPosition;
            }

            var player = SpawnPlayer(playerPosition, kickoffPosition, ownGoal, targetGoal, playerData, teamID, preentrancePosition, entrancePosition);
            player.playerID = playerData.PlayerID;
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
        player.Initialize(position, kickoffPos, ball, ownGoal, targetGoal, data, teamID, preentrancePosition, entrancePosition);
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
                p1.spawnPosition.DistanceSquaredTo(ball.Position).CompareTo(p2.spawnPosition.DistanceSquaredTo(ball.Position)));

            for (int i = 0; i < cpuPlayers.Count; i++)
                cpuPlayers[i].weightOnDutySteering = 1f - Ease(i / 10f, 0.1f);
        }
    }

    private void OnPlayerSwapRequest(PlayerCharacter requester)
    {
        if (ball.Carrier != null && (ball.Carrier.controlScheme == PlayerCharacter.ControlScheme.P1 ||
            ball.Carrier.controlScheme == PlayerCharacter.ControlScheme.P2))
        {
            return;
        }

        if (ball.Carrier != null &&
            ball.Carrier.TeamID == gameManager.playerSetup[0] &&
            ball.Carrier.controlScheme != PlayerCharacter.ControlScheme.P1 &&
            ball.Carrier.controlScheme != PlayerCharacter.ControlScheme.P2)
        {
            var squad1 = ball.Carrier.TeamID == squadHome[0].TeamID ? squadHome : squadAway;
            var currentHuman1 = squad1.Find(p => p.controlScheme == PlayerCharacter.ControlScheme.P1);
            var currentHuman2 = squad1.Find(p => p.controlScheme == PlayerCharacter.ControlScheme.P2);

            if (ball.Carrier == currentHuman1 || ball.Carrier == currentHuman2)
                return;

            currentHuman1?.SetControlScheme(PlayerCharacter.ControlScheme.CPU);
            if (currentHuman1 != null) ball.Carrier.SetControlScheme(PlayerCharacter.ControlScheme.P1);
            if (currentHuman2 != null) ball.Carrier.SetControlScheme(PlayerCharacter.ControlScheme.P2);
            return;
        }

        var squad = requester.TeamID == squadHome[0].TeamID ? squadHome : squadAway;
        var cpuPlayers = squad.FindAll(p => p.controlScheme == PlayerCharacter.ControlScheme.CPU && p.role != PlayerCharacter.Role.GOALIE);
        cpuPlayers.Sort((p1, p2) =>
            p1.Position.DistanceSquaredTo(ball.Position).CompareTo(p2.Position.DistanceSquaredTo(ball.Position)));

        var closest = cpuPlayers[0];
        if (closest.Position.DistanceSquaredTo(ball.Position) < requester.Position.DistanceSquaredTo(ball.Position))
        {
            var oldScheme = requester.controlScheme;
            requester.SetControlScheme(PlayerCharacter.ControlScheme.CPU);
            closest.SetControlScheme(oldScheme);
        }
    }

    private bool _isTransitioningToReset = false;

    private async void CheckForKickoffReadiness()
    {
        if (_isTransitioningToReset) return;

        if (!EntrancesMade)
        {
            foreach (var squad in new[] { squadHome, squadAway })
            {
                foreach (var player in squad)
                {
                    if (!player.IsReadyToGoToKickoffSpots())
                        return;
                }
            }

            _isTransitioningToReset = true;
            await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

            foreach (var squad in new[] { squadHome, squadAway })
            {
                foreach (var player in squad)
                {
                    bool teamIsKickingOff = player.TeamID == gameManager.currentMatch.TeamKickingOff;
                    Vector2 initialPosition = teamIsKickingOff
                        ? player.kickoffPosition
                        : player.spawnPosition;

                    player.SwitchState(PlayerCharacter.State.RESETING,
                        PlayerStateData.Build().SetResetPosition(initialPosition));
                }
            }

            EntrancesMade = true;
            _isTransitioningToReset = false;
            return;
        }

        foreach (var squad in new[] { squadHome, squadAway })
        {
            foreach (var player in squad)
            {
                if (!player.IsReadyForKickoff())
                    return;
                if (gameManager.currentMatch.TeamKickingOff == player.TeamID)
                    if (player.IsKickingOffPlayer)
                        ball.Carrier = player;
            }
        }

        isCheckingForKickoffReadiness = false;
        NorthGoal.layer.ZIndex = 0;
        SouthGoal.layer.ZIndex = 0;
        NorthGoal.goalCounted = false;
        SouthGoal.goalCounted = false;
        ball.IsInNet = false;

        gameEvents.EmitSignal("KickoffReady");
        soundPlayer.Play(SoundPlayer.Sound.WHISTLE);
        await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

        GD.Print("Kickoff Ready");

        // Activate GameStateKickoff so _Process checks for input again
        gameManager.SwitchState(GameManager.State.KICKOFF); ;
    }

    private void SetupControlSchemes()
    {
        ResetControlSchemes();
        int p1Team = gameManager.playerSetup[0];

        if (gameManager.IsCoop())
        {
            var playerSquad = squadHome[0].TeamID == p1Team ? squadHome : squadAway;
            playerSquad[10].SetControlScheme(PlayerCharacter.ControlScheme.P1);
            playerSquad[9].SetControlScheme(PlayerCharacter.ControlScheme.P2);
        }
        else if (gameManager.IsSinglePlayer())
        {
            var playerSquad = squadHome[0].TeamID == p1Team ? squadHome : squadAway;
            playerSquad[10].SetControlScheme(PlayerCharacter.ControlScheme.P1);
        }
        else
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

        if (ball?.Carrier == null || ball.Carrier.TeamID == -1)
            return;

        int playerTeam = gameManager.playerSetup[0];
        if (ball.Carrier.TeamID == playerTeam)
            return;

        var squad = squadHome[0].TeamID == playerTeam ? squadHome : squadAway;

        var humans = squad.FindAll(p =>
            p.controlScheme == PlayerCharacter.ControlScheme.P1 ||
            p.controlScheme == PlayerCharacter.ControlScheme.P2);

        var cpuDefenders = squad.FindAll(p =>
            p.controlScheme == PlayerCharacter.ControlScheme.CPU &&
            p.role != PlayerCharacter.Role.GOALIE);

        if (cpuDefenders.Count == 0 || humans.Count == 0)
            return;

        cpuDefenders.Sort((a, b) =>
            a.Position.DistanceSquaredTo(ball.Position)
            .CompareTo(b.Position.DistanceSquaredTo(ball.Position)));

        foreach (var human in humans)
        {
            var bestCandidate = cpuDefenders.FirstOrDefault();
            if (bestCandidate == null)
                continue;

            float currentDist = human.Position.DistanceTo(ball.Position);
            float candidateDist = bestCandidate.Position.DistanceTo(ball.Position);

            if (candidateDist + MIN_SWITCH_DELTA < currentDist)
            {
                var oldScheme = human.controlScheme;
                human.SetControlScheme(PlayerCharacter.ControlScheme.CPU);
                bestCandidate.SetControlScheme(oldScheme);
                lastAutoSwitchTime = Time.GetTicksMsec();
            }
        }
    }
}