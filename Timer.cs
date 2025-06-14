using Godot;
using System;

public partial class Timer : Godot.Timer
{
    public override void _Ready()
    {
        Timeout += OnTimerTimeoutSignal;
    }
    public void OnTimerTimeoutSignal()
    {
        GD.Print("Timer timeout");
    }
}
