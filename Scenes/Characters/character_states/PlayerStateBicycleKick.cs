using Godot;

[GlobalClass]
public partial class PlayerStateBicycleKick : PlayerState
{
    private const float BallHeightMin = 1.0f;
    private const float BallHeightMax = 25.0f;
    private const float BonusPower = 2.0f;

    public Node soundPlayer;

    public override void _EnterTree()
    {
        animationPlayer.Play($"{player.AnimPrefix}bicycle_kick");
        ballDetectionArea.BodyEntered += OnBallEntered;
    }

    public override void _ExitTree()
    {
        if (ballDetectionArea != null)
            ballDetectionArea.BodyEntered -= OnBallEntered;
    }


    private void OnBallEntered(Node body)
    {
        if (body is Ball contactBall &&
            contactBall.CanAirConnect(BallHeightMin, BallHeightMax))
        {
            Vector2 destination = targetGoal.GetRandomTargetPosition();
            Vector2 direction = ball.Position.DirectionTo(destination);

            ((SoundPlayer)soundPlayer).Play(SoundPlayer.Sound.POWERSHOT);
            contactBall.Shoot(direction * player.power * BonusPower);
        }
    }

    public override void OnAnimationComplete()
    {
        TransitionState(PlayerCharacter.State.RECOVERING);
    }
}