using Godot;
using System;
using System.Collections.Generic;

public partial class TeamSelectionScreen : Screen
{
    private static readonly Vector2 FLAG_ANCHOR_POINT = new(35, 80);
    private static readonly PackedScene FLAG_SELECTOR_PREFAB = GD.Load<PackedScene>("res://scenes/screens/team_selection/flag_selector.tscn");
    private const int NB_COLS = 4;
    private const int NB_ROWS = 2;
    private GameManager gameManager;
    private SoundPlayer soundPlayer;
    private DataLoader dataLoader;
    [Export] private Control flagsContainer;

    private readonly Dictionary<KeyUtils.Action, Vector2I> moveDirs = new()
    {
        { KeyUtils.Action.UP, Vector2I.Up },
        { KeyUtils.Action.DOWN, Vector2I.Down },
        { KeyUtils.Action.LEFT, Vector2I.Left },
        { KeyUtils.Action.RIGHT, Vector2I.Right }
    };

    private Vector2I[] selection = { Vector2I.Zero, Vector2I.Zero };
    private List<FlagSelector> selectors = new();

    public override void _Ready()
    {
        gameManager = GetNode<GameManager>("/root/GameManager");
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");
        dataLoader = GetNode<DataLoader>("/root/DataLoader");
        flagsContainer = GetNode<Control>("Background/FlagsContainer");
        PlaceFlags();
        PlaceSelectors();
    }

    public override void _Process(double delta)
    {
        for (int i = 0; i < selectors.Count; i++)
        {
            var selector = selectors[i];
            if (!selector.IsSelected)
            {
                foreach (var action in moveDirs.Keys)
                {
                    if (KeyUtils.IsActionJustPressed(selector.ControlScheme, action))
                    {
                        TryNavigate(i, moveDirs[action]);
                    }
                }
            }
        }

        if (!selectors[0].IsSelected &&
            KeyUtils.IsActionJustPressed(PlayerCharacter.ControlScheme.P1, KeyUtils.Action.AButton))
        {
            soundPlayer.Play(SoundPlayer.Sound.UI_NAV);
            TransitionScreen(SoccerGame.ScreenType.MainMenu);
        }
    }

    private void TryNavigate(int selectorIndex, Vector2I direction)
    {
        Rect2I gridBounds = new(new Vector2I(0, 0), new Vector2I(NB_COLS, NB_ROWS));

        var newSelection = selection[selectorIndex] + direction;
        if (gridBounds.HasPoint(newSelection))
        {
            selection[selectorIndex] = newSelection;
            int flagIndex = newSelection.X + newSelection.Y * NB_COLS;
            gameManager.playerSetup[selectorIndex] = 
                GameManagement.teamsDictionary[dataLoader.GetTeams()[flagIndex]].TeamID;
            selectors[selectorIndex].Position = flagsContainer.GetChild<TextureRect>(flagIndex).Position;
            soundPlayer.Play(SoundPlayer.Sound.UI_NAV);
        }
    }

    private void PlaceFlags()
    {
        for (int j = 0; j < NB_ROWS; j++)
        {
            for (int i = 0; i < NB_COLS; i++)
            {
                var flagTexture = new TextureRect
                {
                    Position = FLAG_ANCHOR_POINT + new Vector2(55 * i, 50 * j),
                    Texture = FlagHelper.GetTexture(
                        GameManagement.teamsDictionary[dataLoader.GetTeams()[0 + i + j * NB_COLS]].Name),
                    Scale = new Vector2(2, 2),
                    ZIndex = 1
                };
                flagsContainer.AddChild(flagTexture);
            }
        }
    }

    private void PlaceSelectors()
    {
        AddSelector(PlayerCharacter.ControlScheme.P1);

        if (gameManager.playerSetup[1] > -2)
        {
            AddSelector(PlayerCharacter.ControlScheme.P2);
        }
    }

    private void AddSelector(PlayerCharacter.ControlScheme controlScheme)
    {
        var selector = FLAG_SELECTOR_PREFAB.Instantiate<FlagSelector>();
        selector.Position = flagsContainer.GetChild<TextureRect>(0).Position;
        selector.ControlScheme = controlScheme;
        selector.Selected += OnSelectorSelected;
        selectors.Add(selector);
        flagsContainer.AddChild(selector);
    }

    private void OnSelectorSelected()
    {
        foreach (var selector in selectors)
        {
            if (!selector.IsSelected)
                return;
        }

        var teamP1 = gameManager.playerSetup[0];
        var teamP2 = gameManager.playerSetup[1];

        if (teamP2 > -1 && teamP1 != teamP2)
        {
            gameManager.currentMatch = new Match(teamP2, teamP1);
            TransitionScreen(SoccerGame.ScreenType.InGame);
        }
        else
        {
            var tournament = new Tournament();
            AddChild(tournament);
            TransitionScreen(SoccerGame.ScreenType.Tournament, ScreenData.Build().SetTournament(tournament));

        }
    }
}