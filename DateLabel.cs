using Godot;
using System;
using System.Drawing;

public partial class DateLabel : MarginContainer
{
	public Label label;
	public PanelContainer panelContainer;
	public DateTime dateOfDay;

	public DateLabel()
	{

	}

	public override void _Ready()
	{
		panelContainer = GetNode<PanelContainer>("PanelContainer");
		label = GetNode<Label>("PanelContainer/Label");
		label.Text = dateOfDay.Day.ToString();
		SetUnselected();
	}

	private void SetUnselected()
	{
		var currentDate = Season.Instance.seasonGameDate;
		bool isCurrentDate = dateOfDay.Day == currentDate.Day && dateOfDay.Month == currentDate.Month && dateOfDay.Year == currentDate.Year;

		if (!isCurrentDate)
		{
			StyleBoxFlat x = new StyleBoxFlat();
			x.CornerRadiusTopLeft = 0;
			x.CornerRadiusBottomRight = 0;
			x.CornerRadiusTopRight = 0;
			x.CornerRadiusBottomLeft = 0;
			x.SetBorderWidthAll(1);
			x.BgColor = Godot.Colors.White;
			x.BorderColor = Godot.Colors.Black;
			panelContainer.AddThemeStyleboxOverride("panel", x);
		}
	}

	private void OnButtonPressed()
	{
		// do what ever you want
		GD.Print(dateOfDay);
		ShowPopupScreen();
	}

	public void ShowPopupScreen()
	{
		
	}
}