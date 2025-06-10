using Godot;
using System;

public interface iObserver
{
 	void UpdateAnnouncer(string eventString);
	void UpdateOnCamera();
	void UpdateTimeTicked(string timePassed);
	void UpdateHalf();
	void UpdateScoreboard();
	void UpdateGameIsOver();
}
