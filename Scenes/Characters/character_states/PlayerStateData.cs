using Godot;

[GlobalClass]
public partial class PlayerStateData : GodotObject
{
    public Vector2 HurtDirection { get; private set; } = Vector2.Zero;
    public PlayerCharacter PassTarget { get; private set; } = null;
    public Vector2 ResetPosition { get; private set; } = Vector2.Zero;
    public Vector2 PreEntrancePosition { get; private set; } = Vector2.Zero;
    public Vector2 EntrancePosition { get; private set; } = Vector2.Zero;
    public Vector2 ShotDirection { get; private set; } = Vector2.Zero;
    public float ShotPower { get; private set; } = 0f;
    public bool AutoAdvanceToEntrance { get; private set; } = true;

    public static PlayerStateData Build()
    {
        return new PlayerStateData();
    }

    public PlayerStateData SetShotDirection(Vector2 direction)
    {
        ShotDirection = direction;
        return this;
    }

    public PlayerStateData SetShotPower(float power)
    {
        ShotPower = power;
        return this;
    }

    public PlayerStateData SetHurtDirection(Vector2 direction)
    {
        HurtDirection = direction;
        return this;
    }

    public PlayerStateData SetPassTarget(PlayerCharacter player)
    {
        PassTarget = player;
        return this;
    }

    public PlayerStateData SetResetPosition(Vector2 position)
    {
        ResetPosition = position;
        return this;
    }

    public PlayerStateData SetPreEntrancePosition(Vector2 position)
    {
        PreEntrancePosition = position;
        return this;
    }

    public PlayerStateData SetEntrancePosition(Vector2 position)
    {
        EntrancePosition = position;
        return this;
    }

    public PlayerStateData SetAutoAdvanceToEntrance(bool autoAdvance)
    {
        AutoAdvanceToEntrance = autoAdvance;
        return this;
    }
}