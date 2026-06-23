using Godot;
using System;

public partial class GoalKeeper : PlayerCharacter
{
    public CollisionShape2D goalieHandsCollider;
    public Area2D permanentDamageEmitterArea;
    public override string AnimPrefix => "goalkeeper/";
    protected override string DefaultSpriteSheet => "res://assets/art/characters/goalkeeper3-Sheet.png";
    protected override string AlternateSpriteSheet => "res://assets/art/characters/goalkeeper3-Sheet.png"; // or same sheet if no alt exists

    public override void _Ready()
    {
        base._Ready();
        goalieHandsCollider = GetNode<CollisionShape2D>("GoalieHands/GoalieHandsCollider");
        goalieHandsCollider.Disabled = false;
        permanentDamageEmitterArea = GetNode<Area2D>("PermanentDamageEmitterArea");
        permanentDamageEmitterArea.Monitoring = true;
        permanentDamageEmitterArea.BodyEntered += base.OnTacklePlayer;
    }
}
