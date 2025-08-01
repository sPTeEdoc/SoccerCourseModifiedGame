using Godot;
using System;

public partial class BallStateData : Node
{
    public int LockDuration { get; private set; } = 0;

    public static BallStateData Build()
    {
        return new BallStateData();
    }

    public BallStateData SetLockDuration(int duration)
    {
        LockDuration = duration;
        return this;
    }
}