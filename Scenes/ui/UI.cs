using Godot;
using System;
using System.Collections.Generic;

public partial class UI : CanvasLayer
{
    private AnimationPlayer animationPlayer;
    private List<TextureRect> flagTextures = new();
    private Label goalScorerLabel;
    private Label playerLabel;
    private Label scoreInfoLabel;
    private Label scoreLabel;
    private Label timeLabel;

    private string lastBallCarrier = "";
    private GameManager gameManager;

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        flagTextures.Add(GetNode<TextureRect>("UIContainer/ColorRect/HBoxContainer/HomeFlagTexture"));
        flagTextures.Add(GetNode<TextureRect>("UIContainer/ColorRect/HBoxContainer/AwayFlagTexture"));

        goalScorerLabel = GetNode<Label>("UIContainer/GoalScorerLabel");
        playerLabel = GetNode<Label>("UIContainer/ColorRect/HBoxContainer/PlayerLabel");
        scoreInfoLabel = GetNode<Label>("UIContainer/ScoreInfoLabel");
        scoreLabel = GetNode<Label>("UIContainer/ColorRect/HBoxContainer/ScoreLabel");
        timeLabel = GetNode<Label>("UIContainer/ColorRect/HBoxContainer/TimeLabel");

        gameManager = GetNode<GameManager>("/root/GameManager");

        var gameEvents = GetNode("/root/GameEvents");
        gameEvents.Connect("BallPossessed", new Callable(this, nameof(OnBallPossessed)));
        gameEvents.Connect("BallReleased", new Callable(this, nameof(OnBallReleased)));
        gameEvents.Connect("ScoreChanged", new Callable(this, nameof(OnScoreChanged)));
        gameEvents.Connect("TeamResetEventTriggered", new Callable(this, nameof(OnTeamReset)));
        gameEvents.Connect("GameOver", new Callable(this, nameof(OnGameOver)));

        UpdateScore();
        UpdateFlags();
        UpdateClock();
        playerLabel.Text = "";
    }

    public override void _Process(double delta)
    {
        UpdateClock();
    }

    private void UpdateScore()
    {
        scoreLabel.Text = ScoreHelper.GetScoreText(gameManager.currentMatch);
    }

    private void UpdateFlags()
    {
        int[] teams = {
            gameManager.currentMatch.TeamHome,
            gameManager.currentMatch.TeamAway
        };

        for (int i = 0; i < flagTextures.Count; i++)
        {
            flagTextures[i].Texture = FlagHelper.GetTexture(
                GameManagement.teamsDictionary[teams[i]].Name);
        }
    }

    private void UpdateClock()
    {
        if (gameManager.timeLeft < 0)
            timeLabel.Modulate = Colors.Yellow;

        timeLabel.Text = TimeHelper.GetTimeText(gameManager.timeLeft);
    }

    private void OnBallPossessed(string playerName)
    {
        playerLabel.Text = playerName;
        lastBallCarrier = playerName;
    }

    private void OnBallReleased()
    {
        playerLabel.Text = "";
    }

    private void OnScoreChanged()
    {
        if (!gameManager.IsTimeUp())
        {
            goalScorerLabel.Text = $"{lastBallCarrier} SCORED!";
            scoreInfoLabel.Text = ScoreHelper.GetCurrentScoreInfo(gameManager.currentMatch);
            animationPlayer.Play("goal_appear");
        }

        UpdateScore();
    }

    private void OnTeamReset()
    {
        if (gameManager.currentMatch.HasSomeoneScored())
            animationPlayer.Play("goal_hide");
    }

    private void OnGameOver(string winningTeamID)
    {
        scoreInfoLabel.Text = ScoreHelper.GetFinalScoreInfo(gameManager.currentMatch);
        animationPlayer.Play("game_over");
    }
}