using Godot;

[GlobalClass]
public partial class GameStateScored : GameState
{
    private const int DurationCelebration = 3000;
    private ulong timeSinceCelebration = Time.GetTicksMsec();

    public override void _EnterTree()
    {
        manager.IncreaseScore(stateData.CountryScoredOn);
        timeSinceCelebration = Time.GetTicksMsec();
    }

    public override void _Process(double delta)
    {
        if (Time.GetTicksMsec() - timeSinceCelebration > DurationCelebration)
        {
            TransitionState(GameManager.State.RESET, stateData);
        }
    }
}