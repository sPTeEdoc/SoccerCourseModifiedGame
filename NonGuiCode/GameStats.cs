using Godot;
using System;

public partial class GameStats : Node2D
{
	public int assists;
		public int goals;
		public int goalsConceded;
		public int cleansheets;
		public double playerRating;
		public int m_saves;
		public int shotsOnGoal;
		public int shotsTotal;
		public int yellowCards;
		public int redCards;
		public int fouls;
		public int offsides;
		public int minutesWithBall;
		public int penaltyKickAttempts;
		public int penaltyKickGoals;
		public int cornerKicks;
		public int wins;
		public int losses;
		public int draws;
		public int matchesPlayed;

		public void IncreaseSaves()
		{
			LoggingStuff.LogTheEvent("IncreaseSaves");
			m_saves++;
		}

		public int Saves
		{
			get
			{
				return m_saves;
			}
			set
			{
				m_saves = value;
			}
		}

		public void IncreaseShotOnGoal(bool shotOnTarget = true)
		{
			LoggingStuff.LogTheEvent("IncreaseShotOnGoal");
			if (shotOnTarget)
				shotsOnGoal++;
			shotsTotal++;
		}

		public void IncreaseFoulCount(bool yellowCard = false, bool redCard = false)
		{
			fouls++;
			if (yellowCard)
				yellowCards++;
			if (redCard)
				redCards++;
		}

		public GameStats()
		{
			assists = 0;
			goals = 0;
			goalsConceded = 0;
			cleansheets = 0;
			playerRating = 5.0;
			m_saves = 0;
			shotsOnGoal = 0;
			shotsTotal = 0;
			yellowCards = 0;
			redCards = 0;
			fouls = 0;
			offsides = 0;
			minutesWithBall = 0;
			penaltyKickAttempts = 0;
			penaltyKickGoals = 0;
			cornerKicks = 0;
			matchesPlayed = 0;
		}
}
