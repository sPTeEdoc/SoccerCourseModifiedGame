using FunnyOldGame;
using Godot;
using System;
using System.Threading.Tasks;

public partial class DialogYesNo : Window
{
    public Enums.DialogResult dialogResult = Enums.DialogResult.None;

    public override void _Ready()
    {
        this.Show();
    }

    private void OnCloseButtonPressed()
    {
        EmitSignal(SignalName.ConfirmationResult, false);
        this.Hide();
    }
    
    // Define the custom signal using a delegate and the [Signal] attribute.
    [Signal]
    public delegate void ConfirmationResultEventHandler(bool result);

    // Instead of 'finished' and 'result' variables, we'll rely on signals.
    private Button _okButton;
    private Button _cancelButton;

    // public override void _Ready()
    // {
    //     _okButton = GetOkButton(); // Get the OK button
    //     _cancelButton = GetCancelButton(); // Get the Cancel button

    //     // Connect the dialog's signals
    //     AboutToPopup += OnAboutToPopup;
    //     _okButton.Pressed += OnOkPressed; // Connect the pressed signal
    //     _cancelButton.Pressed += OnCancelPressed; // Connect the pressed signal
    // }

    // This method will handle the 'AboutToPopup' signal.
    private void OnAboutToPopup()
    {
        // Reset or initialize any state needed when the dialog is about to show.
        // For example, you might set the dialog's text here.
        GD.Print("Confirmation dialog is about to pop up.");
    }

    // This method will handle the OK button's 'Pressed' signal.
    private void OnOkPressed()
    {
        // Emit the custom signal with the confirmation result.
        dialogResult = Enums.DialogResult.Yes;
        Hide(); // Optionally hide the dialog after confirmation
        EmitSignal(SignalName.ConfirmationResult, true); // Emit the signal
    }

    // This method will handle the Cancel button's 'Pressed' signal.
    private void OnCancelPressed()
    {
        // Emit the custom signal with the cancellation result.
        dialogResult = Enums.DialogResult.No;
        Hide(); // Optionally hide the dialog after cancellation
        EmitSignal(SignalName.ConfirmationResult, false); // Emit the signal
    }

    // How to await the result:
    public async Task<bool> GetConfirmationResult()
    {
        // Show the dialog first
        PopupCentered();

        // Wait for the confirmation_result signal to be emitted
        var result = await ToSignal(this, SignalName.ConfirmationResult);

        // The result will be an array containing the emitted signal's arguments.
        // In this case, it will be an array with one element: the boolean confirmation result.
        return (bool)result[0];
    }
}
