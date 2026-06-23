using Godot;
using System;

[GlobalClass]
public partial class PlayerStateTackling : PlayerState
{
    private const float GROUND_FRICTION = 250.0f;
    private const int DURATION_PRIOR_RECOVERY = 200;

    private bool isTackleComplete = false;
    private int timeFinishTackle = (int)Time.GetTicksMsec();

    public override void _EnterTree()
    {
        // If player is a Goalkeeper, this evaluates to e.g., "gk_tackle"
        // If it's a regular player, it just evaluates to "tackle"
        animationPlayer.Play($"{player.AnimPrefix}tackle");

        tackleDamageEmitterArea.Monitoring = true;
    }

    public override void _Process(double delta)
    {
        if (!isTackleComplete)
        {
            player.Velocity = player.Velocity.MoveToward(Vector2.Zero, (float)delta * GROUND_FRICTION);
            if (player.Velocity == Vector2.Zero)
            {
                isTackleComplete = true;
                timeFinishTackle = (int)Time.GetTicksMsec();
            }
        }
        else if ((int)Time.GetTicksMsec() - timeFinishTackle > DURATION_PRIOR_RECOVERY)
        {
            TransitionState(PlayerCharacter.State.RECOVERING);
        }
    }

    public override void _ExitTree()
    {
        tackleDamageEmitterArea.Monitoring = false;
    }
}