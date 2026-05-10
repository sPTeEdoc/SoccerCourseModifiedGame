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

        string countryStarting = stateData.CountryScoredOn;

        if (string.IsNullOrEmpty(countryStarting))
            countryStarting = manager.currentMatch.CountryHome;

        if (countryStarting == manager.playerSetup[0])
            validControlSchemes.Add(PlayerCharacter.ControlScheme.P1);

        if (countryStarting == manager.playerSetup[1])
            validControlSchemes.Add(PlayerCharacter.ControlScheme.P2);

        if (validControlSchemes.Count == 0)
            validControlSchemes.Add(PlayerCharacter.ControlScheme.P1);
    }

    public override void _Process(double delta)
    {
        foreach (var controlScheme in validControlSchemes)
        {
            if (KeyUtils.IsActionJustPressed(controlScheme, KeyUtils.Action.PASS))
            {
                gameEvents.EmitKickOffStarted();
                soundPlayer.Play(SoundPlayer.Sound.WHISTLE);
                TransitionState(GameManager.State.IN_PLAY);
            }
        }
    }
}