using FunnyOldGame;
using FunnyOldGameRedux;
using Godot;
using System;
using System.Threading.Tasks;

public partial class DateLabel : MarginContainer
{
	private DialogYesNo popScene = null;
    private Window view_port;

	public Label label;
	public PanelContainer panelContainer;
	public DateTime dateOfDay;

	public bool Visible = false; 

	public Sprite2D leagueDay;
	[Signal] public delegate void UserInteractionDialogEventHandler(bool interaction);

	public string textureString;

	public override void _Ready()
	{
		// pus = ResourceLoader.Load<PackedScene>("res://YAP.tscn");
		panelContainer = GetNode<PanelContainer>("PanelContainer");
		label = GetNode<Label>("PanelContainer/TextureRect/Label");
		label.Text = dateOfDay.Day.ToString();
		label.Visible = Visible;
		leagueDay = GetNode<Sprite2D>("PanelContainer/Leagueday");
		label.AddThemeColorOverride("font_color", Godot.Colors.Black);
		if (textureString == "" || !Visible)
		{
			leagueDay.Visible = false;
		}
		else
		{
			if (textureString == "leagueday")
			{
				label.AddThemeColorOverride("font_color", Godot.Colors.White);
			}
			var newTexture = (Texture2D)GD.Load($"res://assets/Logos/{textureString}.jpg");
			leagueDay.Texture = newTexture;
		}
		SetUnselected();
	}

	private void SetUnselected()
	{
		var currentDate = Season.Instance.seasonGameDate;
		bool isCurrentDate = dateOfDay.Day == currentDate.Day && dateOfDay.Month == currentDate.Month && dateOfDay.Year == currentDate.Year;

		if (!isCurrentDate)
		{
			StyleBoxFlat styleBox = new StyleBoxFlat();
			styleBox.CornerRadiusTopLeft = 0;
			styleBox.CornerRadiusBottomRight = 0;
			styleBox.CornerRadiusTopRight = 0;
			styleBox.CornerRadiusBottomLeft = 0;
			styleBox.SetBorderWidthAll(1);
			styleBox.BgColor = Godot.Colors.White;
			styleBox.BorderColor = Godot.Colors.Black;
			panelContainer.AddThemeStyleboxOverride("panel", styleBox);
		}
		else
		{

		}
	}

	private void OnButtonPressed()
	{
		// do what ever you want
		if (Visible)
		{
			ShowPopupScreen();
		}
	}

	private async void ShowPopup()
	{
		view_port = GetWindow();
		popScene = (DialogYesNo)GD.Load<PackedScene>("res://DialogYesNo.tscn").Instantiate();
		view_port.AddChild(popScene);
        var result = await ToSignal(popScene, DialogYesNo.SignalName.ConfirmationResult);
        Enums.DialogResult dialogResult = popScene.dialogResult;
	}

	public void ShowPopupScreen()
	{
		this.ShowPopup();
	}
}