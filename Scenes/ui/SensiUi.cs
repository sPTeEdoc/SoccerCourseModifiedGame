using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public partial class SensiUi : CanvasLayer
{
    private AnimationPlayer animationPlayer;
    private Label goalScorerLabel;
    private Label scoreInfoLabel;
    [Export] private Label minutesLabel;
    [Export] private Label ballCarrierLabel;
    [Export] private Label homeTeamLabel;
    [Export] private Label homeScoreLabel;
    [Export] private Label awayTeamLabel;
    [Export] private Label awayScoreLabel;
    [Export] private PanelContainer scoreContainer;
    [Export] private Label HomeGoalSummary;
    [Export] private Label AwayGoalSummary;
    private GameManager gameManager;
    private PlayerResource player;

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        goalScorerLabel = GetNode<Label>("UIContainer/GoalScorerLabel");
        scoreInfoLabel = GetNode<Label>("UIContainer/ScoreInfoLabel");

        gameManager = GetNode<GameManager>("/root/GameManager");

        var gameEvents = GetNode("/root/GameEvents");
        gameEvents.Connect("BallPossessed", new Callable(this, nameof(OnBallPossessed)));
        gameEvents.Connect("BallReleased", new Callable(this, nameof(OnBallReleased)));
        gameEvents.Connect("ScoreChanged", new Callable(this, nameof(OnScoreChanged)));
        gameEvents.Connect("TeamResetEventTriggered", new Callable(this, nameof(OnTeamReset)));
        gameEvents.Connect("GameOver", new Callable(this, nameof(OnGameOver)));
        gameEvents.Connect("KickoffReady", new Callable(this, nameof(OnKickoffReady)));
        gameEvents.Connect("KickoffStarted", new Callable(this, nameof(OnKickoffStarted)));
        gameEvents.Connect("HalfOver", new Callable(this, nameof(OnHalfOver)));
        gameEvents.Connect("Entrance", new Callable(this, nameof(OnEntrance)));

        UpdateClock();
    }

    public override void _Process(double delta)
    {
        UpdateClock();
    }

    private void UpdateClock()
    {
        float time = gameManager.TimeElapsed;
        minutesLabel.Text = $"{Math.Round(time)} Minutes";
    }

    private void OnBallPossessed(int playerID)
    {
        ballCarrierLabel.Visible = true;
        player = GameManagement.Instance.PlayerDictionary[playerID];
        ballCarrierLabel.Text = $"{player.Number} {player.FullName}";
    }

    private void OnBallReleased()
    {
        ballCarrierLabel.Visible = false;
    }

    private void OnScoreChanged(int teamScoredOn)
    {
        if (!gameManager.IsTimeUp())
        {
            string goalString = $"{player.FullName} SCORED!";
            if (player.TeamID == teamScoredOn)
                goalString = goalString + " (og)";
            goalScorerLabel.Text = goalString;
            scoreInfoLabel.Text = ScoreHelper.GetCurrentScoreInfo(gameManager.currentMatch);
            animationPlayer.Play("goal_appear");
        }
    }

    private void OnTeamReset()
    {
        if (gameManager.currentMatch.HasSomeoneScored())
            animationPlayer.Play("goal_hide");
    }

    private void OnGameOver(string winningTeamID)
    {
        ShowScores();
        GD.Print("Show the statistics");
    }

    private void OnKickoffReady()
    {
        ShowScores();
    }

    private void ShowScores()
    {
        homeTeamLabel.Text = GameManagement.Instance.TeamsDictionary[gameManager.currentMatch.HomeTeam].Name;
        awayTeamLabel.Text = GameManagement.Instance.TeamsDictionary[gameManager.currentMatch.AwayTeam].Name;
        awayScoreLabel.Text = gameManager.currentMatch.GoalsAway.ToString();
        homeScoreLabel.Text = gameManager.currentMatch.GoalsHome.ToString();
        scoreContainer.Visible = true;
        CreateGoalSummary(gameManager.currentMatch.awayPlayerGoalTrackerTraditional, ref AwayGoalSummary);
        CreateGoalSummary(gameManager.currentMatch.homePlayerGoalTrackerTraditional, ref HomeGoalSummary);
    }

    private void CreateGoalSummary(List<GoalSummary> goalSummary, ref Label goalSummaryLabel)
    {
        StringBuilder sb = new StringBuilder();

        var goalTracker = goalSummary;
        var playerDict = GameManagement.Instance.PlayerDictionary;

        for (int i = 0; i < goalTracker.Count; i++)
        {
            var trackingItem = goalTracker[i];
            string playerName = playerDict[trackingItem.PlayerID].FullName;

            // Check if this summary represents an own goal
            string ogTag = trackingItem.IsOwnGoal ? " (OG)" : "";

            // Formats minutes and appends (OG) to each timestamp if IsOwnGoal is true
            // Result example: "12'(OG), 45'(OG)" or just "12', 45'"
            string minutesFormatted = string.Join(", ", trackingItem.MinutesScored.Select(m => $"{Math.Truncate(m) + 1}'{ogTag}"));

            // Append the line for this player
            sb.AppendLine($"{playerName} {minutesFormatted}");
        }

        string finalGoalSummary = sb.ToString().TrimEnd();

        goalSummaryLabel.Text = finalGoalSummary.ToString();
    }

    private void OnKickoffStarted()
    {
        scoreContainer.Visible = false;
    }

    private void OnEntrance()
    {
        scoreContainer.Visible = false;
    }

    private void OnHalfOver()
    {
        GD.Print("Show the statistics");
        ShowScores();
    }
}