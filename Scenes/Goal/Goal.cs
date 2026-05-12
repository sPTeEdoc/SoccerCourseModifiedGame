using Godot;

public partial class Goal : Node2D
{
    [Export] private Area2D backNetArea;
    [Export] private Area2D scoringArea;
    [Export] private Node targets;

    private int teamID = -1;

    private GameEvents gameEvents;
    private SoundPlayer soundPlayer;

    public override void _Ready()
    {
        backNetArea = GetNode<Area2D>("BackNetArea");
        scoringArea = GetNode<Area2D>("ScoringArea");
        targets = GetNode<Node>("Targets");

        backNetArea.BodyEntered += OnBallEnterBackNet;
        scoringArea.BodyEntered += OnBallEnterScoringArea;
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");
    }

    public void Initialize(int contextTeam)
    {
        teamID = contextTeam;
    }

    private void OnBallEnterBackNet(Node body)
    {
        if (body is Ball ball)
            ball.Stop();
    }

    private void OnBallEnterScoringArea(Node body)
    {
        if (body is Ball)
        {
            soundPlayer.Play(SoundPlayer.Sound.WHISTLE);
            gameEvents.EmitSignal("TeamScored", teamID);
        }
    }

    public Vector2 GetRandomTargetPosition()
    {
        int index = (int)GD.Randi() % targets.GetChildCount();
        return targets.GetChild<Node2D>(index).GlobalPosition;
    }

    public Vector2 GetCenterTargetPosition()
    {
        int index = (int)(targets.GetChildCount() / 2.0);
        return targets.GetChild<Node2D>(index).GlobalPosition;
    }

    public Vector2 GetTopTargetPosition()
    {
        return targets.GetChild<Node2D>(0).GlobalPosition;
    }

    public Vector2 GetBottomTargetPosition()
    {
        int index = targets.GetChildCount() - 1;
        return targets.GetChild<Node2D>(index).GlobalPosition;
    }

    public Area2D GetScoringArea()
    {
        return scoringArea;
    }
}