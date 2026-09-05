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

        validControlSchemes.Clear();

        // If a team was scored on, they kick off. Otherwise, use TeamKickingOff (updated by AdvanceHalf)
        int startingTeamID = stateData.TeamScoredOn > -1
            ? stateData.TeamScoredOn
            : manager.currentMatch.TeamKickingOff;

        if (startingTeamID == manager.playerSetup[0])
            validControlSchemes.Add(PlayerCharacter.ControlScheme.P1);

        if (startingTeamID == manager.playerSetup[1])
            validControlSchemes.Add(PlayerCharacter.ControlScheme.P2);

        if (validControlSchemes.Count == 0)
            validControlSchemes.Add(PlayerCharacter.ControlScheme.P1);
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