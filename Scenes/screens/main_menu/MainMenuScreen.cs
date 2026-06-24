using Godot;
using System;
using System.Collections.Generic;

public partial class MainMenuScreen : Screen
{
    private readonly Texture2D[,] MenuTextures = new Texture2D[2, 2]
    {
        {
            GD.Load<Texture2D>("res://assets/art/ui/mainmenu/1-player.png"),
            GD.Load<Texture2D>("res://assets/art/ui/mainmenu/1-player-selected.png")
        },
        {
            GD.Load<Texture2D>("res://assets/art/ui/mainmenu/2-players.png"),
            GD.Load<Texture2D>("res://assets/art/ui/mainmenu/2-players-selected.png")
        }
    };

    [Export] private TextureRect SinglePlayerTexture;
    [Export] private TextureRect TwoPlayersTexture;
    [Export] private TextureRect SelectionIcon;

    private List<TextureRect> selectableMenuNodes;
    private int currentSelectedIndex = 0;
    private bool isActive = false;

    private GameManager gameManager;
    private SoundPlayer soundPlayer;
    private DataLoader dataLoader;

    public override void _Ready()
    {
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");
        gameManager = GetNode<GameManager>("/root/GameManager");
        dataLoader = GetNode<DataLoader>("/root/DataLoader");
        SinglePlayerTexture = GetNode<TextureRect>("Background/SinglePlayerTexture");
        TwoPlayersTexture = GetNode<TextureRect>("Background/TwoPlayersTexture");
        SelectionIcon = GetNode<TextureRect>("Background/SelectionIcon");

        selectableMenuNodes = new() { SinglePlayerTexture, TwoPlayersTexture };
        RefreshUI();
    }

    public override void _Process(double delta)
    {
        if (!isActive)
            return;

        if (KeyUtils.IsActionJustPressed(PlayerCharacter.ControlScheme.P1, KeyUtils.Action.UP))
            ChangeSelectedIndex(currentSelectedIndex - 1);
        else if (KeyUtils.IsActionJustPressed(PlayerCharacter.ControlScheme.P1, KeyUtils.Action.DOWN))
            ChangeSelectedIndex(currentSelectedIndex + 1);
        else if (KeyUtils.IsActionJustPressed(PlayerCharacter.ControlScheme.P1, KeyUtils.Action.AButton))
            SubmitSelection();
    }

    private void RefreshUI()
    {
        for (int i = 0; i < selectableMenuNodes.Count; i++)
        {
            selectableMenuNodes[i].Texture = MenuTextures[i, currentSelectedIndex == i ? 1 : 0];
            if (currentSelectedIndex == i)
                SelectionIcon.Position = selectableMenuNodes[i].Position + Vector2.Left * 25;
        }
    }

    private void ChangeSelectedIndex(int newIndex)
    {
        currentSelectedIndex = Mathf.Clamp(newIndex, 0, selectableMenuNodes.Count - 1);
        soundPlayer.Play(SoundPlayer.Sound.UI_NAV);
        RefreshUI();
    }

    private void SubmitSelection()
    {
        soundPlayer.Play(SoundPlayer.Sound.UI_SELECT);

        var teamDefault = dataLoader.GetTeams()[0];
        var playerTwo = currentSelectedIndex == 0 ? -2 : teamDefault;

        gameManager.playerSetup = new int[] { teamDefault, playerTwo };
        TransitionScreen(SoccerGame.ScreenType.TeamSelection);
    }

    private void OnSetActive()
    {
        RefreshUI();
        isActive = true;
    }
}