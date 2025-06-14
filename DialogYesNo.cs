using Godot;
using System;

public partial class DialogYesNo : Control
{
    private Timer _timer;
    // [Signal] public delegate void UserInteractionDialogEventHandler(bool interaction);
    public Action<bool> Interaction;
    public bool interaction = false;
    public Enums.DialogResult dialogResult = Enums.DialogResult.None;

    public override void _Ready()
    {
        // Await the ConfirmationResult signal from the dialog
        var popScene = (PopupScene)GD.Load<PackedScene>("res://PopupScene.tscn").Instantiate();
        // var result = await ToSignal(popScene, PopupScene.SignalName.ConfirmationResult);
        this.dialogResult = popScene.dialogResult;

        // 'result' will be an array containing the emitted signal's arguments.
        // In this case, it will contain a single boolean value.
        // bool confirmationResult = (bool)result[0];

        // if (confirmationResult)
        // {
        //     // Create and await a one-shot timer using SceneTree.CreateTimer()
        //     GD.Print("Confirmation received! Waiting for 3 seconds...");
        //     await ToSignal(GetTree().CreateTimer(3.0f), SceneTreeTimer.SignalName.Timeout);
        //     GD.Print("3 seconds elapsed.");
        // }
        // else
        // {
        //     GD.Print("Confirmation cancelled.");
        // }
    }

    private void OnTimerTimeoutSignal()
    {
        Interaction?.Invoke(interaction);
        // EmitSignal(SignalName.UserInteractionDialog);
    }
}
