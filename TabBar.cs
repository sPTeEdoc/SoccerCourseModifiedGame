using Godot;
using System;

public partial class TabBar : Godot.TabBar
{
	TabBar()
	{
		TeamRepository.Instance.LoadTeams();
	}
}
