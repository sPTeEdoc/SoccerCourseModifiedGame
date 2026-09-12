using Godot;
using System;

public partial class BallStateData : Node
{
    public int LockDuration { get; private set; } = 0;
    public PlayerCharacter PassReceiver { get; private set; } = null;

    public static BallStateData Build()
    {
        return new BallStateData();
    }

    public BallStateData SetLockDuration(int duration)
    {
        LockDuration = duration;
        return this;
    }

    public BallStateData SetPassReceiver(PlayerCharacter receiver)
    {
        PassReceiver = receiver;
        return this;
    }
}