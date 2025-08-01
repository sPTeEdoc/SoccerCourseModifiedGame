using Godot;
using System;

public partial class GameEvents : Node
{
    [Signal] public delegate void BallPossessedEventHandler(string playerName);
    [Signal] public delegate void BallReleasedEventHandler();
    [Signal] public delegate void GameOverEventHandler(string countryWinner);
    [Signal] public delegate void KickoffReadyEventHandler();
    [Signal] public delegate void KickoffStartedEventHandler();
    [Signal] public delegate void ImpactReceivedEventHandler(Vector2 impactPosition, bool isHighImpact);
    [Signal] public delegate void ScoreChangedEventHandler();
    [Signal] public delegate void TeamResetEventTriggeredEventHandler();
    [Signal] public delegate void TeamScoredEventHandler(string countryScoredOn);

    public void EmitGameOver(string winnerCountry)
    {
        EmitSignal("GameOver", winnerCountry);
    }

    public void EmitReset()
    {
        EmitSignal("TeamResetEventTriggered");
    }

    public void EmitKickOffReady()
    {
        EmitSignal("KickoffReady");
    }

    public void EmitKickOffStarted()
    {
        EmitSignal("KickoffStarted");
    }

    public void EmitImpact(Vector2 impactPosition, bool isHighImpact)
    {
        EmitSignal("ImpactReceived", impactPosition, isHighImpact);
    }

    public void EmitScoreChanged()
    {
        EmitSignal("ScoreChanged");
    }
}
