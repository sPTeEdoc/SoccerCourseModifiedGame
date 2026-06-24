using Godot;
using System;

public partial class FlagSelector : Control
{
    [Signal]
    public delegate void SelectedEventHandler();

    private AnimationPlayer animationPlayer;
    private TextureRect indicator1P;
    private TextureRect indicator2P;

    public PlayerCharacter.ControlScheme ControlScheme { get; set; } = PlayerCharacter.ControlScheme.P1;
    public bool IsSelected { get; private set; } = false;
    public SoundPlayer soundPlayer;

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        indicator1P = GetNode<TextureRect>("Indicator1P");
        indicator2P = GetNode<TextureRect>("Indicator2P");
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");

        indicator1P.Visible = ControlScheme == PlayerCharacter.ControlScheme.P1;
        indicator2P.Visible = ControlScheme == PlayerCharacter.ControlScheme.P2;
    }

    public override void _Process(double delta)
    {
        if (!IsSelected && KeyUtils.IsActionJustPressed(ControlScheme, KeyUtils.Action.AButton))
        {
            IsSelected = true;
            animationPlayer.Play("selected");
            soundPlayer.Play(SoundPlayer.Sound.UI_SELECT);
            EmitSignal(SignalName.Selected);
        }
        else if (IsSelected && KeyUtils.IsActionJustPressed(ControlScheme, KeyUtils.Action.BButton))
        {
            IsSelected = false;
            animationPlayer.Play("selecting");
        }
    }
}