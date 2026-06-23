using Godot;
using System;

public partial class Outfielder : PlayerCharacter
{
        // Each subclass will define its own prefix
    public Area2D tackleDamageEmitterArea;
    public override string AnimPrefix => "outfielder/";

    public override void _Ready()
    {
        base._Ready();
        tackleDamageEmitterArea = GetNode<Area2D>("TackleDamageEmitterArea");
        tackleDamageEmitterArea.BodyEntered += base.OnTacklePlayer;
    }

    public override void FlipSprites() // Changed 'new' to 'override'
    {
        base.FlipSprites(); // Runs the base sprite and particle flipping logic first

        // Recalculate scale here so it exists in this context
        float scale = heading == Vector2.Right ? 1 : -1;

        tackleDamageEmitterArea.Scale = new Vector2(scale, tackleDamageEmitterArea.Scale.Y);
    }

    // public void OnTacklePlayer(Node other)
    // {
    //     if (other is PlayerCharacter player &&
    //         player != this &&
    //         player.teamID != teamID &&
    //         ball.Carrier == player)
    //     {
    //         Vector2 direction = Position.DirectionTo(player.Position);
    //         player.GetHurt(direction);
    //     }
    // }
}
