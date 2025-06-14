using Godot;
using System;

public partial class ColorRect : Godot.ColorRect
{

    private Timer _timer;
    // [Signal] public delegate void UserInteractionDialogEventHandler(bool interaction);
    public Action<bool> Interaction;
    public bool interaction = false;
    public override void _Ready()
    {

    }
    
    private void OnTimerTimeoutSignal()
    {
        Interaction?.Invoke(interaction);
        // EmitSignal(SignalName.UserInteractionDialog);
    }
}
