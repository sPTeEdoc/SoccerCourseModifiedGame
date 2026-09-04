using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class GameStateKickoff : GameState
{
    public GameEvents gameEvents;
    public SoundPlayer soundPlayer;

    private List<PlayerCharacter.ControlScheme> validControlSchemes = new();

    public override void _EnterTree()
    {
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");
        gameEvents = GetNode<GameEvents>("/root/GameEvents");

        int startingTeamID = stateData.TeamScoredOn;

        if (startingTeamID > -1)
            startingTeamID = manager.currentMatch.HomeTeam;

        if (startingTeamID == manager.playerSetup[0])
            validControlSchemes.Add(PlayerCharacter.ControlScheme.P1);

        if (startingTeamID == manager.playerSetup[1])
            validControlSchemes.Add(PlayerCharacter.ControlScheme.P2);

        if (validControlSchemes.Count == 0)
            validControlSchemes.Add(PlayerCharacter.ControlScheme.P1);

        gameEvents.CallDeferred(nameof(GameEvents.EmitReset));
    }

    public override void _Process(double delta)
    {
        foreach (var controlScheme in validControlSchemes)
        {
            if (KeyUtils.IsActionJustPressed(controlScheme, KeyUtils.Action.SHOOT))
            {
                gameEvents.EmitKickOffStarted();
                TransitionState(GameManager.State.IN_PLAY);
            }
        }
    }
}