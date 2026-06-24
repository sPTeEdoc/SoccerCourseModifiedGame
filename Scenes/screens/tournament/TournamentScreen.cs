using Godot;
using System;
using System.Collections.Generic;

public partial class TournamentScreen : Screen
{
    private Dictionary<Tournament.Stage, Texture2D> StageTextures = new()
    {
        { Tournament.Stage.QuarterFinals, GD.Load<Texture2D>("res://assets/art/ui/teamselection/quarters-label.png") },
        { Tournament.Stage.SemiFinals, GD.Load<Texture2D>("res://assets/art/ui/teamselection/semis-label.png") },
        { Tournament.Stage.Final, GD.Load<Texture2D>("res://assets/art/ui/teamselection/finals-label.png") },
        { Tournament.Stage.Complete, GD.Load<Texture2D>("res://assets/art/ui/teamselection/winner-label.png") },
    };

    private Dictionary<Tournament.Stage, List<Control>> flagContainers;

    private TextureRect stageTexture;
    private Tournament tournament;
    private int playerTeam;

    public GameManager gameManager;

    public SoundPlayer soundPlayer;

    public override void _Ready()
    {
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");
        gameManager = GetNode<GameManager>("/root/GameManager");
        tournament = screenData.Tournament;
        playerTeam = gameManager.playerSetup[0];
        stageTexture = GetNode<TextureRect>("Background/StageTexture");

        flagContainers = new()
        {
            { Tournament.Stage.QuarterFinals, new List<Control> {
                GetNode<Control>("Background/BracketsContainer/QFLeftContainer"),
                GetNode<Control>("Background/BracketsContainer/QFRightContainer")
            }},
            { Tournament.Stage.SemiFinals, new List<Control> {
                GetNode<Control>("Background/BracketsContainer/SFLeftContainer"),
                GetNode<Control>("Background/BracketsContainer/SFRightContainer")
            }},
            { Tournament.Stage.Final, new List<Control> {
                GetNode<Control>("Background/BracketsContainer/FinalLeftContainer"),
                GetNode<Control>("Background/BracketsContainer/FinalRightContainer")
            }},
            { Tournament.Stage.Complete, new List<Control> {
                GetNode<Control>("Background/WinnerContainer")
            }},
        };

        if (tournament.CurrentStage == Tournament.Stage.Complete)
            musicPlayer.PlayMusic(MusicPlayer.Music.WIN);

        RefreshBrackets();
    }

    public override void _Process(double delta)
    {
        if (KeyUtils.IsActionJustPressed(PlayerCharacter.ControlScheme.P1, KeyUtils.Action.AButton))
        {
            if (tournament.CurrentStage < Tournament.Stage.Complete)
                TransitionScreen(SoccerGame.ScreenType.InGame, screenData);
            else
                TransitionScreen(SoccerGame.ScreenType.MainMenu);

            soundPlayer.Play(SoundPlayer.Sound.UI_SELECT);
        }
    }

    private void RefreshBrackets()
    {
        foreach (Tournament.Stage stage in Enum.GetValues(typeof(Tournament.Stage)))
        {
            if (stage > tournament.CurrentStage)
                break;

            RefreshBracketStage(stage);
        }
    }

    private void RefreshBracketStage(Tournament.Stage stage)
    {
        var flagNodes = GetFlagNodesForStage(stage);
        stageTexture.Texture = StageTextures[stage];

        if (stage < Tournament.Stage.Complete)
        {
            var matches = tournament.Matches[stage];
            for (int i = 0; i < matches.Count; i++)
            {
                Match currentMatch = matches[i];
                BracketFlag flagHome = flagNodes[i * 2];
                BracketFlag flagAway = flagNodes[i * 2 + 1];

                flagHome.Texture = FlagHelper.GetTexture(GameManagement.teamsDictionary[currentMatch.TeamHome].Name);
                flagAway.Texture = FlagHelper.GetTexture(GameManagement.teamsDictionary[currentMatch.TeamAway].Name);

                if (currentMatch.Winner > -1)
                {
                    var flagWinner = currentMatch.Winner == currentMatch.TeamHome ? flagHome : flagAway;
                    var flagLoser = flagWinner == flagHome ? flagAway : flagHome;

                    flagWinner.SetAsWinner(currentMatch.FinalScore);
                    flagLoser.SetAsLoser();
                }
                else if ((currentMatch.TeamHome == playerTeam || currentMatch.TeamAway == playerTeam) && stage == tournament.CurrentStage)
                {
                    var flagPlayer = currentMatch.TeamHome == playerTeam ? flagHome : flagAway;
                    flagPlayer.SetAsCurrentTeam();
                    gameManager.currentMatch = currentMatch;
                }
            }
        }
        else
        {
            flagNodes[0].Texture = FlagHelper.GetTexture(
                GameManagement.teamsDictionary[tournament.Winner].Name);
        }
    }

    private List<BracketFlag> GetFlagNodesForStage(Tournament.Stage stage)
    {
        var flagNodes = new List<BracketFlag>();
        foreach (var container in flagContainers[stage])
        {
            foreach (Node child in container.GetChildren())
            {
                if (child is BracketFlag flag)
                    flagNodes.Add(flag);
            }
        }
        return flagNodes;
    }
}
