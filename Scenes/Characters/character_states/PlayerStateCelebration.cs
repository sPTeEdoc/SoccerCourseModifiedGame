using Godot;
using System;

[GlobalClass]
public partial class PlayerStateCelebrating : PlayerState
{
    private const float AirFriction = 60.0f;
    private const float CelebratingHeight = 2.0f;

    private int initialDelay = GD.RandRange(200, 500);
    private int timeSinceCelebrating = (int)Time.GetTicksMsec();

    public GameEvents gameEvents;

    public override void _EnterTree()
    {

    }

    public override void _Ready()
    {
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        gameEvents.TeamResetEventTriggered += OnTeamReset;
    }

    public override void _Process(double delta)
    {
        if (player.height == 0 && (int)Time.GetTicksMsec() - timeSinceCelebrating > initialDelay)
        {
            Celebrate();
        }

        player.Velocity = player.Velocity.MoveToward(Vector2.Zero, (float)delta * AirFriction);
    }

    private void Celebrate()
    {
        if (player.role == PlayerCharacter.Role.GOALIE)
            animationPlayer.Play($"{player.AnimPrefix}idle");
        else
        {
            animationPlayer.Play($"{player.AnimPrefix}celebrate");
            player.height = 0.1f;
            player.heightVelocity = CelebratingHeight;
        }
        player.Velocity = Vector2.Zero;
    }

    private void OnTeamReset()
    {
        TransitionState(PlayerCharacter.State.RESETING, PlayerStateData.Build().SetResetPosition(player.spawnPosition));
    }

    public override void _ExitTree()
    {
        if (gameEvents != null)
            gameEvents.TeamResetEventTriggered -= OnTeamReset;
    }
}