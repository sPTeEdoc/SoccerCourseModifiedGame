using Godot;

[GlobalClass]
public partial class GameStateData : GodotObject
{
    public int TeamScoredOn { get; private set; } = -1;

    public static GameStateData Build()
    {
        return new GameStateData();
    }

    public GameStateData SetTeamScoredOn(int teamID)
    {
        TeamScoredOn = teamID;
        return this;
    }
}