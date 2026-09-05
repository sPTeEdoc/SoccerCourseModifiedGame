using Godot;
using System;

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
    [Export] private HBoxContainer scoreContainer;
    private GameManager gameManager;
    private float minutesPerSecond = 0;
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

        UpdateClock();

        minutesPerSecond = gameManager.IN_GAME_MINUTES_PER_HALF / gameManager.DURATION_GAME_SEC;
    }

    public override void _Process(double delta)
    {
        UpdateClock();
    }

    private void UpdateClock()
    {
        float time = (gameManager.DURATION_GAME_SEC - gameManager.timeLeft) * minutesPerSecond 
                     + ((gameManager.currentMatch.Half - 1) * gameManager.IN_GAME_MINUTES_PER_HALF);
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
    }

    private void OnKickoffStarted()
    {
        scoreContainer.Visible = false;
    }

    private void OnHalfOver()
    {
        GD.Print("Show the statistics");
        ShowScores();
    }
}