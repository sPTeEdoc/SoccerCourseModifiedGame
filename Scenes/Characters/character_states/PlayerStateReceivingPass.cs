using Godot;
using System;

public partial class PlayerStateReceivingPass : PlayerState
{
    private Vector2 target;
    private float receiveSpeed = 140f;
    float controlRadius = 24f;
    private float passerRating = 50f; // default fallback


    public override void _EnterTree()
    {
        target = player.GetPassTarget();
        // animationPlayer.Play("receive_pass");
        //player.Velocity = Vector2.Zero;
    }

    public void SetPasserRating(float rating)
    {
        passerRating = rating;
    }

    public override void _Process(double delta)
    {
        // Predict a point ahead of the ball based on its velocity
        // Vector2 anticipatedPosition = ball.Position + ball.ConstantLinearVelocity * 0.3f;
        // Use pass rating to scale anticipation trust
        float trustFactor = Mathf.InverseLerp(50f, 99f, passerRating);
        float anticipationWindow = Mathf.Lerp(0.2f, 0.6f, trustFactor);
        Vector2 anticipatedPosition = ball.Position + ball.ConstantLinearVelocity * anticipationWindow;

        // Vector2 anticipatedPosition = ball.Position + ball.ConstantLinearVelocity * anticipationWindow;

        Vector2 direction = player.Position.DirectionTo(anticipatedPosition);
        player.Velocity = direction * receiveSpeed;

        // Face the ball at all times
        player.FaceDirectionOfBall();

        // Intercept opportunity
        if (player.Position.DistanceTo(ball.Position) < controlRadius)
        {
            player.ControlBall();
            ball.Carrier = player;
            ball.SwitchState(Ball.State.CARRIED);

            player.Velocity = Vector2.Zero;
            player.SwitchState(PlayerCharacter.State.MOVING);
            return;
        }

        // Backup arrival logic — if the player nears the original target
        if (player.Position.DistanceTo(target) < 10f)
        {
            player.Velocity = Vector2.Zero;
            player.SwitchState(PlayerCharacter.State.MOVING);
        }
    }

}

