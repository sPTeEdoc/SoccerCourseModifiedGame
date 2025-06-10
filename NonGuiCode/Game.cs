using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

public class Game
{
	public Team homeTeam;
		public Team awayTeam;
		public int halfNumber = 1;
		private int minutesPlayed = 0;
		private int minutesPerHalf;
		private int minutesPerHalfPlusInjury = 0;
		private bool gameIsGoing = true;
		private Team AttackingTeam;
		private Team DefendingTeam;
		private bool homeTeamHadBallFirst = true;
		public int homeScore = 0;
		public int awayScore = 0;
		public int homeFirstLeg = -1;
		public int awayFirstLeg = -1;
		private Referee referee;
		private bool attackingTeamIsHomeTeam;
		private bool specialSaveRequired;
		private bool changedPossessionAtLeastOnceOrMidfieldBattle = false;
		private bool silvergoalOT = false;
		private bool traditionalOT = false;
		private bool penaltyKicksOnlyOT = false;
		private bool kickOff = true;
		//private Die Dice.Instance.d6 = new Die(6);
		//private Die d100 = new Die(100);
		private bool PKScored = false;
		public List<Player> awaySubs = new List<Player>();
		public List<Player> homeSubs = new List<Player>();
		public int numberOfAwaySubsLeft = 3;
		public int numberOfHomeSubsLeft = 3;
		public Dictionary<Player, List<string>> awayPlayerGoalTrackerTraditional = new Dictionary<Player, List<string>>();
		public Dictionary<Player, List<string>> homePlayerGoalTrackerTraditional = new Dictionary<Player, List<string>>();
		public List<string> gameSummary = new List<string>();
		private int maxRating = 8;
		private bool forceSub = true;
		private Enums.WeatherIssue weather;
		private ArrayList observers = new ArrayList();
		public int gameSpeed = 1500;
		private Thread m_SimulateEverything;
		public int homePKScore;
		public int awayPKScore;
		public int currentPKRound;
		private int minInjuryTime = 0;
		private int timeWhenSeizedPossession = 0;
		private bool watchingGame = false;
		public GameStats homeGameStats;
		public GameStats awayGameStats;
		public string GameResult = "";
		public Team Winner = null;
		public Team Loser = null;
		public bool GamePlayed = false;
		private Enums.YellowCardRegulations yellowCardRegs = Enums.YellowCardRegulations.EPL;
		private bool ballIsLive = false;
		public static int shotAttemptValue = 75;
		public DateTime gameDate = DateTime.Now;
		public bool FACupGame = false;
		public bool isPlayoffOrFriendly = false;
		public string leagueName = "";
		public int FixtureOrRoundNumber = 0;
		public string gameName = "";
		public bool isEuroKnockoutRound = false;

		public void BeginGame(bool watchingGame = true, Enums.YellowCardRegulations yellowCardRegs = Enums.YellowCardRegulations.EPL) 
		{
			if (!isPlayoffOrFriendly)
			{
				foreach (Player p in homeTeam.completeRoster)
				{
					p.InitializePlayerStats(leagueName, homeTeam.Name);
				}

				foreach (Player p in awayTeam.completeRoster)
				{
					p.InitializePlayerStats(leagueName, awayTeam.Name);
				}

				foreach (Player p in homeTeam.onCamera)
					p.LeagueTeamSeasonStats[leagueName][homeTeam.Name].matchesPlayed++;
				foreach (Player p in homeTeam.offCamera)
					p.LeagueTeamSeasonStats[leagueName][homeTeam.Name].matchesPlayed++;
				foreach (Player p in awayTeam.onCamera)
					p.LeagueTeamSeasonStats[leagueName][awayTeam.Name].matchesPlayed++;
				foreach (Player p in awayTeam.offCamera)
					p.LeagueTeamSeasonStats[leagueName][awayTeam.Name].matchesPlayed++;

				this.homeTeam.goalKeeper.LeagueTeamSeasonStats[leagueName][homeTeam.Name].matchesPlayed++;
				this.awayTeam.goalKeeper.LeagueTeamSeasonStats[leagueName][awayTeam.Name].matchesPlayed++;
			}

			this.yellowCardRegs = yellowCardRegs;
			this.watchingGame = watchingGame;
			if (!this.watchingGame)
			{
				this.PlayGame();
			}
			else
			{
				m_SimulateEverything = new Thread(PlayGame);
				m_SimulateEverything.Start();
			}
		}

		public void Attach(iObserver o)
		{
			observers.Add(o);
		}

		public void Detach(iObserver o)
		{
			observers.Add(o);
		}

		public void ShowMessage(string eventText)
		{
			LoggingStuff.LogTheEvent(eventText);
			if (watchingGame)
			{
				NotifyAnnouncerChange(eventText);
				Thread.Sleep(gameSpeed);
			}
		}

		public void NotifyAnnouncerChange(string eventText)
		{
			if (watchingGame)
			{
				foreach (iObserver o in observers)
				{
					o.UpdateAnnouncer(eventText);
				}
			}
		}

		public void NotifyOnCamera()
		{
			if (watchingGame)
			{
				foreach (iObserver o in observers)
				{
					o.UpdateOnCamera();
				}
			}
		}

		public void NotifyTimeTicked(string minutesPassed)
		{
			if (watchingGame)
			{
				foreach (iObserver o in observers)
				{
					o.UpdateTimeTicked(minutesPassed);
				}
			}
		}

		public void NotifyHalfChanged()
		{
			if (watchingGame)
			{
				foreach (iObserver o in observers)
				{
					o.UpdateHalf();
				}
			}
		}

		public void NotifyScoreChange()
		{
			if (watchingGame)
			{
				foreach (iObserver o in observers)
				{
					o.UpdateScoreboard();
				}
			}
		}
		
		public void NotifyGameIsOver()
		{
			if (watchingGame)
			{
				foreach (iObserver o in observers)
				{
					o.UpdateGameIsOver();
				}
			}
		}

		// TO DO: Rare results
		public Game(Team homeTeam, Team awayTeam, int minutesPerHalf, bool silvergoalOT, bool traditionalOT, bool penaltyKicksOnlyOT,
			int numberOfsubs, DateTime gameTime, 
			string leagueName, int FixtureOrRoundNumber, string gameName,
			bool forceSub = true, bool cup = false, bool isPlayoffOrFriendly = false, bool isEuroKnockoutRound = false)
		{
			this.leagueName = leagueName;
			this.FixtureOrRoundNumber = FixtureOrRoundNumber;
			this.gameName = gameName + " " + FixtureOrRoundNumber.ToString();

			this.homeTeam = homeTeam;
			this.awayTeam = awayTeam;
			this.minutesPerHalf = minutesPerHalf;
			this.minutesPerHalfPlusInjury = minutesPerHalf;
			this.referee = new Referee("Enigmatic", "Nobody");
			this.silvergoalOT = silvergoalOT;
			this.traditionalOT = traditionalOT;
			this.penaltyKicksOnlyOT = penaltyKicksOnlyOT;
			this.forceSub = forceSub;
			this.numberOfHomeSubsLeft = numberOfsubs;
			this.numberOfAwaySubsLeft = numberOfsubs;
			weather = Enums.WeatherIssue.None;
			homeGameStats = new GameStats();
			awayGameStats = new GameStats();
			this.gameDate = gameTime;
			this.FACupGame = cup;
			this.isPlayoffOrFriendly = isPlayoffOrFriendly;
			this.isEuroKnockoutRound = isEuroKnockoutRound;
		}

		//private void SanityCheck()
		//{
		//    homeTeam.TotalTeamGameStats();
		//    awayTeam.TotalTeamGameStats();
		//    LoggingStuff.LogTheEvent("Home shots: " + homeTeam.gameStats.shotsOnGoal.ToString());
		//    LoggingStuff.LogTheEvent("Home goals: " + homeTeam.gameStats.goals.ToString());
		//    LoggingStuff.LogTheEvent("Home PKs: " + homeTeam.gameStats.penaltyKickAttempts.ToString());
		//    LoggingStuff.LogTheEvent("Total shots minus goals: " + (homeTeam.gameStats.shotsOnGoal - homeTeam.gameStats.goals - homeTeam.gameStats.penaltyKickAttempts).ToString());
		//    LoggingStuff.LogTheEvent("Away saves: " + awayTeam.gameStats.Saves.ToString());

		//    LoggingStuff.LogTheEvent("Away shots: " + awayTeam.gameStats.shotsOnGoal.ToString());
		//    LoggingStuff.LogTheEvent("Away goals: " + awayTeam.gameStats.goals.ToString());
		//    LoggingStuff.LogTheEvent("Away PKs: " + awayTeam.gameStats.penaltyKickAttempts.ToString());
		//    LoggingStuff.LogTheEvent("Total shots minus goals: " + (awayTeam.gameStats.shotsOnGoal - awayTeam.gameStats.goals - awayTeam.gameStats.penaltyKickAttempts).ToString());
		//    LoggingStuff.LogTheEvent("Home saves: " + homeTeam.gameStats.Saves.ToString());
		//    if ((homeTeam.gameStats.shotsOnGoal - homeTeam.gameStats.goals - homeTeam.gameStats.penaltyKickAttempts) > awayTeam.gameStats.Saves)
		//    {
		//        foreach (Player p in homeTeam.onCamera)
		//        {
		//            ShowMessage(p.fullName);
		//        }
		//        foreach (Player p in homeTeam.offCamera)
		//        {
		//            ShowMessage(p.fullName);
		//        }
		//        foreach (Player p in homeTeam.bench)
		//        {
		//            ShowMessage(p.fullName);
		//        }
		//        LoggingStuff.LogTheEvent("TO DO: Failure, shotsontarget - goals is greather than saves. Happened at minute: " + this.DetermineCurrentTime() + ".");
		//        LoggingStuff.LogTheRunningSummary();
		//    }
		//    else if ((awayTeam.gameStats.shotsOnGoal - awayTeam.gameStats.goals - awayTeam.gameStats.penaltyKickAttempts) > homeTeam.gameStats.Saves)
		//    {
		//        foreach (Player p in awayTeam.onCamera)
		//        {
		//            ShowMessage(p.fullName);
		//        }
		//        foreach (Player p in awayTeam.offCamera)
		//        {
		//            ShowMessage(p.fullName);
		//        }
		//        foreach (Player p in awayTeam.bench)
		//        {
		//            ShowMessage(p.fullName);
		//        }
		//        LoggingStuff.LogTheEvent("TO DO: Failure, shotsontarget - goals is greather than saves. Happened at minute: " + this.DetermineCurrentTime() + ".");
		//        LoggingStuff.LogTheRunningSummary();
		//    }

		//    homeTeam.gameStats = new GameStats();
		//    awayTeam.gameStats = new GameStats();
		//}

		public void PlayGame()
		{
			this.homeTeam.ResetStats();
			this.awayTeam.ResetStats();

			//this.watchingGame = watchGame;
			LoggingStuff.CreateNewLog();
			//coin flip to determine who has ball first
			int decider = Dice.Instance.d6.Roll();
			if (decider <= 3)
			{
				AttackingTeam = homeTeam;
				DefendingTeam = awayTeam;
			}
			else
			{
				homeTeamHadBallFirst = false;
				AttackingTeam = awayTeam;
				DefendingTeam = homeTeam;
			}

			//this.homeTeam.ConfigureRoster();
			//this.awayTeam.ConfigureRoster();

			while (gameIsGoing)
			{
				this.changedPossessionAtLeastOnceOrMidfieldBattle = false;
				//ShowMessage("Minutes elapsed: " + this.minutesPlayed, true);

				if (kickOff)
				{
					ShowMessage(AttackingTeam.Name + " kicks off.");
					kickOff = false;
					timeWhenSeizedPossession = this.minutesPlayed;
				}

				//ShowCurrentRosters();

				this.attackingTeamIsHomeTeam = AttackingTeam == homeTeam;

				this.specialSaveRequired = false;

				this.DeterminePitchAction();

				//ShowScores();

				DetermineTimeIssues();
			}

			NotifyGameIsOver();

			CheckCleanSheets();

			this.homeTeam.StoreGameStats();
			this.awayTeam.StoreGameStats();
			if (!this.isPlayoffOrFriendly)
			{
				this.homeTeam.StoreSeriesOfStats(leagueName);
				this.awayTeam.StoreSeriesOfStats(leagueName);
			}

			StoreStatsForGame(homeGameStats, this.homeTeam);
			StoreStatsForGame(awayGameStats, this.awayTeam);

			GamePlayed = true;

			//LoggingStuff.LogTheRunningSummary();

			LoggingStuff.DisposeLog();

			if (m_SimulateEverything != null)
				m_SimulateEverything.Abort();
		}

		private void StoreStatsForGame(GameStats gs, Team team)
		{
			gs.assists += team.gameStats.assists;
			gs.fouls += team.gameStats.fouls;
			gs.goals += team.gameStats.goals;
			gs.goalsConceded += team.gameStats.goalsConceded;
			gs.penaltyKickAttempts += team.gameStats.penaltyKickAttempts;
			gs.penaltyKickGoals += team.gameStats.penaltyKickGoals;
			gs.redCards += team.gameStats.redCards;
			gs.Saves += team.gameStats.Saves;
			gs.shotsOnGoal += team.gameStats.shotsOnGoal;
			gs.shotsTotal += team.gameStats.shotsTotal;
			gs.yellowCards += team.gameStats.yellowCards;
			gs.minutesWithBall += team.gameStats.minutesWithBall;
		}

		private String DetermineCurrentTime()
		{
			int secondHalfTotalTime = this.minutesPerHalf * 2;
			int extraTimeHalfOne = secondHalfTotalTime + 15;
			int extraTimeHalfTwo = extraTimeHalfOne + 15;
			int maxMinutes = this.minutesPlayed;
			if (halfNumber == 1)
				maxMinutes = this.minutesPerHalf;
			if (halfNumber == 2)
				maxMinutes = secondHalfTotalTime;
			if (halfNumber == 3)
				maxMinutes = extraTimeHalfOne;
			if (halfNumber == 4)
				maxMinutes = extraTimeHalfTwo;
			if (halfNumber > 4)
				return "PK";

			if (this.minutesPlayed > maxMinutes)
				return maxMinutes.ToString() + "+" + (this.minutesPlayed - maxMinutes);
			else
				return this.minutesPlayed.ToString();
		}

		private void ClockTick(int timePassed)
		{
			this.minutesPlayed += timePassed;
			NotifyTimeTicked(DetermineCurrentTime());
		}

		private void UpdateScore(Player assister, Player shooter, Player goalKeeper, bool fromPK = false)
		{
			Dictionary<Player, List<string>> goalTrackerTraditional = new Dictionary<Player, List<string>>();
			string goalScoredSummary = "";

			if (AttackingTeam == homeTeam)
			{
				homeScore += 1;
				goalTrackerTraditional = homePlayerGoalTrackerTraditional;
			}
			else
			{
				awayScore += 1;
				goalTrackerTraditional = awayPlayerGoalTrackerTraditional;
			}

			string minutesToAdd = this.DetermineCurrentTime();
			List<string> minutes = new List<string>();

			if (shooter == null)
			{
				shooter = new Player(-1, "Own Goal");
				goalScoredSummary = "In minute number: " + minutesToAdd + ", " + this.DefendingTeam.Name + " gave up an own goal";
			}
			else
			{
				if (goalTrackerTraditional.ContainsKey(shooter))
				{
					minutes = goalTrackerTraditional[shooter];
				}
				AddPlayerRating(goalKeeper, -0.5);
				shooter.gameStats.goals++;
				this.DefendingTeam.goalKeeper.gameStats.goalsConceded++;
				AddPlayerRating(shooter, -1, false, true);
				goalScoredSummary = "In minute number: " + minutesToAdd + ", " + shooter.fullName + " on team " + this.AttackingTeam.Name + " scored";
			}

			minutes.Add(minutesToAdd + "'");
			goalTrackerTraditional[shooter] = minutes;
			if (fromPK) goalScoredSummary += " (from PK)";

			if (assister != null)
			{
				AddPlayerRating(assister, -1, true, false);
				assister.gameStats.assists++;
				goalScoredSummary += " with the assist from " + assister.fullName;
			}
			goalScoredSummary += ".";

			gameSummary.Add(goalScoredSummary);

			NotifyScoreChange();

			ShowMessage("With that goal, the score is now...");
			ShowScores();

			referee.ResetAttitude();

			ballIsLive = false;

			kickOff = true;
		}

		public void ShowScores()
		{
			ShowMessage(homeTeam.Name + ", " + GetHomeScore() + " - " + awayTeam.Name + ", " + GetAwayScore());
			//ShowGoalTraditionalSummary();
		}

		//public void ShowGoalTraditionalSummary()
		//{
		//    //if (homePlayerGoalTrackerTraditional.Count > 0) ShowMessage(homeTeam.Name + " goals:", true);
		//    foreach (KeyValuePair<Player, List<string>> entry in homePlayerGoalTrackerTraditional)
		//    {
		//        List<string> minutes = homePlayerGoalTrackerTraditional[entry.Key];
		//        string goalRecord = entry.Key.fullName + " ";
		//        foreach (string minute in minutes)
		//            goalRecord += minute.ToString() + ", ";
		//        goalRecord = goalRecord.Remove(goalRecord.Length - 2); ;
		//        //ShowMessage(goalRecord, true);
		//    }
		//    //if (awayPlayerGoalTrackerTraditional.Count > 0) ShowMessage(awayTeam.Name + " goals:", true);
		//    foreach (KeyValuePair<Player, List<string>> entry in awayPlayerGoalTrackerTraditional)
		//    {
		//        List<string> minutes = awayPlayerGoalTrackerTraditional[entry.Key];
		//        string goalRecord = entry.Key.fullName + " ";
		//        foreach (string minute in minutes)
		//            goalRecord += minute.ToString() + ", ";
		//        goalRecord = goalRecord.Remove(goalRecord.Length - 2); ;
		//        //ShowMessage(goalRecord, true);
		//    }
		//}

		public void DetermineTimeIssues()
		{
			int greendie = Dice.Instance.d6.Roll();

			int secondHalfTotalTime = this.minutesPerHalf * 2;
			int extraTimeHalfOne = secondHalfTotalTime + 15;
			int extraTimeHalfTwo = extraTimeHalfOne + 15;
			int timeToCompare = 0;
			if (halfNumber == 1)
				timeToCompare = this.minutesPerHalf;
			if (halfNumber == 2)
				timeToCompare = secondHalfTotalTime;
			if (halfNumber == 3)
				timeToCompare = extraTimeHalfOne;
			if (halfNumber == 4)
				timeToCompare = extraTimeHalfTwo;

			if ((this.minutesPlayed < this.minutesPerHalfPlusInjury) && (this.minutesPerHalfPlusInjury == timeToCompare))
			{
				if ((this.minutesPlayed + greendie) >= this.minutesPerHalfPlusInjury)
				{
					ShowMessage("About " + (greendie + minInjuryTime).ToString() + " minutes of stoppage time will be added by my estimation.");
					this.minutesPerHalfPlusInjury += greendie + minInjuryTime;
				}
			}

			if ((minutesPlayed > minutesPerHalfPlusInjury) && changedPossessionAtLeastOnceOrMidfieldBattle)
			{
				bool determineWinner = false;

				referee.ResetAttitude();

				if (halfNumber == 1)
					ShowMessage("That'll end the first half!");
				if (halfNumber == 2)
					ShowMessage("That's the end of regulation!");
				if (halfNumber == 3)
					ShowMessage("That's the end of the first half of extra time!");
				if (halfNumber == 4)
					ShowMessage("That's the end of the second half of extra time!");
				if (halfNumber >= 2)
				{
					if (TotalHomeScore() != TotalAwayScore() && 
						((halfNumber == 2 || halfNumber == 4) || (halfNumber == 3 && silvergoalOT)))
					{
						determineWinner = true;
						if (silvergoalOT && halfNumber == 3)
						{
							ShowMessage("And due to the unpopular silver goal rules, this game is over!");
							determineWinner = true;
						}
					}

					if (!determineWinner)
					{
						if (!silvergoalOT && !traditionalOT && !penaltyKicksOnlyOT)
						{
							ShowMessage("It's a draw!");
							GameResult = RecordGameResult(homeTeam, GetHomeScore(), awayTeam, GetAwayScore(), false);
							gameIsGoing = false;
						}
						else
						{
							if (silvergoalOT)
							{
								if (halfNumber == 2)
								{
									ShowMessage("We are headed to extra time! The unpopular silver goal rules apply.");
								}
								if (halfNumber == 3)
								{
									ShowMessage("With no winner, we are headed to the second half of extra time just like the more popular rules!");
								}
								if (halfNumber == 4)
								{
									if (TotalHomeScore() == TotalAwayScore())
									{
										ShowMessage("The game cannot be decided after extra time, so we're going to penalty kicks!");
										PenaltyKickOT();
									}
								}
							}
							if (traditionalOT)
							{
								if (halfNumber == 2)
								{
									ShowMessage("We are headed to extra time! Two halves of 15 to hopefully determine a winner!");
								}
								if (halfNumber == 3)
								{
									ShowMessage("We are headed to the second half of extra time! Another half of 15 to hopefully determine a winner!");
								}
								if (halfNumber == 4)
								{
									if (TotalHomeScore() == TotalAwayScore())
									{
										ShowMessage("The game cannot be decided after extra time, so we're going to penalty kicks!");
										PenaltyKickOT();
									}
								}
							}
							if (penaltyKicksOnlyOT)
							{
								ShowMessage("The game cannot be decided in regulation, so we're going to penalty kicks!");
								halfNumber = 4;
								PenaltyKickOT();
							}
						}
					}

					if (determineWinner)
					{
						if (TotalHomeScore() > TotalAwayScore())
						{
							ShowMessage(homeTeam.Name + " wins!");
							GameResult = RecordGameResult(homeTeam, GetHomeScore(), awayTeam, GetAwayScore(), true);
						}
						else if (TotalAwayScore() > TotalHomeScore())
						{
							ShowMessage(awayTeam.Name + " wins!");
							GameResult = RecordGameResult(awayTeam, GetHomeScore(), homeTeam, GetAwayScore(), true);
						}

						//ShowPlayerRatings();
						//ShowPlayerScoredSummary();

						gameIsGoing = false;
					}
				}
				if (gameIsGoing)
				{
					minInjuryTime = 0;
					halfNumber++;
				}
				else
				{
				}

				NotifyHalfChanged();

				if (halfNumber == 2)
				{
					if (homeTeamHadBallFirst)
					{
						AttackingTeam = awayTeam;
						DefendingTeam = homeTeam;
					}
					else
					{
						AttackingTeam = homeTeam;
						DefendingTeam = awayTeam;
					}
					kickOff = true;
					minutesPerHalfPlusInjury = secondHalfTotalTime;
					minutesPlayed = minutesPerHalf;
				}
				if (halfNumber == 3)
				{
					if (homeTeamHadBallFirst)
					{
						AttackingTeam = homeTeam;
						DefendingTeam = awayTeam;
					}
					else
					{
						AttackingTeam = awayTeam;
						DefendingTeam = homeTeam;
					}
					kickOff = true;
					minutesPerHalfPlusInjury = extraTimeHalfOne;
					minutesPlayed = secondHalfTotalTime;
				}
				if (halfNumber == 4)
				{
					if (homeTeamHadBallFirst)
					{
						AttackingTeam = awayTeam;
						DefendingTeam = homeTeam;
					}
					else
					{
						AttackingTeam = homeTeam;
						DefendingTeam = awayTeam;
					}
					kickOff = true;
					minutesPerHalfPlusInjury = extraTimeHalfTwo;
					minutesPlayed = extraTimeHalfOne;
				}
				if (!gameIsGoing)
				{
					if (timeWhenSeizedPossession < minutesPlayed)
						this.AttackingTeam.gameStats.minutesWithBall += (minutesPlayed - timeWhenSeizedPossession);
					if (halfNumber == 2)
						this.minutesPlayed = secondHalfTotalTime;
					if (halfNumber == 3)
						this.minutesPlayed = extraTimeHalfOne;
					if (halfNumber == 4)
						this.minutesPlayed = extraTimeHalfTwo;
				}
				NotifyTimeTicked(DetermineCurrentTime());
			}
		}

		private string RecordGameResult(Team winningTeam, string homeScore, Team losingTeam, string awayScore, bool wasAWinner)
		{
			string homeRank = "";
			string awayRank = "";
			if (Season.Instance.teamRankInLeague.ContainsKey(leagueName))
			{
				homeRank = Season.Instance.teamRankInLeague[leagueName][homeTeam].ToString() + " ";
				awayRank = Season.Instance.teamRankInLeague[leagueName][awayTeam].ToString() + " ";
			}

			string GameResult = homeRank + homeTeam.Name + ": " + homeScore + " - " + awayRank + awayTeam.Name + ": " + awayScore;
			winningTeam.CreateSeasonStats(leagueName);
			losingTeam.CreateSeasonStats(leagueName);
			if (wasAWinner)
			{
				this.Winner = winningTeam;
				this.Loser = losingTeam;
				if (!isPlayoffOrFriendly && !isEuroKnockoutRound)
				{
					this.Winner.seasonStats[leagueName].wins++;
					this.Loser.seasonStats[leagueName].losses++;
				}
			}
			else
			{
				if (!isPlayoffOrFriendly && !isEuroKnockoutRound)
				{
					winningTeam.seasonStats[leagueName].draws++;
					losingTeam.seasonStats[leagueName].draws++;
				}
			}
			return GameResult;
		}

		public void ShowCurrentRosters()
		{
			ShowMessage("Current rosters:");
			foreach (Player p in homeTeam.onCamera)
			{
				ShowMessage(p.fullName + ", " + p.Position);
			}
			foreach (Player p in awayTeam.onCamera)
			{
				ShowMessage(p.fullName + ", " + p.Position);
			}
		}

		public void changePlayer(Team team, int indexOfSwap)
		{
			//LogMessage("Change Player");
			indexOfSwap -= 1;
			Player playerToSwitch = team.onCamera[indexOfSwap];
			team.onCamera[indexOfSwap] = team.offCamera[indexOfSwap];
			team.offCamera[indexOfSwap] = playerToSwitch;
		}

		public void DeterminePitchAction()
		{
			//LogMessage("Determine Pitch Action");
			int greendie = Dice.Instance.d6.Roll();
			int whitedie = Dice.Instance.d6.Roll();
			int blackdie = Dice.Instance.d6.Roll();

			Enums.Characteristic characteristic = (Enums.Characteristic)greendie;
			int diceSum = blackdie + whitedie;
			if (diceSum == 2 || diceSum == 4 || diceSum == 7 || diceSum == 6 || diceSum == 12)
			{
				ballIsLive = true;
			}

			if (diceSum == 2)
			{
				ShowMessage("Great through ball and " + AttackingTeam.Name + " is on the attack!");
				Attack(whitedie, blackdie, false);
			}
			if (diceSum == 3)
				HighlightReel("M");
			if (diceSum == 4)
				HomeFieldAdvantage(whitedie, blackdie);
			if (diceSum == 5)
				RefereeDecision("M");
			if (diceSum == 6)
				TakeOn(characteristic, whitedie, blackdie);
			if (diceSum == 7)
			{
				MidfieldBattleOrBuildup(characteristic, greendie, whitedie, blackdie, false);
			}
			if (diceSum == 8)
				MidfieldBattleOrBuildup(characteristic, greendie, whitedie, blackdie, true);
			if (diceSum == 9)
				SidelineBattle(greendie, whitedie, blackdie);
			if (diceSum == 10)
			{
				bool doTackle = true;
				if (minutesPlayed >= 60)
				{
					doTackle = false;
					bool substitute = false;
					if ((AttackingTeam == homeTeam && numberOfHomeSubsLeft > 0) ||
						(AttackingTeam != homeTeam && numberOfAwaySubsLeft > 0))
					{
						substitute = true;
					}
					if (substitute)
					{
						if (ballIsLive)
						{
							// fakeout message
							int defenderIndex = Dice.Instance.d6.Roll();
							if (defenderIndex == 6) defenderIndex = 5;
							Player tackler = FindPlayerWhenNoMatter(this.DefendingTeam.onCamera[defenderIndex - 1], this.DefendingTeam.onCamera, (defenderIndex - 1));
							ShowMessage(tackler.fullName + " tugs at the attacker's jersey.");
							ShowMessage("Certainly a foul. Looked like it warranted a yellow in my book but the referee apparently disagrees.");
							ShowMessage("A free kick will be granted but not anywhere near enough to the opposition goal to be dangerous. He'll just pass it from here I'm sure.");
							tackler.gameStats.IncreaseFoulCount();
						}
						SubstitutePlayer(greendie - 1, this.AttackingTeam);
					}
					else
					{
						ballIsLive = true;
						doTackle = true;
					}
				}
				else
				{
					doTackle = true;
				}
				if (doTackle)
					Tackle();
			}
			if (diceSum == 11)
			{
				Player shooterWhoLeadsToCorner = this.FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[0], this.AttackingTeam.onCamera, 0);
				ShowMessage("After a series of passes, " + shooterWhoLeadsToCorner.fullName + " collects the ball just outside the box!");
				ShowMessage(shooterWhoLeadsToCorner.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
				ShowMessage(shooterWhoLeadsToCorner.fullName + " takes a shot, but it is deflected off a defender.");
				ShowMessage("It rolls past the end line which will lead to a corner.");
				if (minutesPlayed >= 60 && forceSub)
				{
					if (numberOfAwaySubsLeft > 0)
					{
						SubstitutePlayer(greendie - 1, awayTeam);
					}
					if (numberOfHomeSubsLeft > 0)
					{
						SubstitutePlayer(greendie - 1, homeTeam);
					}
				}
				CornerKick();
			}
			if (diceSum == 12)
				CounterAttack(whitedie, blackdie);

			if (minutesPlayed >= 80 && forceSub)
			{
				bool submade = false;
				if (awaySubs.Count == 0 || homeSubs.Count == 0)
				{
					if (ballIsLive)
					{
						// fakeout message
						int defenderIndex = Dice.Instance.d6.Roll();
						if (defenderIndex == 6) defenderIndex = 5;
						Player tackler = FindPlayerWhenNoMatter(this.DefendingTeam.onCamera[defenderIndex - 1], this.DefendingTeam.onCamera, (defenderIndex - 1));
						ShowMessage(tackler.fullName + " tugs at the attacker's jersey.");
						ShowMessage("Certainly a foul. Looked like it warranted a yellow in my book but the referee apparently disagrees.");
						ShowMessage("A free kick will be granted but not anywhere near enough to the opposition goal to be dangerous. He'll just pass it from here I'm sure.");
						tackler.gameStats.IncreaseFoulCount();
					}
				}

				if (awaySubs.Count == 0)
				{
					SubstitutePlayer(greendie - 1, awayTeam);
					submade = true;
				}
				if (homeSubs.Count == 0)
				{
					SubstitutePlayer(greendie - 1, homeTeam);
					submade = true;
				}
				if (submade)
					return;
			}
		}

		// To Do: figure out a better way to attack
		public void Attack(int whitedie, int blackdie, bool noSquaresOverride)
		{
			ballIsLive = true;
			//LogMessage("Attack");
			this.ChangePlayers();
			ClockTick(1);
			double offenseSkill = 0;
			double defenseSkill = 0;

			double offensiveCount = 0;
			foreach (Player p in this.AttackingTeam.onCamera)
			{
				if (p.Position == Enums.Positions.Forward || p.Position == Enums.Positions.Striker ||
					p.Position == Enums.Positions.LeftWingForward || p.Position == Enums.Positions.RightWingForward ||
					p.Position == Enums.Positions.CentralAttackingMidfielder)
				{
					offensiveCount++;
					offenseSkill += (p.shooting + p.passing) / 2.0;
				}
			}

			double defenderCount = 0;
			foreach (Player p in this.DefendingTeam.onCamera)
			{
				if (p.Position == Enums.Positions.Defender || p.Position == Enums.Positions.CenterBack ||
					p.Position == Enums.Positions.LeftBack || p.Position == Enums.Positions.RightBack ||
					p.Position == Enums.Positions.CentralDefendingMidfielder)
				{
					defenderCount++;
					defenseSkill += (p.defending + p.intercept) / 2.0;
				}
			}

			if (noSquaresOverride)
				defenseSkill = 0;

			offenseSkill = offenseSkill / offensiveCount;
			defenseSkill = defenseSkill / defenderCount;

			if (OnPitchCount(this.AttackingTeam) < OnPitchCount(this.DefendingTeam))
			{
				if (halfNumber == 1)
					offenseSkill /= 2;
				else
					offenseSkill /= 4;
			}
			else if (OnPitchCount(this.DefendingTeam) < OnPitchCount(this.AttackingTeam))
			{
				if (halfNumber == 1)
					defenseSkill /= 2;
				else
					defenseSkill /= 4;
			}

			blackdie = Dice.Instance.d100.Roll();
			whitedie = Dice.Instance.d100.Roll();

			int shotAttemptVal = shotAttemptValue;

			if ((defenseSkill - offenseSkill) > 20)
				shotAttemptVal -= 25;
			else if ((defenseSkill - offenseSkill) > 15)
				shotAttemptVal -= 20;
			else if ((defenseSkill - offenseSkill) > 10)
				shotAttemptVal -= 15;
			else if ((defenseSkill - offenseSkill) > 5)
				shotAttemptVal -= 10;
			else if ((defenseSkill - offenseSkill) > 0)
				shotAttemptVal -= 5;

			double offenseSucceeds = shotAttemptVal + offenseSkill - defenseSkill;

			if (blackdie <= offenseSucceeds)
			{
				OnTarget();
			}
			else
			{
				DefendedShot();
			}
		}

		public void DefendedShot()
		{
			//LogMessage("Defended Shot");

			int whiteDieVal = Dice.Instance.d6.Roll();
			int blackDieVal = Dice.Instance.d6.Roll();
			int sum = whiteDieVal + blackDieVal;

			int defenderIndex = -1;
			int attackingIndex = -1;
			int shotTaker = 0;

			if (sum == 2)
				defenderIndex = 0;
			if (sum == 3)
				defenderIndex = 1;
			if (sum == 4)
			{
				defenderIndex = 2;
				shotTaker = 4;
			}
			if (sum == 5)
			{
				defenderIndex = 3;
				shotTaker = 3;
			}
			if (sum == 6)
			{
				defenderIndex = 4;
				shotTaker = 0;
			}
			if (sum == 7)
			{
				Player shooterWhoLeadsToCorner = this.AttackingTeam.onCamera[0];
				if (PlayerIsOffField(shooterWhoLeadsToCorner))
				{
					HandleMissingPlayerFlavorTextAndChangePossession();
				}
				else
				{
					ShowMessage("After a series of passes, " + shooterWhoLeadsToCorner.fullName + " collects the ball just outside the box!");
					ShowMessage(shooterWhoLeadsToCorner.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
					ShowMessage(shooterWhoLeadsToCorner.fullName + " takes a shot, but it is deflected off a defender.");
					ShowMessage("It rolls past the end line which will lead to a corner.");
					CornerKick();
				}
				return;
			}
			if (sum == 8)
			{
				shotTaker = 1;
				attackingIndex = 0;
			}
			if (sum == 9)
			{
				shotTaker = 2;
				attackingIndex = 1;
			}
			if (sum == 10)
			{
				shotTaker = 0;
				attackingIndex = 2;
			}
			if (sum == 11)
			{
				shotTaker = 3;
			}
			if (sum == 12)
				shotTaker = 4;

			int greenDieVal = Dice.Instance.d6.Roll();
			if (greenDieVal == 6) greenDieVal--;

			Player assister = this.AttackingTeam.onCamera[greenDieVal - 1];
			if (PlayerIsOffField(assister))
			{
				assister = null;
			}
			Player shooter = this.AttackingTeam.onCamera[shotTaker];
			if (PlayerIsOffField(shooter))
			{
				HandleMissingPlayerFlavorTextAndChangePossession();
				return;
			}
			if (assister != null)
			{
				if (shooter != assister)
				{
					ShowMessage(assister.fullName + " " + "makes a nice pass!");
				}
				else
				{
					assister = null;
					ShowMessage(shooter.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
				}
			}

			ShowMessage(shooter.fullName + " takes a shot, but it is deflected off a defender.");
			bool changePossession = false;

			// try a rebound
			if (defenderIndex > -1)
			{
				Player defender = this.DefendingTeam.onCamera[defenderIndex];
				int numberOfCircles = defender.acceleration;
				// TO DO: roll to see whether the player collects the ball. d100, if less than acceleration, ball is collected.
				if (PlayerIsOffField(defender))
				{
					numberOfCircles = 0;
				}
				if (numberOfCircles > 0)
				{
					ShowMessage(this.DefendingTeam.onCamera[defenderIndex].fullName + " collects the ball and clears it to a teammate.");
					changePossession = true;
				}
				else
				{
					if (PlayerIsOffField(defender))
					{
						ShowMessage("The ball bounces off a defender but the ball remains in play! The attacking team catches right up to it and maintains possession");
					}
					else
					{
						ShowMessage(this.DefendingTeam.onCamera[defenderIndex].fullName + " collects the ball and clears it from the box. But the attacking team catches right up to it and maintains possession.");
					}
				}
			}
			if (attackingIndex > -1)
			{
				Player attacker = this.AttackingTeam.onCamera[attackingIndex];
				int numberOfCircles = attacker.acceleration;
				// TO DO: roll to see whether the player collects the ball. d100, if less than acceleration, ball is collected.
				if (PlayerIsOffField(attacker))
				{
					numberOfCircles = 0;
				}
				if (numberOfCircles > 0)
				{
					ShowMessage(this.AttackingTeam.onCamera[attackingIndex].fullName + " collects the attempted clearance so his team maintains possession.");
				}
				else
				{
					if (!PlayerIsOffField(attacker))
					{
						ShowMessage(this.AttackingTeam.onCamera[attackingIndex].fullName + " collects the attempted clearance but then loses it.");
					}
					else
					{
						ShowMessage("The ball bounces off a defender but the ball remains in play! The defending team catches right up to it and claims possession!");
					}
					changePossession = true;
				}
			}

			if (changePossession)
				ChangePossession();
		}

		// TO DO: Proper on target
		public void OnTarget(int shooterIndex = -1, bool specialSaveRequiredOverride = false, bool useReaction = false, int offsideValue = 6)
		{
			LoggingStuff.LogTheEvent("On Target");
			int greenDieVal = Dice.Instance.d6.Roll();
			int whiteDieVal = Dice.Instance.d6.Roll();
			int blackDieVal = Dice.Instance.d6.Roll();
			int assistValue = 0;
			int shotValue = 0;

			ChangePlayers();

			Player assister = null;

			if (greenDieVal < 6)
			{
				assister = this.AttackingTeam.onCamera[greenDieVal - 1];
				if (PlayerIsOffField(assister))
				{
					assister = null;
				}
				else
				{
					if (!assister.isInjured)
						assistValue = assister.passing;
				}
			}
			bool specialSaveRequired = false;
			int shootIndex = shooterIndex;
			if (shootIndex == -1)
			{
				shootIndex = DetermineShooter(whiteDieVal, blackDieVal, ref specialSaveRequired);
			}
			if (specialSaveRequiredOverride) specialSaveRequired = true;
			Player shooter = this.AttackingTeam.onCamera[shootIndex];
			if (PlayerIsOffField(shooter))
			{
				HandleMissingPlayerFlavorTextAndChangePossession();
				return;
			}
			double totalShotPower = 0;
			if (shooter.shooting > 0)
			{
				shotValue = shooter.shooting;
				totalShotPower += shotValue;
			}
			if (shooter.isInjured)
			{
				totalShotPower = 0;
				shotValue = -2;
			}
			//if (weather == Enums.WeatherIssue.Light)
			//    if (totalShotPower > 3) totalShotPower = 3;
			//if (weather == Enums.WeatherIssue.Moderate)
			//    if (totalShotPower > 2) totalShotPower = 2;
			//if (weather == Enums.WeatherIssue.Heavy)
			//    if (totalShotPower > 1) totalShotPower = 1;

			if (!useReaction)
			{
				if (assister != null)
				{
					if (shooter != assister)
					{
						ShowMessage(assister.fullName + " " + "makes a nice pass!");
						double totalAssistValue = ((assister.passing - 60) / 10.0);
						if (totalAssistValue > 0)
							totalShotPower += totalAssistValue;
					}
					else
					{
						assister = null;
						ShowMessage(shooter.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
					}
				}
			}
			else
			{
				//because it's a rebound.
				assister = null;
				ShowMessage(shooter.fullName + " with the rebound!");
				// No modifiers in the base set of rules but I think rebounds should score more often than they do. Hence,
				// the change below.
				totalShotPower += 1;
			}
			ShowMessage(shooter.fullName + " takes a shot!");

			int goalKeeperSkill = this.DefendingTeam.goalKeeper.goalKeepingPositioning;
			if (useReaction)
				goalKeeperSkill = this.DefendingTeam.goalKeeper.goalKeepingReflexes - 10;
			blackDieVal = Dice.Instance.d100.Roll();
			whiteDieVal = Dice.Instance.d100.Roll();

			double successValue = 65;
			int maxGKSuccess = 99;
			successValue += (goalKeeperSkill - totalShotPower);
			if (successValue > maxGKSuccess)
				successValue = maxGKSuccess;
			//if ((blackDieVal > totalShotPower) || (whiteDieVal < goalKeeperSkill - 15))
			if (blackDieVal < successValue)
			{
				shooter.gameStats.IncreaseShotOnGoal();
				Saved();
				//SanityCheck();
			}
			else
			{
				GoalChance(greenDieVal, assister, shooter, 6, specialSaveRequired);
				//SanityCheck();
			}
		}

		public int DetermineShooter(int whiteDieVal, int blackDieVal, ref bool specialSaveRequired)
		{
			//LogMessage("DetermineShooter");
			specialSaveRequired = false;
			int sum = whiteDieVal + blackDieVal;
			int shooterIndex = 1;
			if (sum == 2)
			{
				specialSaveRequired = true;
				shooterIndex = 5;
			}
			if (sum == 3)
			{
				specialSaveRequired = true;
				shooterIndex = 4;
			}
			if (sum == 4)
			{
				specialSaveRequired = true;
				shooterIndex = 5;
			}
			if (sum == 5)
			{
				if (Dice.Instance.d6.Roll() > 3)
				{
					shooterIndex = 5;
				}
				else
				{
					shooterIndex = 4;
				}
			}
			if (sum == 6)
			{
				shooterIndex = 3;
			}
			if (sum == 7)
				shooterIndex = 1;
			if (sum == 8)
				shooterIndex = 2;
			if (sum == 9)
				shooterIndex = 1;
			if (sum == 10)
			{
				specialSaveRequired = true;
				shooterIndex = 2;
			}
			if (sum == 11)
			{
				specialSaveRequired = true;
				shooterIndex = 3;
			}
			if (sum == 12)
			{
				specialSaveRequired = true;
				shooterIndex = 5;
			}
			shooterIndex -= 1;
			return shooterIndex;
		}

		// TO DO: Figure out a proper way of handling goal chances, spectacular saves, etc.
		public void GoalChance(int greendie, Player assister, Player shooter, int offsideValue = 6, bool specialSaveRequired = false,
			bool fromPKOT = false)
		{
			int whiteDieVal = Dice.Instance.d6.Roll();
			int blackDieVal = Dice.Instance.d6.Roll();
			int sum = whiteDieVal + blackDieVal;
			double successValue = 60;

			bool goalScored = false;
			if (specialSaveRequired)
			{
				if (sum == 2)
				{
					// saved
					successValue = 100;
				}
				if (sum == 3)
				{
					successValue -= 5;
				}
				if (sum == 4)
				{
					// saved
					successValue = 100;
				}
				if (sum == 5)
				{
					successValue -= 10;
				}
				if (sum == 6)
				{
					successValue += 5;
				}
				if (sum == 7)
				{
					successValue += 10;
				}
				if (sum == 8)
				{
					successValue += 15;
				}
				if (sum == 9)
				{
					successValue -= 5;
				}
				if (sum == 10)
				{
					goalScored = true;
				}
				if (sum == 11)
				{
					successValue -= 10;
				}
				if (sum == 12)
				{
					goalScored = true;
				}

				double maxGKSuccess = 95;
				successValue += (this.DefendingTeam.goalKeeper.goalkeepingDiving - shooter.shooting) / 10;
				if (successValue > maxGKSuccess)
					successValue = maxGKSuccess;

				int d100Value = Dice.Instance.d100.Roll();

				if (!goalScored)
				{
					if (d100Value < successValue)
					{
						ballIsLive = false;
						shooter.gameStats.IncreaseShotOnGoal();
						ShowMessage("Fingertip save! What a save by " + this.DefendingTeam.goalKeeper.fullName + ".");
						this.DefendingTeam.goalKeeper.gameStats.IncreaseSaves();
						AddGKRating(this.DefendingTeam.goalKeeper);
						if (!fromPKOT) ShowMessage("However, the ball goes out of bounds which leads to a corner!");
						goalScored = false;
					}
					else
					{
						goalScored = true;
					}
				}

				if (!goalScored)
				{
					if (!fromPKOT) CornerKick();
					//we return even if it's a PK OT because we don't want anything else to happen
					return;
				}
			}
			else
			{
				goalScored = true;
			}

			if (fromPKOT)
			{
				ShowMessage(this.DefendingTeam.goalKeeper.fullName + " lunges but it isn't enough. Goal!");
				this.PKScored = true;
				// the rest of the method determines if a goal is counted or if someone was offside.
				return;
			}

			bool offsidesNegatesShot = DetermineIfGoalStands(greendie, whiteDieVal, blackDieVal, ref goalScored, offsideValue);
			sum = whiteDieVal + blackDieVal;
			if (sum == 12)
			{
				if (greendie == 6)
				{
					// own goal
					assister = null;
					shooter = null;
				}
			}
			if (offsideValue == 4 && !goalScored)
			{
				// we must be coming from the highlight reel and the goal must not have stood due to the offside call.
				// offside
				this.AttackingTeam.gameStats.offsides++;
				ShowMessage("Members of the " + this.AttackingTeam.Name + " vehemently argue with the referee!");
				ShowMessage("Referee is now hostile toward " + this.AttackingTeam.Name);
				if (!this.attackingTeamIsHomeTeam)
				{
					this.referee.attitudeToVisitingTeam = Enums.RefAttitude.Hostile;
				}
				else
				{
					this.referee.attitudeToHomeTeam = Enums.RefAttitude.Hostile;
				}
			}
			else
			{
				if (shooter != null)
				{
					if (!offsidesNegatesShot)
						shooter.gameStats.IncreaseShotOnGoal();
				}
				if (goalScored)
				{
					UpdateScore(assister, shooter, this.DefendingTeam.goalKeeper);
				}
			}
			//SanityCheck();
			ChangePossession();
		}

		public bool DetermineIfGoalStands(int greenDieVal, int whiteDieVal, int blackDieVal, ref bool goalScored, int offsideValue = 6)
		{
			//LogMessage("Determine if goal stands");
			int sum = whiteDieVal + blackDieVal;
			if (sum == 2)
			{
				ShowMessage("Beautiful effort! Goal!");
			}
			if (sum == 3)
			{
				ShowMessage("Difficult angle! It's in!");
			}
			if (sum == 4)
			{
				ShowMessage("Dead-on shot! Goal!");
			}
			if (sum == 5)
			{
				ShowMessage("Blasted in from close range. Easy goal.");
			}
			if (sum == 6)
			{
				ShowMessage("Chipped in the air and gets in! Goal!");
			}
			if (sum == 7)
			{
				ShowMessage("To the far post - Goal!");
			}
			if (sum == 8)
			{
				ShowMessage("Great finish! Goal!");
			}
			if (sum == 9)
			{
				ShowMessage(this.DefendingTeam.goalKeeper.fullName + " dives but can't get to it! It's in!");
			}
			if (sum == 10)
			{
				ShowMessage("Off-balance shot but gets in! Goal!");
			}
			if (sum == 11)
			{
				ShowMessage("Off the crossbar and In!");
			}
			if (sum == 12)
			{
				if (greenDieVal == 6)
				{
					ShowMessage(this.DefendingTeam.goalKeeper.fullName + " is able to slap it away in the air and makes an excellent save.");
					this.DefendingTeam.goalKeeper.gameStats.IncreaseSaves();
					AddGKRating(this.DefendingTeam.goalKeeper);
					ShowMessage("But the ball is still loose!");
					ShowMessage("However, a defender collects the rebound and... no wait! His backpass to the goalkeeper goes awry and it's an own goal!");
				}
				else
				{
					ShowMessage("Finds the corner of the net - barely! Goal!");
				}
			}

			if ((sum >= 9 && sum <= 11) || greenDieVal < 6)
			{
				if (greenDieVal >= offsideValue)
				{
					ShowMessage("Oh, wait! No! The flag's been raised! He's offside! That'll negate the goal!");
					ShowMessage("Indirect free kick awarded to the defending team.");
					this.AttackingTeam.gameStats.offsides++;
					goalScored = false;
					return true;
				}
			}
			return false;
		}

		public void Rebound(int shooterIndex)
		{
			OnTarget(shooterIndex, this.specialSaveRequired, true);
		}

		// TO DO: Make sure the saved method makes sense.
		public void Saved(bool PKOT = false)
		{
			int whiteDieVal = Dice.Instance.d6.Roll();
			int blackDieVal = Dice.Instance.d6.Roll();
			int sum = whiteDieVal + blackDieVal;
			double numberOfSquares = 0;
			int shooterIndex = -1;
			int defenderIndex = 0;
			this.specialSaveRequired = false;
			bool counterAttack = false;

			foreach (Player p in this.DefendingTeam.onCamera)
			{
				numberOfSquares += p.defending;
			}

			numberOfSquares /= 5;

			if (sum == 2)
			{
				//if (numberOfSquares >= 70)
				//{
					
				//}
				//else
				//{
					
				//}
				defenderIndex = 2;
				shooterIndex = 3;
			}
			if (sum == 3)
			{
				//if (numberOfSquares >= 65)
				//{
					
				//}
				//else
				//{
					
				//}
				defenderIndex = 4;
				shooterIndex = 4;
			}
			if (sum == 4)
			{
				//if (numberOfSquares >= 60)
				//{
					
				//}
				//else
				//{
					
				//}
				defenderIndex = 3;
				shooterIndex = 5;
			}
			if (sum == 5)
			{
				//if (numberOfSquares >= 65)
				//{
					
				//}
				//else
				//{
					
				//}
				defenderIndex = 3;
				shooterIndex = 2;
			}
			if (sum == 6)
			{
				//if (numberOfSquares >= 65)
				//{
					
				//}
				//else
				//{
					
				//}
				defenderIndex = 4;
				shooterIndex = 1;
			}
			if (sum == 7)
			{
				shooterIndex = 1;
				this.specialSaveRequired = true;
			}
			if (sum == 8)
			{
				defenderIndex = 4;
			}
			if (sum == 9)
			{
				defenderIndex = 3;
			}
			if (sum == 10)
			{
				defenderIndex = 2;
			}
			if (sum == 11)
			{
				counterAttack = true;
			}
			if (sum == 12)
			{
			}

			int d100Value = Dice.Instance.d100.Roll();

			if (shooterIndex == -1 || (d100Value < numberOfSquares))
			{
				ballIsLive = true;
				ShowMessage(this.DefendingTeam.goalKeeper.fullName + " catches it in the air and makes an excellent save.");
				this.DefendingTeam.goalKeeper.gameStats.IncreaseSaves();
				AddGKRating(this.DefendingTeam.goalKeeper);
				if (!PKOT)
				{
					ShowMessage("He then kicks the ball to a teammate who gets the ball out of danger.");
				}
				ChangePossession();
				if (counterAttack)
				{
					ShowMessage("A great pass follows and now " + this.AttackingTeam.Name + " are on the move!");
					whiteDieVal = Dice.Instance.d6.Roll();
					blackDieVal = Dice.Instance.d6.Roll();
					Attack(whiteDieVal, blackDieVal, true);
				}
			}
			else
			{
				ballIsLive = true;
				shooterIndex = shooterIndex - 1;
				ShowMessage(this.DefendingTeam.goalKeeper.fullName + " is able to slap it away in the air and makes an excellent save.");
				ShowMessage("But the ball is still loose!");
				this.DefendingTeam.goalKeeper.gameStats.IncreaseSaves();
				AddGKRating(this.DefendingTeam.goalKeeper);
				Player shooter = this.AttackingTeam.onCamera[shooterIndex];
				if (!PlayerIsOffField(shooter))
				{
					Rebound(shooterIndex);
				}
				else
				{
					ShowMessage("Several players on the attacking team get to the ball first...");
					HandleMissingPlayerFlavorTextAndChangePossession();
				}
			}
		}

		public void ChangePlayers()
		{
			//LogMessage("Change Players");
			int greenDieVal = Dice.Instance.d6.Roll();
			int whiteDieVal = Dice.Instance.d6.Roll();
			int blackDieVal = Dice.Instance.d6.Roll();

			if (greenDieVal < 6)
			{
				this.changePlayer(AttackingTeam, greenDieVal);
			}
			if (greenDieVal == 6)
			{
				if (whiteDieVal < 6)
					this.changePlayer(DefendingTeam, whiteDieVal);
				if (blackDieVal < 6 && whiteDieVal != blackDieVal)
					this.changePlayer(DefendingTeam, blackDieVal);
			}
			//ShowCurrentRosters();
			NotifyOnCamera();
		}

		public void ChangePossession(bool PKOT = false)
		{
			if (!PKOT)
			{
				this.AttackingTeam.gameStats.minutesWithBall += (minutesPlayed - timeWhenSeizedPossession);
				timeWhenSeizedPossession = minutesPlayed;
			}
			//LogMessage("Change Possession");
			ClockTick(1);
			Team previousAttackingTeam = this.AttackingTeam;
			this.AttackingTeam = this.DefendingTeam;
			this.DefendingTeam = previousAttackingTeam;

			if (this.AttackingTeam == this.homeTeam)
			{
				this.attackingTeamIsHomeTeam = true;
			}
			else
			{
				this.attackingTeamIsHomeTeam = false;
			}

			changedPossessionAtLeastOnceOrMidfieldBattle = true;
		}

		public void HighlightReel(string type)
		{
			//LogMessage("Highlight Reel");
			ChangePlayers();
			ClockTick(1);
			int greenDieVal = Dice.Instance.d6.Roll();
			int whiteDieVal = Dice.Instance.d6.Roll();
			int blackDieVal = Dice.Instance.d6.Roll();
			int sum = whiteDieVal + blackDieVal;

			int freeKickTaker = 0;
			for (int i = 0; i < this.AttackingTeam.onCamera.Length - 1; i++)
			{
				if (!PlayerIsOffField(this.AttackingTeam.onCamera[i]))
				{
					if (this.AttackingTeam.onCamera[i].freekicks > this.AttackingTeam.onCamera[freeKickTaker].freekicks)
					{
						freeKickTaker = i;
					}
				}
			}

			if (greenDieVal == 6)
				greenDieVal = 1;

			if (sum == 2 || sum == 12)
			{
				if (type == "DFK")
					ShowMessage("Shot fired but right into the wall! The ball ricochets so hard that it has effectively reset play!");
				RareResult();
				ballIsLive = false; // all rare result plays "kill" the ball.
			}
			if (sum == 3)
			{
				Player attacker = this.AttackingTeam.onCamera[greenDieVal - 1];
				if (greenDieVal == 5) greenDieVal = 1; else greenDieVal++;
				Player assister = this.AttackingTeam.onCamera[greenDieVal - 1];
				if (PlayerIsOffField(attacker) || PlayerIsOffField(assister))
				{
					HandleMissingPlayerFlavorTextAndChangePossession();
					return;
				}
				if (type == "M")
				{
					ShowMessage("Fortuitous bounce leads to open shot by " + attacker.fullName);
				}
				if (type == "CK")
				{
					attacker = this.AttackingTeam.onCamera[0];
					if (PlayerIsOffField(attacker))
					{
						HandleMissingPlayerFlavorTextAndChangePossession();
						return;
					}
					ShowMessage("Perfectly executed corner by " + assister.fullName + " to " + attacker.fullName + " who volleys!");
				}
				if (type == "DFK")
				{
					ShowMessage("Directed back to " + attacker.fullName + " who shoots!");
				}
				GoalChance(greenDieVal - 1, assister, attacker, 6, true);
			}
			if (sum == 4)
			{
				if (type == "M")
				{
					Player player = this.awayTeam.onCamera[greenDieVal - 1];
					if (!PlayerIsOffField(player))
					{
						ShowMessage(player.fullName + " is called for a foul! That looked mild, honestly.");
						ShowMessage(player.fullName + " vehemently argues with the referee!");
						ShowMessage("Referee is now friendly toward " + homeTeam.Name);
						referee.attitudeToHomeTeam = Enums.RefAttitude.Friendly;
						if (!this.attackingTeamIsHomeTeam)
						{
							ChangePossession();
						}
						ballIsLive = false;
					}
					else
					{
						HandleMissingPlayerFlavorTextAndChangePossession();
						return;
					}
				}
				if (type == "CK")
				{
					if (FoulLeadsToACorner(greenDieVal, false))
					{
						CornerKick();
					}
					else
					{
						ChangePossession();
					}
				}
				if (type == "DFK")
				{
					if (FoulDuringFreeKickLeadsToSecondOne(greenDieVal, true, freeKickTaker))
					{
						ShowMessage("Another free kick will be granted again in a really dangerous position. I think there's a real chance of a score here.");
						ShowMessage("West Ham United build a wall as they get ready for the free kick.");
						PerformHeaderOrKick(Enums.SpecialShot.FreeKick, freeKickTaker);
					}
					else
					{
						ChangePossession();
					}
				}
			}
			if (sum == 5)
			{
				if (type == "M")
				{
					ballIsLive = false;
					Player homePlayer = this.homeTeam.onCamera[greenDieVal - 1];
					if (PlayerIsOffField(homePlayer))
					{
						HandleMissingPlayerFlavorTextAndChangePossession();
						return;
					}
					if (this.attackingTeamIsHomeTeam)
					{
						ShowMessage("The ball is kicked out of bounds after " + homePlayer.fullName + " gives his opponent a shove. Might've been incidental.");
						ShowMessage("The players on " + this.awayTeam.Name + " certainly don't think it was incidental. They are arguing vehemently at the non-call!");
						ShowMessage("Referee is now hostile toward " + this.awayTeam.Name);
						referee.attitudeToVisitingTeam = Enums.RefAttitude.Hostile;
					}
					else
					{
						ShowMessage(homePlayer.fullName + " with a rather rough tackle. Refree says it was clean. Moreover, he says a player on " + this.awayTeam.Name + " was the last to touch it as it goes out of bounds!");
						ShowMessage("The players on " + this.awayTeam.Name + " certainly don't agree with the non-call. They are arguing vehemently about it!");
						ShowMessage("Referee is now hostile toward " + this.awayTeam.Name);
						referee.attitudeToVisitingTeam = Enums.RefAttitude.Hostile;
						ChangePossession();
					}
				}
				if (type == "CK" || type == "DFK")
				{
					Enums.SpecialShot specShot = Enums.SpecialShot.Header;
					int index = 4;
					if (type == "DFK")
					{
						specShot = Enums.SpecialShot.FreeKick;
						index = freeKickTaker;
					}
					PerformHeaderOrKick(specShot, index);
				}
			}
			if (sum == 6)
			{
				if (type == "M")
				{
					ShowMessage("Play moves toward the sidelines as the offense tries to move and the defense tries to apply pressure.");
					ShowMessage("It's followed by a quick series of passes and some deft dribble moves.");
					ShowMessage("A foot has kicked the ball out of bounds.");
					ShowMessage("On that play, a pair of legs tangled together and their bodies hit the grass. One already rose. The other has not yet.");
					Injury(this.AttackingTeam, greenDieVal);
					ballIsLive = false;
				}
				if (type == "CK" || type == "DFK")
				{
					Enums.SpecialShot specShot = Enums.SpecialShot.Header;
					int index = 2;
					if (type == "DFK")
					{
						specShot = Enums.SpecialShot.FreeKick;
						index = freeKickTaker;
					}
					PerformHeaderOrKick(specShot, index);
				}

			}
			if (sum == 7)
			{
				Enums.SpecialShot specShot = Enums.SpecialShot.Header;
				int shooterIndex = -1;
				if (type == "CK")
				{
					shooterIndex = 0;
				}
				if (type == "DFK")
				{
					specShot = Enums.SpecialShot.FreeKick;
					shooterIndex = freeKickTaker;
				}
				PerformHeaderOrKick(specShot, shooterIndex);
			}
			if (sum == 8)
			{
				if (type == "M")
				{
					ballIsLive = false;
					ShowMessage("Play moves toward the sidelines as the offense tries to move and the defense tries to apply pressure.");
					ShowMessage("It's followed by a quick series of passes and some deft dribble moves.");
					ShowMessage("A foot has kicked the ball out of bounds.");
					ShowMessage("On that play, a pair of legs tangled together and their bodies hit the grass. One already rose. The other has not yet.");
					Injury(this.DefendingTeam, greenDieVal);
				}
				if (type == "CK" || type == "DFK")
				{
					Enums.SpecialShot specShot = Enums.SpecialShot.Header;
					int index = 1;
					if (type == "DFK")
					{
						specShot = Enums.SpecialShot.FreeKick;
						index = freeKickTaker;
					}
					PerformHeaderOrKick(specShot, index);
				}
			}
			if (sum == 9)
			{
				if (type == "M")
				{
					Player awayPlayer = this.awayTeam.onCamera[greenDieVal - 1];
					if (PlayerIsOffField(awayPlayer))
					{
						HandleMissingPlayerFlavorTextAndChangePossession();
						return;
					}
					if (!this.attackingTeamIsHomeTeam)
					{
						ballIsLive = false;
						ShowMessage("The ball is kicked out of bounds after " + awayPlayer.fullName + " gives his opponent a shove. Might've been incidental.");
						ShowMessage("The players on " + this.homeTeam.Name + " certainly don't think it was incidental. They are arguing vehemently at the non-call!");
						ShowMessage("Referee is now hostile toward " + this.homeTeam.Name);
						referee.attitudeToHomeTeam = Enums.RefAttitude.Hostile;
					}
					else
					{
						ballIsLive = false;
						ShowMessage(awayPlayer.fullName + " with a rather rough tackle. Refree says it was clean. Moreover, he says a player on " + this.homeTeam.Name + " was the last to touch it as it goes out of bounds!");
						ShowMessage("The players on " + this.homeTeam.Name + " certainly don't agree with the non-call. They are arguing vehemently about it!");
						ShowMessage("Referee is now hostile toward " + this.homeTeam.Name);
						referee.attitudeToHomeTeam = Enums.RefAttitude.Hostile;
						ChangePossession();
					}
				}
				if (type == "CK" || type == "DFK")
				{
					Enums.SpecialShot specShot = Enums.SpecialShot.Header;
					int index = 3;
					if (type == "DFK")
					{
						specShot = Enums.SpecialShot.FreeKick;
						index = freeKickTaker;
					}
					PerformHeaderOrKick(specShot, index);
				}
			}
			if (sum == 10)
			{
				Player homePlayer = this.homeTeam.onCamera[greenDieVal - 1];
				if (PlayerIsOffField(homePlayer))
				{
					HandleMissingPlayerFlavorTextAndChangePossession();
					return;
				}
				if (type == "M")
				{
					ballIsLive = false;
					ShowMessage(homePlayer.fullName + " is called for a foul! That looked mild, honestly.");
					ShowMessage(homePlayer.fullName + " vehemently argues with the referee!");
					ShowMessage("Referee is now friendly toward " + this.awayTeam.Name);
					referee.attitudeToVisitingTeam = Enums.RefAttitude.Friendly;
					if (!this.attackingTeamIsHomeTeam)
					{
						ChangePossession();
					}
				}
				if (type == "CK")
				{
					if (FoulLeadsToACorner(greenDieVal, true))
					{
						CornerKick();
					}
					else
					{
						ChangePossession();
					}
				}
				if (type == "DFK")
				{
					if (FoulDuringFreeKickLeadsToSecondOne(greenDieVal, true, freeKickTaker))
					{
						ShowMessage("Another free kick will be granted again in a really dangerous position. I think there's a real chance of a score here.");
						ShowMessage("West Ham United build a wall as they get ready for the free kick.");
						PerformHeaderOrKick(Enums.SpecialShot.FreeKick, freeKickTaker);
					}
					else
					{
						ChangePossession();
					}
				}
			}
			if (sum == 11)
			{
				Player player = this.AttackingTeam.onCamera[greenDieVal - 1];
				if (PlayerIsOffField(player))
				{
					HandleMissingPlayerFlavorTextAndChangePossession();
					return;
				}
				if (type == "M")
				{
					ShowMessage("Series of great passes leaves the net open for " + player.fullName);
				}
				if (type == "CK")
				{
					ShowMessage("An unexpected bounce creates a gift shot for " + player.fullName);
				}
				OnTarget(greenDieVal -1, true, false, 6);
			}
		}

		// TO DO: Figure out a way to factor in a player's free kick ability.
		public void FreeKick(int sum)
		{
			ShowMessage(DefendingTeam.Name + " build a wall as they get ready for the free kick.");
			ClockTick(1);
			if (sum == 2)
			{
				OnTarget(-1, true, false);
			}
			else if (sum == 3)
			{
				HighlightReel("DFK");
			}
			else if (sum == 4)
			{
				OnTarget();
			}
			else if (sum == 5)
			{
				RefereeDecision("K");
			}
			else
			{
				ShowMessage("Shot fired but right into the wall! The ball ricochets so hard that it has effectively reset play!");
				DeterminePitchAction();
			}
		}

		public bool FoulLeadsToACorner(int greenDieVal, bool homeTeamArgues)
		{
			//LogMessage("HandleCornerArgueResult");
			if (greenDieVal == 6) greenDieVal = 1;
			Player attacker = this.FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[greenDieVal - 1], this.AttackingTeam.onCamera, (greenDieVal - 1));
			Player defender = this.FindPlayerWhenNoMatter(this.DefendingTeam.onCamera[greenDieVal - 1], this.DefendingTeam.onCamera, (greenDieVal - 1));
			ShowMessage(attacker.fullName + " and " + defender.fullName + " both seem to hit the ball at the same time going for the corner!");
			ShowMessage("The referee says " + defender.fullName + " touched it last, giving another corner to " + this.AttackingTeam.Name + ".");
			ClockTick(1);
			ballIsLive = false;

			Team offendedTeam = awayTeam;

			if (homeTeamArgues)
			{
				offendedTeam = homeTeam;
				referee.attitudeToHomeTeam = Enums.RefAttitude.Hostile;
			}
			else
			{
				referee.attitudeToVisitingTeam = Enums.RefAttitude.Hostile;
			}
			bool offensiveTeamIsOffended = (homeTeamArgues && this.attackingTeamIsHomeTeam) || (!homeTeamArgues && !this.attackingTeamIsHomeTeam);
			Player offendedPlayer = this.FindPlayerWhenNoMatter(offendedTeam.onCamera[greenDieVal - 1], offendedTeam.onCamera, (greenDieVal-1));

			if (!offensiveTeamIsOffended)
			{
				// home team thinks they should have the ball
				ShowMessage(offendedPlayer.fullName + " vehemently disagrees with the call and lets the referee know this quite loudly! He thinks it should have been a goal kick.");
				ShowMessage("Referee is now hostile toward " + offendedTeam.Name);
			}
			else
			{
				// home team thinks they were fouled
				ShowMessage(offendedPlayer.fullName + " is yelling at the referee! I'm not quite sure why. I think he thinks it should have been a foul and a penalty.");
				ShowMessage("Referee is now hostile toward " + offendedTeam.Name);
			}
			return true;
		}

		public bool FoulDuringFreeKickLeadsToSecondOne(int greenDieVal, bool homeTeamArgues, int shotTaker)
		{
			ClockTick(1);
			ballIsLive = false;
			//LogMessage("HandleFreeKickArgueResult");
			if (greenDieVal == 6) greenDieVal = 1;
			Player shooter = this.FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[shotTaker], this.AttackingTeam.onCamera, shotTaker);
			ShowMessage(shooter.fullName + " shoots on goal from the free kick! Can he make it from this distance?");

			Team offendedTeam = awayTeam;

			if (homeTeamArgues)
			{
				offendedTeam = homeTeam;
				referee.attitudeToHomeTeam = Enums.RefAttitude.Hostile;
			}
			else
			{
				referee.attitudeToVisitingTeam = Enums.RefAttitude.Hostile;
			}
			bool offensiveTeamIsOffended = (homeTeamArgues && this.attackingTeamIsHomeTeam) || (!homeTeamArgues && !this.attackingTeamIsHomeTeam);
			Player offendedPlayer = this.FindPlayerWhenNoMatter(offendedTeam.onCamera[greenDieVal - 1], offendedTeam.onCamera, (greenDieVal - 1));
			Player attacker = this.FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[shotTaker], this.AttackingTeam.onCamera, shotTaker);

			if (!offensiveTeamIsOffended)
			{
				// home team thinks they should have the ball
				ShowMessage(attacker.fullName + " misses so that will be a goal kick... no, wait! The referee is calling on off the ball foul on " + offendedTeam.onCamera[greenDieVal - 1].fullName +
					" which means another free kick even closer to the goal!");
				ShowMessage(offendedPlayer.fullName + " vehemently disagrees with the call and lets the referee know this quite loudly! He thinks it should have been a goal kick.");
				ShowMessage("Referee is now hostile toward " + offendedTeam.Name);
				return true;
			}
			else
			{
				// home team thinks they were fouled
				ShowMessage(attacker.fullName + " misses so that will be a goal kick...");
				ShowMessage(offendedPlayer.fullName + " is yelling at the referee! I'm not quite sure why. I think he thinks he was fouled they should have a penalty.");
				ShowMessage("Referee is now hostile toward " + offendedTeam.Name);
				return false;
			}
		}

		// TO DO: See if free kick adjustments work and make sense.
		public void PerformHeaderOrKick(Enums.SpecialShot specShot, int shooterIndex = -1)
		{
			Player shooter = null;
			int greenDieVal = 1;
			greenDieVal = Dice.Instance.d6.Roll();

			if (greenDieVal == 6)
			{
				greenDieVal = 5;
			}
			shooter = this.AttackingTeam.onCamera[greenDieVal - 1];
			//LogMessage("PerformHeaderOrKick");
			Player assister = null;
			while (assister == null)
			{
				int secondGreenDieRoll = Dice.Instance.d6.Roll();
				if (secondGreenDieRoll == 6)
				{
					secondGreenDieRoll = 5;
				}
				if (secondGreenDieRoll == greenDieVal)
				{
					if (secondGreenDieRoll == 1)
						secondGreenDieRoll++;
					else if (secondGreenDieRoll == 5)
						secondGreenDieRoll--;
					else
						secondGreenDieRoll--;
				}
				assister = this.AttackingTeam.onCamera[secondGreenDieRoll - 1];
				if (PlayerIsOffField(assister))
					assister = null;
			}

			int whiteDieVal = Dice.Instance.d6.Roll();
			int blackDieVal = Dice.Instance.d6.Roll();
			int sum = whiteDieVal + blackDieVal;

			int ability = shooter.header;
			int goalKeepingAbility = this.DefendingTeam.goalKeeper.goalkeepingDiving;
			if (specShot == Enums.SpecialShot.FreeKick)
			{
				shooter = FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[greenDieVal - 1], this.AttackingTeam.onCamera, (greenDieVal - 1));
				ability = shooter.freekicks;
				ShowMessage(shooter.fullName + " shoots on goal from the free kick! Can he make it from this distance?");
			}
			else
			{
				ShowMessage(assister.fullName + " with a nice cross to " + shooter.fullName + "!");
				if (PlayerIsOffField(shooter))
				{
					ShowMessage("But the pass goes awry!");
					HandleMissingPlayerFlavorTextAndChangePossession();
					return;
				}
				ShowMessage(shooter.fullName + " with the header!");
			}
			int successValue = 62;
			int maxGKSuccess = 95;
			successValue += (goalKeepingAbility - ability);
			if (successValue > maxGKSuccess)
				successValue = maxGKSuccess;

			int d100Roll = Dice.Instance.d100.Roll();
			bool goalScored = false;

			if (d100Roll >= successValue)
			{
				ShowMessage(this.DefendingTeam.goalKeeper.fullName + " dives but cannot get there in time and the ball hits the back of the net! Goal!");
				goalScored = true;
				UpdateScore(assister, shooter, this.DefendingTeam.goalKeeper);
			}
			else
			{
				ballIsLive = false;
				ShowMessage(this.DefendingTeam.goalKeeper.fullName + " dives and knocks the ball to the ground and is able to gather it on a bounce for a terrific save!");
				this.DefendingTeam.goalKeeper.gameStats.IncreaseSaves();
			}
			shooter.gameStats.IncreaseShotOnGoal(true);
			ChangePossession();
		}

		public void Injury(Team injuredTeam, int greendie)
		{
			ballIsLive = false;
			//LogMessage("Injury");
			if (greendie == 6)
			{
				// player shakes it off
				minInjuryTime += 1;
				ShowMessage("It looked like someone might have been injured but it appears to be a false alarm.");
			}
			else
			{
				Player injuredPlayer = injuredTeam.onCamera[greendie - 1];
				if (PlayerIsOffField(injuredPlayer))
				{
					ShowMessage("It looked like someone might have been injured but it appears to be a false alarm.");
				}
				else
				{
					minInjuryTime += 1;
					ShowMessage("The physio comes to attend to " + injuredPlayer.fullName + ". His injury seems pretty bad...");
					int injuryRoll = Dice.Instance.d100.Roll();
					if (injuryRoll < injuredPlayer.stamina)
					{
						ShowMessage("Wait! He's getting up! He's okay! Thank goodness for that magic sponge!");
					}
					else
					{
						injuredPlayer.isInjured = true;
						gameSummary.Add(injuredPlayer.fullName + " of team " + injuredTeam.Name + " went down with an injury at minute: " + this.DetermineCurrentTime());
						injuryRoll = Dice.Instance.d100.Roll();
						if (injuryRoll < injuredPlayer.stamina)
						{
							ShowMessage("He's up and walking but he seems to be favoring his injury. I don't know if he'll continue playing. It might be a good idea to sub him off but we'll see.");
							injuredPlayer.gamesOutDueToInjury = 1; // this game.
						}
						else
						{
							ShowMessage("The stretcher comes to take him off the field. Hopefully, he's all right. In the meantime, we'll see if they have a sub available.");
							injuryRoll = Dice.Instance.d100.Roll();
							int injuryResistanceModified = injuredPlayer.stamina - 20;
							if (injuryRoll < injuryResistanceModified)
							{
								minInjuryTime += 3;
								injuredPlayer.gamesOutDueToInjury = Dice.Instance.d6.Roll();
							}
							else
							{
								minInjuryTime += 4;
								injuredPlayer.gamesOutDueToInjury = Dice.Instance.d6.Roll() + Dice.Instance.d6.Roll();
							}
							//modifier - when the season advances a week, it reduces the number of games out due to injury. Hence, the modifier.
							injuredPlayer.gamesOutDueToInjury++;
						}
						if ((injuredTeam == homeTeam && numberOfHomeSubsLeft > 0) ||
							(injuredTeam != homeTeam && numberOfAwaySubsLeft > 0))
						{
							SubstitutePlayer(greendie - 1, injuredTeam);
						}
						else
						{
							// The player will be considered off the field and missing.
						}
					}
				}
			}
		}

		//public void Injury(Team injuredTeam, int greendie)
		//{
		//    ballIsLive = false;
		//    //LogMessage("Injury");
		//    if (greendie == 6)
		//    {
		//        // player shakes it off
		//        minInjuryTime += 1;
		//        ShowMessage("It looked like someone might have been injured but it appears to be a false alarm.");
		//    }
		//    else
		//    {
		//        Player injuredPlayer = injuredTeam.onCamera[greendie - 1];
		//        if (PlayerIsOffField(injuredPlayer))
		//        {
		//            ShowMessage("It looked like someone might have been injured but it appears to be a false alarm.");
		//        }
		//        else
		//        {
		//            minInjuryTime += 1;
		//            ShowMessage("The physio comes to attend to " + injuredPlayer.fullName + ". His injury seems pretty bad...");
		//            if (injuredPlayer.injuryResistance == Enums.Ability.Amazing)
		//            {
		//                ShowMessage("Wait! He's getting up! He's okay! Thank goodness for that magic sponge!");
		//            }
		//            else
		//            {
		//                injuredPlayer.isInjured = true;
		//                gameSummary.Add(injuredPlayer.fullName + " of team " + injuredTeam.Name + " went down with an injury at minute: " + this.DetermineCurrentTime());
		//                if (injuredPlayer.injuryResistance == Enums.Ability.AboveAverage)
		//                {
		//                    ShowMessage("He's up and walking but he seems to be favoring his injury. I don't know if he'll continue playing. It might be a good idea to sub him off but we'll see.");
		//                }
		//                else
		//                {
		//                    ShowMessage("The stretcher comes to take him off the field. Hopefully, he's all right. In the meantime, we'll see if they have a sub available.");
		//                    if (injuredPlayer.injuryResistance == Enums.Ability.Average)
		//                    {
		//                        minInjuryTime += 3;
		//                        injuredPlayer.gamesOutDueToInjury = Dice.Instance.d6.Roll();
		//                    }
		//                    if (injuredPlayer.injuryResistance == Enums.Ability.BelowAverage)
		//                    {
		//                        minInjuryTime += 4;
		//                        injuredPlayer.gamesOutDueToInjury = Dice.Instance.d6.Roll() + Dice.Instance.d6.Roll();
		//                    }
		//                    //modifier - when the season advances a week, it reduces the number of games out due to injury. Hence, the modifier.
		//                    injuredPlayer.gamesOutDueToInjury++;
		//                }
		//                if ((injuredTeam == homeTeam && numberOfHomeSubsLeft > 0) ||
		//                    (injuredTeam != homeTeam && numberOfAwaySubsLeft > 0))
		//                {
		//                    SubstitutePlayer(greendie - 1, injuredTeam);
		//                }
		//                else
		//                {
		//                    // The player will be considered off the field and missing.
		//                }
		//            }
		//        }
		//    }
		//}

		public void HomeFieldAdvantage(int whitedie, int blackdie)
		{
			bool homeTeamWinsEncounter = true;
			bool attackingTeamWinsEncounter = true;

			if (OnPitchCount(homeTeam) < OnPitchCount(awayTeam))
			{
				homeTeamWinsEncounter = false;
				if (attackingTeamIsHomeTeam)
					attackingTeamWinsEncounter = false;
			}
			else
			{
				if (attackingTeamIsHomeTeam)
					attackingTeamWinsEncounter = true;
				else
					attackingTeamWinsEncounter = false;
			}

			if (!attackingTeamWinsEncounter)
			{
				ShowMessage(AttackingTeam.Name + " attempts to pass but no! That poor pass has led to a opportunity for " 
					+ DefendingTeam.Name + " a counterattack!");
				if (homeTeamWinsEncounter)
					ShowMessage("The crowd cheers wildy thanks to this turn of events!");
				ChangePossession();
			}
			else
			{
				if (homeTeamWinsEncounter)
					ShowMessage("The crowd cheers wildly!");
				ShowMessage("A perfect through ball by " + AttackingTeam.Name + " signifies they are clearly on the attack!");
			}
			Attack(whitedie, blackdie, false);
		}

		public bool OffsideTrapAttempted(string type, int sum, Enums.RefAttitude disposition)
		{
			//LogMessage("OffsideTrapAttempted");
			return (sum == 4 || ((type == "M") && (sum == 5 || sum == 9)));
		}

		public bool OffensiveFoulOccurred(string type, int sum, Enums.RefAttitude disposition, ref bool yellowCard)
		{
			ballIsLive = false;
			yellowCard = false;
			bool offensiveFoulOccurred = false;
			if (sum == 5 && type == "K")
			{
				if (disposition == Enums.RefAttitude.Hostile)
					yellowCard = true;
				if (disposition > Enums.RefAttitude.Friendly)
					offensiveFoulOccurred = true;
			}
			if ((sum == 9 || sum == 10) && disposition == Enums.RefAttitude.Hostile)
				offensiveFoulOccurred = true;
			return offensiveFoulOccurred;
		}

		public bool DefensiveFoulOccurred(string type, int sum, Enums.RefAttitude disposition)
		{
			//LogMessage("DefensiveFoulOccurred");
			if (sum == 2 || sum == 3 || sum == 7 || sum == 12)
			{
				return true;
			}
			if (sum == 6)
			{
				if (type == "M")
				{
					return true;
				}
				else
				{
					if (disposition == Enums.RefAttitude.Friendly || disposition == Enums.RefAttitude.Neutral)
					{
						return true;
					}
				}
			}
			if (sum == 8)
			{
				if (disposition == Enums.RefAttitude.Friendly)
				{
					return true;
				}
				else
				{
					if (type == "M" && disposition == Enums.RefAttitude.Neutral)
					{
						return true;
					}
				}
			}
			if (sum == 9 || sum == 11)
			{
				if (type == "K")
				{
					if (disposition != Enums.RefAttitude.Hostile)
					{
						return true;
					}
				}
			}
			if (sum == 10)
			{
				if (type == "M" && disposition != Enums.RefAttitude.Hostile)
				{
					return true;
				}
			}
			return false;
		}

		private void HandleOffsideTrapAttempt(string type, int sum, Enums.RefAttitude disposition, ref bool penaltyKick, ref bool offside,
			ref bool specialSave, ref int shooterIndex)
		{
			//LogMessage("HandleOffsideTrapAttempt");
			offside = false;
			penaltyKick = false;
			specialSave = false;
			int offsideValue = 6;
			if (type == "M")
			{
				if (sum == 4)
				{
					if (disposition == Enums.RefAttitude.Friendly)
					{
						offsideValue = 4;
						shooterIndex = 0;
						penaltyKick = true;
					}
					if (disposition == Enums.RefAttitude.Neutral)
					{
						offsideValue = 5;
						shooterIndex = 0;
						penaltyKick = true;
					}
					if (disposition == Enums.RefAttitude.Hostile)
					{
						offsideValue = 1;
					}
				}
				if (sum == 5)
				{
					if (disposition == Enums.RefAttitude.Friendly)
					{
						offsideValue = 4;
						shooterIndex = 1;
						specialSave = true;
					}
					if (disposition == Enums.RefAttitude.Neutral)
					{
						offsideValue = 5;
						shooterIndex = 1;
						specialSave = true;
					}
					if (disposition == Enums.RefAttitude.Hostile)
					{
						offsideValue = 1;
					}
				}
				if (sum == 9)
				{
					if (disposition == Enums.RefAttitude.Friendly)
					{
						offsideValue = 2;
						shooterIndex = 0;
						specialSave = true;
					}
					if (disposition == Enums.RefAttitude.Neutral)
					{
						offsideValue = 3;
						shooterIndex = 0;
						specialSave = true;
					}
					if (disposition == Enums.RefAttitude.Hostile)
					{
						offsideValue = 1;
					}
				}
			}
			if (type == "K")
			{
				if (sum == 4)
				{
					specialSave = true;
					if (disposition == Enums.RefAttitude.Friendly) offsideValue = 3;
					if (disposition == Enums.RefAttitude.Neutral) offsideValue = 4;
					if (disposition == Enums.RefAttitude.Hostile) offsideValue = 5;
				}
			}
			int greendie = Dice.Instance.d6.Roll();
			if (greendie <= offsideValue)
			{
				offside = true;
				shooterIndex = -1;
				specialSave = false;
				penaltyKick = false;
			}
		}

		private void HandleDefensiveFoul(string type, int sum, Enums.RefAttitude disposition, ref bool penaltyKick,
			ref bool DirectFreeKick, ref bool IndirectFreeKick, ref Player tackler, ref bool yellowCardShown, ref bool redcardShown)
		{
			if (PlayerIsOffField(tackler))
			{
				ShowMessage("A couple players legs get caught together but it looks like it's merely inadvertant contact. No foul.");
				return;
			}

			//LogMessage("HandleDefensiveFoul");
			penaltyKick = false;
			DirectFreeKick = false;
			IndirectFreeKick = false;
			bool yellowCardPotential = false;
			bool redCardPotential = false;
			bool hardDominates = false;
			bool cardAvgDominates = false;
			bool cardPlusDominates = false;

			if (sum == 2)
			{
				if (type == "M" || disposition == Enums.RefAttitude.Neutral || disposition == Enums.RefAttitude.Friendly)
				{
					yellowCardPotential = true;
				}
				if (disposition == Enums.RefAttitude.Friendly || disposition == Enums.RefAttitude.Neutral)
				{

					penaltyKick = true;
				}
				else
				{
					DirectFreeKick = true;
					if (type == "K")
					{
						yellowCardPotential = true;
					}
				}
			}
			if (sum == 3)
			{
				if (disposition == Enums.RefAttitude.Friendly)
				{
					if (type == "M")
					{
						yellowCardPotential = true;
					}
					penaltyKick = true;
				}
				if (disposition == Enums.RefAttitude.Neutral)
				{
					DirectFreeKick = true;
				}
				if (disposition == Enums.RefAttitude.Hostile)
				{
					IndirectFreeKick = true;
				}
			}
			if (sum == 6)
			{
				if (disposition == Enums.RefAttitude.Friendly)
				{
					DirectFreeKick = true;
					yellowCardPotential = true;
				}
				if (disposition == Enums.RefAttitude.Neutral || (disposition == Enums.RefAttitude.Hostile && type == "M"))
				{
					IndirectFreeKick = true;
					if (type == "M" && disposition != Enums.RefAttitude.Hostile)
					{
						yellowCardPotential = true;
					}
				}
			}
			if (sum == 7)
			{
				if (disposition == Enums.RefAttitude.Friendly || disposition == Enums.RefAttitude.Friendly)
				{
					DirectFreeKick = true;
					yellowCardPotential = true;
					if (type == "K" && disposition == Enums.RefAttitude.Neutral)
					{
						hardDominates = true;
					}
				}
				else
				{
					IndirectFreeKick = true;
				}
			}
			if (sum == 8)
			{
				if (type == "M")
				{
					if (disposition == Enums.RefAttitude.Friendly || disposition == Enums.RefAttitude.Neutral)
					{
						yellowCardPotential = true;
						IndirectFreeKick = true;
					}
				}
				else
				{
					if (disposition == Enums.RefAttitude.Friendly)
					{
						DirectFreeKick = true;
						yellowCardPotential = true;
					}
				}
			}
			if (sum == 9)
			{
				if (type == "K")
				{
					if (disposition == Enums.RefAttitude.Friendly)
					{
						DirectFreeKick = true;
					}
					if (disposition == Enums.RefAttitude.Neutral)
					{
						IndirectFreeKick = true;
					}
				}
			}
			if (sum == 10)
			{
				if (type == "M")
				{
					if (disposition == Enums.RefAttitude.Friendly || (disposition == Enums.RefAttitude.Neutral))
					{
						IndirectFreeKick = true;
						hardDominates = true;
						yellowCardPotential = true;
					}
				}
			}
			if (sum == 11)
			{
				if (type == "K")
				{
					if (disposition == Enums.RefAttitude.Friendly || disposition == Enums.RefAttitude.Neutral)
					{
						penaltyKick = true;
						yellowCardPotential = true;
					}
				}
			}
			if (sum == 12)
			{
				yellowCardPotential = true;
				if (disposition == Enums.RefAttitude.Friendly || disposition == Enums.RefAttitude.Neutral)
				{
					redCardPotential = true;
					if (disposition == Enums.RefAttitude.Friendly)
					{
						cardAvgDominates = true;
						penaltyKick = true;
					}
					else
					{
						DirectFreeKick = true;
						cardPlusDominates = true;
					}
				}
				else
				{
					if (type == "M")
					{
						IndirectFreeKick = true;
					}
					else
					{
						penaltyKick = true;
					}
				}
			}

			this.DetermineIfCardsShownDefensiveFoul(yellowCardPotential, redCardPotential, hardDominates, cardAvgDominates, cardPlusDominates, ref yellowCardShown, ref redcardShown, ref tackler);
		}

		// TO DO: Figure out a better way of determining if a player is yellow or red carded. It should be more random.
		private void DetermineIfCardsShownDefensiveFoul(bool yellowCardPotential, bool redCardPotential, bool hardDominates,
			bool cardAvgDominates, bool cardPlusDominates, ref bool yellowCardShown, ref bool redCardShown, ref Player tackler)
		{
			//LogMessage("DetermineIfCardsShownDefensiveFoul");
			redCardShown = false;
			yellowCardShown = false;

			int greendie = Dice.Instance.d6.Roll();
			if (greendie == 6) greendie--;

			if (PlayerIsOffField(tackler))
			{
				ShowMessage("A couple players legs get caught together but it looks like it's merely inadvertant contact. No foul.");
				return;
			}

			if (!yellowCardPotential && !redCardPotential)
			{
				return;
			}

			Enums.Ability howOftenCardTest = Enums.Ability.Average;
			Enums.Ability playerCardedValue = Enums.Ability.BelowAverage;

			if (cardPlusDominates)
			{
				howOftenCardTest = Enums.Ability.AboveAverage;
			}

			int d100Roll = Dice.Instance.d100.Roll();
			if ((d100Roll - tackler.aggression) < 0)
				playerCardedValue = Enums.Ability.AboveAverage;
			else if ((d100Roll - tackler.aggression) > 0)
			{
				d100Roll = Dice.Instance.d100.Roll();
				playerCardedValue = Enums.Ability.Average;
				if ((d100Roll - tackler.aggression) > 0)
					playerCardedValue = Enums.Ability.BelowAverage;
			}

			//if (tackler.physicality > 80)
			//    playerCardedValue = Enums.Ability.AboveAverage;
			//else if (tackler.physicality > 50)
			//    playerCardedValue = Enums.Ability.Average;

			bool cardPlayerFound = false;

			if (redCardPotential)
			{
				if (greendie < 6)
				{
					if (playerCardedValue <= howOftenCardTest)
					{
						redCardShown = true;
					}
					else
					{
						yellowCardShown = true;
					}
				}
				// else no red card shown
			}
			else
			{
				yellowCardShown = true;
				if (greendie == 6)
				{
					foreach (Player p in this.DefendingTeam.onCamera)
					{
						if (playerCardedValue >= Enums.Ability.AboveAverage)
						{
							tackler = p;
							cardPlayerFound = true;
							break;
						}
					}
					if (!cardPlayerFound || hardDominates)
					{
						foreach (Player p in this.DefendingTeam.onCamera)
						{
							if (playerCardedValue >= Enums.Ability.AboveAverage)
							{
								tackler = p;
								break;
							}
						}
					}
				}
			}
		}

		public void RefereeDecision(string type)
		{
			//LogMessage("RefereeDecision");
			ChangePlayers();
			ClockTick(1);
			Enums.RefAttitude disposition = referee.attitudeToHomeTeam;
			if (!attackingTeamIsHomeTeam)
			{
				disposition = referee.attitudeToVisitingTeam;
			}

			int greendie = Dice.Instance.d6.Roll();
			int blackdie = Dice.Instance.d6.Roll();
			int whitedie = Dice.Instance.d6.Roll();
			int sum = whitedie + blackdie;

			bool penaltyKick = false;
			bool directFreeKick = false;
			bool indirectFreeKick = false;
			if (greendie == 6) greendie = 5;
			Player tackler = FindPlayerWhenNoMatter(this.DefendingTeam.onCamera[greendie - 1], this.DefendingTeam.onCamera, (greendie - 1));
			bool yellowCardShown = false;
			bool redCardShown = false;
			bool defensiveFoulOccurred = DefensiveFoulOccurred(type, sum, disposition);

			if (defensiveFoulOccurred)
			{
				ballIsLive = false;
				HandleDefensiveFoul(type, sum, disposition, ref penaltyKick, ref directFreeKick, ref indirectFreeKick, ref tackler, ref yellowCardShown, ref redCardShown);
			}

			bool goalKickOccurred = false;

			if (!defensiveFoulOccurred)
			{
				if (sum == 10 || sum == 11)
				{
					if (disposition == Enums.RefAttitude.Hostile)
					{
						ballIsLive = false;
						goalKickOccurred = (type == "K") || (sum == 11);
					}
				}
			}
			bool loseBall = false;
			bool changePossession = false;
			if (!goalKickOccurred)
			{
				if (sum == 6 || sum == 8)
				{
					if (disposition == Enums.RefAttitude.Hostile)
					{
						loseBall = (type == "K") || (sum == 8);
						changePossession = loseBall;
					}
				}
			}
			bool offensiveFoul = OffensiveFoulOccurred(type, sum, disposition, ref yellowCardShown);
			if (offensiveFoul) changePossession = true;
			greendie = Dice.Instance.d6.Roll();
			if (greendie == 6) greendie--;
			Player attacker = this.FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[greendie - 1], this.AttackingTeam.onCamera, (greendie - 1));

			bool offsideTrapAttempted = OffsideTrapAttempted(type, sum, disposition);
			bool offside = false;
			int shooterIndex = -1;
			if (offsideTrapAttempted)
			{
				HandleOffsideTrapAttempt(type, sum, disposition, ref penaltyKick, ref offside, ref specialSaveRequired, ref shooterIndex);
			}
			bool cornerKick = false;
			if (!offsideTrapAttempted)
			{
				if (type == "M")
				{
					if (sum == 11 && (disposition != Enums.RefAttitude.Hostile))
					{
						cornerKick = true;
					}
				}
				if (type == "K")
				{
					if ((disposition == Enums.RefAttitude.Friendly && (sum == 5 || sum == 10)) || (disposition == Enums.RefAttitude.Neutral && (sum == 8 || sum == 10)))
					{
						cornerKick = true;
					}
				}
			}

			attacker = FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[0], this.AttackingTeam.onCamera, 0);
			ShowMessage(attacker.fullName + " rushes past the defense as a pass sails to him.");
			ShowMessage("Defenders scramble to catch up!");

			if (offside)
			{
				ballIsLive = false;
				ShowMessage("The flag's been raised. An attacking player has gone adrift and he's offside.");
				ShowMessage("On second look, I can clearly see that was an excellent offside trap.");
				ShowMessage("Indirect free kick awarded to the defending team.");
				this.AttackingTeam.gameStats.offsides++;
				changePossession = true;
			}
			if (defensiveFoulOccurred)
			{
				ballIsLive = false;
				this.HandleFoulEvent(tackler, yellowCardShown, redCardShown, true);
			}
			if (penaltyKick)
			{
				ballIsLive = false;
				ShowMessage("And it's, yes! Didn't see it at first but that foul occurred within the box. That'll be a penalty kick!");
				PenaltyKick();
			}
			if (directFreeKick)
			{
				ballIsLive = false;
				ShowMessage("A free kick will be granted in a really dangerous position. I think there's a real chance of a score here.");
				FreeKick(sum);
			}
			if (indirectFreeKick)
			{
				ballIsLive = false;
				ShowMessage("A free kick will be granted but not anywhere near enough to the opposition goal to be dangerous. He'll just pass it from here I'm sure.");
			}
			if (cornerKick)
			{
				ballIsLive = false;
				ShowMessage("A defender rushes in front of the attacker!");
				ShowMessage("He gets a foot to the ball and knocks it past the end line!");
				ShowMessage("That'll be a corner kick for " + this.AttackingTeam.Name + "!");
				CornerKick();
				return;
			}
			if (offensiveFoul)
			{
				ballIsLive = false;
				ShowMessage(attacker.fullName + " runs to get into a better position.");
				ShowMessage("Defenders scramble to catch up to him.");
				ShowMessage("Oof! Looks like someone might've been pushed down there.");
				ShowMessage("Yep! There's a whistle! That's an offensive foul");
				HandleFoulEvent(attacker, yellowCardShown, redCardShown, false);
			}
			if (changePossession)
			{
				if (!offside && !offensiveFoul) ShowMessage("On the play, the attacker loses the ball and the defense collects it.");
				if (offensiveFoul)
				{ ShowMessage("A free kick will be granted but not anywhere near enough to the opposition goal to be dangerous. He'll just pass it from here I'm sure."); ballIsLive = false; }
				ChangePossession();
				return;
			}
		}

		// TO DO: Actually factor in penalty kicks properly. I'm thinking within values, critical misses, etc.
		private void PenaltyKick(Player shotTaker = null, bool OT = false)
		{
			//LogMessage("PenaltyKick");
			int shotIndex = 0;
			if (shotTaker == null)
			{
				shotTaker = FindPlayerWhenNoMatter(AttackingTeam.onCamera[shotIndex], AttackingTeam.onCamera, shotIndex);
				for (int i = 0; i < AttackingTeam.onCamera.Length; i++)
				{
					if (!PlayerIsOffField(AttackingTeam.onCamera[i]))
					{
						if (AttackingTeam.onCamera[i].penalties > shotTaker.penalties)
						{
							shotTaker = AttackingTeam.onCamera[i];
							shotIndex = i;
						}
					}
				}
			}
			ClockTick(1);
			int blackdie = Dice.Instance.d6.Roll();
			int whitedie = Dice.Instance.d6.Roll();
			int sum = blackdie + whitedie;
			bool saved = false;
			bool divingSave = false;
			bool specSave = false;
			bool whiff = false;
			bool goalScored = false;

			ShowMessage(shotTaker.fullName + " lines up for the penalty kick. Hard to say if there's more pressure on him or " + DefendingTeam.goalKeeper.fullName + ".");

			if (sum == 2)
			{
				whiff = true;
			}
			if (sum == 3)
			{
				saved = true;
			}
			if (sum == 4)
			{
				if (shotTaker.penalties <= 70)
				{
					divingSave = true;
				}
				else if (shotTaker.penalties <= 85)
				{
					specSave = true;
				}
				else
				{
					goalScored = true;
				}
			}
			if (sum == 5)
			{
				if (shotTaker.penalties >= 85)
				{
					goalScored = true;
				}
				else if (shotTaker.penalties >= 70)
				{
					specSave = true;
				}
				else
				{
					divingSave = true;
				}
			}
			if (sum == 6)
			{
				if (shotTaker.penalties < 70)
				{
					divingSave = true;
				}
				else
				{
					goalScored = true;

				}
			}
			if (sum >= 7)
			{
				goalScored = true;
			}

			Player goalkeeper = this.DefendingTeam.goalKeeper;

			if (divingSave)
			{
				ShowMessage("Goalkeeper " + goalkeeper.fullName + " dives... and guesses right! What a wonderful save!");
			}
			else if (saved)
			{
				ShowMessage(shotTaker.fullName + " shoots... right at goalkeeper " + goalkeeper.fullName + ". Easy save!");
			}
			else if (whiff)
			{
				ballIsLive = false;
				ShowMessage(shotTaker.fullName + " shoots... it right over the bar! The pressure must have gotten to him! No goal!");
			}
			else if (specSave)
			{
				// this method already handles updating the goal count
				GoalChance(shotIndex, null, shotTaker, 6, true, OT);
			}
			else
			{
				if (!goalScored)
					ShowMessage("Something must've happened.");
			}
			if (goalScored)
			{
				ShowMessage(shotTaker.fullName + " shoots his penalty kick...");
				if (sum <= 6)
				{
					ShowMessage(this.DefendingTeam.goalKeeper.fullName + " gets a hand on it, but can't stop it! Goal!");
				}
				if (sum == 7)
				{
					ShowMessage(this.DefendingTeam.goalKeeper.fullName + " dives in one direction but the ball goes the other! Goal!");
				}
				if (sum == 8)
				{
					ShowMessage(this.DefendingTeam.goalKeeper.fullName + " guesses right but the ball was simply hit too hard and accurately into the upper corner of the net! Goal!");
				}
				if (sum == 9)
				{
					ShowMessage(this.DefendingTeam.goalKeeper.fullName + " guesses right but the ball was simply hit too hard and accurately into the lower corner of the net! Goal!");
				}
				if (sum == 10)
				{
					ShowMessage(this.DefendingTeam.goalKeeper.fullName + " guesses right but the ball was simply hit too hard and accurately past him into the net! Goal!");
				}
				if (sum == 11)
				{
					ShowMessage(this.DefendingTeam.goalKeeper.fullName + " leaps but can't reach it! It gets in just under the crossbar! Goal!");
				}
				if (sum == 12)
				{
					ShowMessage(this.DefendingTeam.goalKeeper.fullName + " guesses right but the ball was simply hit too hard and accurately past him into the inside of the post and in! Goal!");
				}
				this.PKScored = true;
				if (!OT) 
				{
					shotTaker.gameStats.penaltyKickGoals++;
					UpdateScore(null, shotTaker, this.DefendingTeam.goalKeeper, true); 
				}
			}
			else
			{
				if (!OT)
				{
					this.DefendingTeam.goalKeeper.gameStats.IncreaseSaves();
					AddPlayerRating(this.DefendingTeam.goalKeeper, 1);
				}
			}
			if (!OT) shotTaker.gameStats.penaltyKickAttempts++;
			ChangePossession(OT);
		}

		private void HandleFoulEvent(Player tackler, bool yellowCard, bool redCard, bool tackleMade)
		{
			//LogMessage("HandleFoulEvent");
			ballIsLive = false;
			if (tackleMade) ShowMessage("Vicious tackle by " + tackler.fullName);
			if (redCard)
			{
				HandleCards(tackler, false, true);
			}
			else if (yellowCard)
			{
				HandleCards(tackler, true, false);
			}
			else
			{
				tackler.gameStats.IncreaseFoulCount();
				ShowMessage("Certainly a foul. Looked like it warranted a yellow in my book but the referee apparently disagrees.");
			}
		}

		public void HandleCards(Player tackler, bool yellowCard, bool redCard)
		{
			ballIsLive = false;
			bool yellowCardStands = false;

			Enums.Ability playerCardedValue = Enums.Ability.Average;
			int d100Roll = Dice.Instance.d100.Roll();
			if ((d100Roll - tackler.aggression) < 0)
				playerCardedValue = Enums.Ability.AboveAverage;
			else if ((d100Roll - tackler.aggression) > 0)
			{
				d100Roll = Dice.Instance.d100.Roll();
				playerCardedValue = Enums.Ability.Average;
				if ((d100Roll - tackler.aggression) > 0)
					playerCardedValue = Enums.Ability.BelowAverage;
			}
			if (yellowCard)
			{
				if (referee.PlayerAlreadyBookedYellowCard(tackler))
				{
					if (playerCardedValue == Enums.Ability.AboveAverage)
					{
						yellowCardStands = true;
						ShowMessage("The referee appears pulls out another yellow card! And that's his second!");
						redCard = true;
					}
					else
					{
						ShowMessage("The referee appeared tempted but ultimately does not pull out another yellow card. That would've been his second...");
					}
					AddPlayerRating(tackler, -0.5);
				}
				else
				{
					yellowCardStands = true;
					ShowMessage("The referee appears to be pulling out a yellow to warn the player of his misdeed.");
					AddPlayerRating(tackler, -1);
					referee.AddPlayerToBookYellowCard(tackler);
				}
			}
			if (redCard)
			{
				ShowMessage("As a result of his actions, he'll receive a red card! He'll have to watch the rest of the match from the locker room!");
				AddPlayerRating(tackler, -2);
				referee.AddPlayerToBookRedCard(tackler);
				tackler.gamesOutDueToSuspension = 3;
			}
			if (yellowCardStands)
				gameSummary.Add("At minute: " + this.DetermineCurrentTime() + ", " + tackler.fullName + " was given a yellow card.");
			if (redCard)
				gameSummary.Add("At minute: " + this.DetermineCurrentTime() + ", " + tackler.fullName + " was given a red card and ejected.");
			tackler.gameStats.IncreaseFoulCount(yellowCardStands, redCard);
			string teamName = this.DefendingTeam.Name;
			if (!tackler.LeagueTeamSeasonStats[leagueName].ContainsKey(teamName))
			{
				teamName = this.AttackingTeam.Name;
			}
			int yellowCardCount = tackler.LeagueTeamSeasonStats[leagueName][teamName].yellowCards + tackler.gameStats.yellowCards;
			if (yellowCardRegs == Enums.YellowCardRegulations.EPL)
			{
				if (yellowCardCount > 0)
				{
					if (yellowCardCount % 5 == 0 || yellowCardCount % 8 == 0)
						tackler.gamesOutDueToSuspension = 1;
					if (yellowCardCount % 12 == 0)
						tackler.gamesOutDueToSuspension = 3;
				}
			}
			//modifier - because when the season advances a week, it reduces the number of games out due to injury. Hence, the modifier.
			if (tackler.gamesOutDueToSuspension > 1) tackler.gamesOutDueToSuspension += 1;
		}

		public void TakeOn(Enums.Characteristic characteristic, int whitedie, int blackdie)
		{
			ChangePlayers();

			Player attacker = this.AttackingTeam.onCamera[whitedie - 1];
			Player defender = this.DefendingTeam.onCamera[blackdie - 1];
			ClockTick(1);
			bool attackerWins = false;
			bool attackerOffField = false;
			bool defenderOffField = false;
			bool showFlavorText = true;
			if (PlayerIsOffField(attacker))
			{
				attackerOffField = true;
			}
			if (PlayerIsOffField(defender))
			{
				defenderOffField = true;
			}
			if (!attackerOffField && defenderOffField)
			{
				ShowMessage("Loose ball! But " + attacker.fullName + " recovers it and finds himself clear and on the attack!");
				attackerWins = true;
				showFlavorText = false;
			}
			else if (!defenderOffField && attackerOffField)
			{
				ShowMessage("Loose ball! But " + defender.fullName + " collects it and seizes possession!");
				attackerWins = false;
				showFlavorText = false;
			}
			else if (defenderOffField && attackerOffField)
			{
				ShowMessage("Loose ball! But a player on the defensive team collects it and seizes possession!");
				attackerWins = false;
				showFlavorText = false;
			}
			else
			{
				int competitiveValue = 50;
				int d100RollValue = Dice.Instance.d100.Roll();
				if (characteristic == Enums.Characteristic.pace)
					competitiveValue += (attacker.pace - defender.pace);
				else if (characteristic == Enums.Characteristic.dribbling)
					competitiveValue += (attacker.dribbling - defender.dribbling);
				else if (characteristic == Enums.Characteristic.agility)
					competitiveValue += (attacker.agility - defender.agility);
				else if (characteristic == Enums.Characteristic.isStrong)
					competitiveValue += (attacker.physicality - defender.physicality);
				else if (characteristic == Enums.Characteristic.passing)
					competitiveValue += (attacker.passing - defender.intercept);
				else
					competitiveValue += (attacker.balance - defender.balance);
				if (competitiveValue < 10)
					competitiveValue = 10;
				if (competitiveValue > 90)
					competitiveValue = 90;
				if (d100RollValue < competitiveValue)
					attackerWins = true;
				else
					attackerWins = false;
			}

			if (showFlavorText) TakeOnFlavorText(attacker, defender, attackerWins, characteristic);
			if (attackerWins)
			{
				AddPlayerRating(attacker);
				if (!defenderOffField) AddPlayerRating(defender, -0.5);
				Attack(whitedie, blackdie, false);
			}
			else
			{
				AddPlayerRating(defender);
				if (!attackerOffField) AddPlayerRating(attacker, -0.5);
				ChangePossession();
			}
		}

		public void TakeOnFlavorText(Player attacker, Player defender, bool attackerWins, Enums.Characteristic characteristic)
		{
			if (characteristic == Enums.Characteristic.pace)
			{
				ShowMessage(attacker.fullName + " makes a swift move and tries to use his speed to get past " + defender.fullName);
				if (attackerWins)
				{
					ShowMessage(attacker.fullName + " blows right by " + defender.fullName + " and continues to run just ahead of him!");
				}
				else
				{
					ShowMessage(defender.fullName + " reads the movement, though, and is able to take the ball away! Eliminates a scoring chance for " + attacker.fullName);
				}
			}
			if (characteristic == Enums.Characteristic.dribbling)
			{
				ShowMessage(attacker.fullName + " kicks the ball to his side trying to dribble past " + defender.fullName);
				if (attackerWins)
				{
					ShowMessage(defender.fullName + " reaches for it but misses! " + attacker.fullName + " is now free thanks to his deft move and his team has a scoring chance!");
				}
				else
				{
					ShowMessage(defender.fullName + " gets to the ball first, though, and is able to take the ball away!");
				}
			}
			if (characteristic == Enums.Characteristic.isStrong)
			{
				ShowMessage(attacker.fullName + " lowers his shoulder and tries to blow by " + defender.fullName);
				if (attackerWins)
				{
					ShowMessage(defender.fullName + " takes a step back which allows " + attacker.fullName + " to blow right by him. Now, he has a scoring chance!");
				}
				else
				{
					ShowMessage(defender.fullName + " is like a wall this time around. The ball skirts free and he collects it!");
				}
			}
			if (characteristic == Enums.Characteristic.isHard)
			{
				ShowMessage(attacker.fullName + " makes a bull-headed charge into " + defender.fullName + "'s chest.");
				if (attackerWins)
				{
					ShowMessage(defender.fullName + " takes a step back which allows " + attacker.fullName + " to blow right by him. Now, he has a scoring chance!");
				}
				else
				{
					ShowMessage(defender.fullName + " is like a wall this time around. The ball skirts free and he collects it!");
				}
			}
			if (characteristic == Enums.Characteristic.passing || characteristic == Enums.Characteristic.agility)
			{
				ShowMessage(attacker.fullName + " passes to a teammate who passes it right back dangerously close to " + defender.fullName);
				if (attackerWins)
				{
					ShowMessage(attacker.fullName + " manages to get it, though, and moves swiftly past " + defender.fullName + ". Now, he has a scoring chance!");
				}
				else
				{
					ShowMessage(defender.fullName + " sticks his foot out and makes an excellent steal.");
				}
			}
		}

		public void MidfieldBattleOrBuildupFlavorText(Enums.Characteristic characteristic, bool attackerWins, bool buildup)
		{
			ShowMessage("A series of quick passes back and forth at midfield!");
			ShowMessage("A through ball out in the open! Both teams run for it!");
			Team winnerOfBattle = AttackingTeam;
			string s = "";
			if (!attackerWins)
			{
				winnerOfBattle = DefendingTeam;
			}
			if (characteristic == Enums.Characteristic.pace)
			{
				s = winnerOfBattle.Name + " get to it first";
			}
			if (characteristic == Enums.Characteristic.dribbling)
			{
				s = "One of the players on " + winnerOfBattle.Name + " is able to get a foot on it first and knocks it to a teammate...";
			}
			if (characteristic == Enums.Characteristic.isStrong)
			{
				s = "Both teams get into a shoving match which leads to one of the players on " + winnerOfBattle.Name + " being able to get a foot on it first. He knocks it to a teammate...";
			}
			if (characteristic == Enums.Characteristic.isHard)
			{
				s = "Both teams get into a heated shoving match which leads to one of the players on " + winnerOfBattle.Name + " being able to get a foot on it first. He knocks it to a teammate...";
			}
			if (characteristic == Enums.Characteristic.passing || characteristic == Enums.Characteristic.agility)
			{
				s = "Two players on " + winnerOfBattle.Name + " perform sliding tackles at just the right time and are able to pass it to each other as their opponents stand slack-jawed";
			}

			if (attackerWins)
			{
				s += " and now they have a great scoring chance!";
			}
			else
			{
				s += " and by doing so win possession of the ball!";
			}
			ShowMessage(s);
		}

		public void MidfieldBattleOrBuildup(Enums.Characteristic characteristic, int greendie, int whitedie, int blackdie, bool buildup)
		{
			//LogMessage("MidfieldBattleOrBuildup");
			changedPossessionAtLeastOnceOrMidfieldBattle = true;
			ChangePlayers();
			bool attackerWins = DetermineTeamCharacteristicWinner(characteristic, buildup, greendie);
			MidfieldBattleOrBuildupFlavorText(characteristic, attackerWins, buildup);
			if (attackerWins)
			{
				Attack(whitedie, blackdie, false);
			}
			else
			{
				ChangePossession();
			}
		}

		public bool DetermineTeamCharacteristicWinner(Enums.Characteristic characteristic, bool buildup, int greendie)
		{
			double attackingValue = 0;
			double defendingValue = 0;
			double competitiveValue = 50;
			int d100RollValue = Dice.Instance.d100.Roll();
			ClockTick(1);
			foreach (Player p in this.AttackingTeam.onCamera)
			{
				if (characteristic == Enums.Characteristic.pace)
					attackingValue += p.pace;
				else if (characteristic == Enums.Characteristic.dribbling)
					attackingValue += p.dribbling;
				else if (characteristic == Enums.Characteristic.agility)
					attackingValue += p.agility;
				else if (characteristic == Enums.Characteristic.isStrong)
					attackingValue += p.physicality;
				else if (characteristic == Enums.Characteristic.passing)
					attackingValue += p.passing;
				else
					attackingValue += p.balance;
			}

			attackingValue /= 5;

			foreach (Player p in this.DefendingTeam.onCamera)
			{
				if (characteristic == Enums.Characteristic.pace)
					defendingValue += p.pace;
				else if (characteristic == Enums.Characteristic.dribbling)
					defendingValue += p.dribbling;
				else if (characteristic == Enums.Characteristic.agility)
					defendingValue += p.agility;
				else if (characteristic == Enums.Characteristic.isStrong)
					defendingValue += p.physicality;
				else if (characteristic == Enums.Characteristic.passing)
					defendingValue += p.intercept;
				else
					defendingValue += p.balance;
			}

			defendingValue /= 5;

			if (buildup)
			{
			}
			else
			{
				//ClockTick(greendie);
			}
			ClockTick(1);

			competitiveValue += (attackingValue - defendingValue);

			if (competitiveValue < 10)
				competitiveValue = 10;
			if (competitiveValue > 90)
				competitiveValue = 90;

			bool attackingTeamwins = false;
			if (d100RollValue < competitiveValue)
				attackingTeamwins = true;
			else
				attackingTeamwins = false;

			return attackingTeamwins;
		}

		public void SidelineBattle(int greendie, int whiteDieVal, int blackDieVal)
		{
			ShowMessage("Play moves toward the sidelines as the offense tries to move and the defense tries to apply pressure.");
			ShowMessage("It's followed by a quick series of passes and some deft dribble moves.");
			ChangePlayers();
			ClockTick(1);
			//if (whiteDieVal == 3 || blackDieVal == 3)
			//{
			//    ClockTick(3);
			//}
			Team possessingTeam = this.AttackingTeam;

			ShowMessage("A foot has kicked the ball out of bounds.");

			if (greendie == 1)
			{
				if (this.attackingTeamIsHomeTeam)
				{
					possessingTeam = this.DefendingTeam;
				}
			}
			if (greendie >= 2 && greendie <= 4)
			{
				double attackingTeamCircles = 0;
				double defendingTeamCircles = 0;

				foreach (Player p in this.AttackingTeam.onCamera)
				{
					if (!PlayerIsOffField(p))
					{
						attackingTeamCircles += p.acceleration;
					}
				}
				attackingTeamCircles /= 5;
				foreach (Player p in this.DefendingTeam.onCamera)
				{
					if (!PlayerIsOffField(p))
					{
						defendingTeamCircles += p.acceleration;
					}
				}
				defendingTeamCircles /= 5;
				if (defendingTeamCircles > attackingTeamCircles)
				{
					possessingTeam = this.DefendingTeam;
				}
			}
			if (greendie == 5)
			{
				if (homeScore > awayScore)
				{
					possessingTeam = this.DefendingTeam;
				}
			}

			if (greendie == 6)
			{
				if (!this.attackingTeamIsHomeTeam)
				{
					possessingTeam = this.DefendingTeam;
				}
			}

			if (OnPitchCount(AttackingTeam) > OnPitchCount(DefendingTeam))
			{
				possessingTeam = this.AttackingTeam;
			}
			else if (OnPitchCount(DefendingTeam) > OnPitchCount(AttackingTeam))
			{
				possessingTeam = this.DefendingTeam;
			}

			if (possessingTeam == this.DefendingTeam)
			{
				ChangePossession();
			}

			ShowMessage("Referee says it was last touched by " + DefendingTeam.Name+ ".");
			ShowMessage(possessingTeam.Name + " will thusly throw the ball back in.");
		}

		public void Tackle()
		{
			//LogMessage("Tackle");
			ChangePlayers();
			ClockTick(1);
			int greendie = Dice.Instance.d6.Roll();
			if (greendie == 6) greendie--;
			Player tackler = FindPlayerWhenNoMatter(this.DefendingTeam.onCamera[greendie - 1], this.DefendingTeam.onCamera, (greendie - 1));
			if (defenseSucceedsAsTeam(this.DefendingTeam))
			{
				ShowMessage(tackler.fullName + " with an excellent tackle taking possession back to his team.");
				AddPlayerRating(tackler);
				ChangePossession();
			}
			else
			{
				ShowMessage(tackler.fullName +"'s tackle is deftly avoided.");
			}
		}

		public void CornerKick()
		{
			this.AttackingTeam.gameStats.cornerKicks++;
			ShowMessage("Corner kick is delivered...");
			ChangePlayers();
			int greenDieVal = Dice.Instance.d6.Roll();
			int whiteDieVal = Dice.Instance.d6.Roll();
			int blackDieVal = Dice.Instance.d6.Roll();
			int sum = whiteDieVal + blackDieVal;
			ClockTick(1);

			if (sum == 2)
			{
				if (greenDieVal > 5) greenDieVal = 1;
				Player assister = this.FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[greenDieVal - 1], this.AttackingTeam.onCamera, greenDieVal - 1);
				ShowMessage(assister.fullName + " with the corner...");
				if (greenDieVal > 5) greenDieVal = 1;
				greenDieVal++;
				if (greenDieVal > 5) greenDieVal = 1;
				Player shooter = this.FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[greenDieVal - 1], this.AttackingTeam.onCamera, greenDieVal - 1);
				int count = 0;
				while (shooter == assister && count < 5)
				{
					count++;
					greenDieVal++;
					if (greenDieVal > 5) greenDieVal = 1;
					shooter = this.FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[greenDieVal - 1], this.AttackingTeam.onCamera, greenDieVal - 1);
				}
				ShowMessage("And " + shooter.fullName + " collects it in front of the net...");
				GoalChance(greenDieVal, assister, shooter, 6, true);
			}
			if (sum == 3)
			{
				HighlightReel("CK");
			}
			if (sum == 4)
			{
				ShowMessage("Possibilities...!");
				Attack(whiteDieVal, blackDieVal, true);
			}
			if (sum == 5)
			{
				RefereeDecision("K");
			}
			if (sum >= 6)
			{
				ShowMessage("The corner kick goes awry and we're going to have to reset.");

				DeterminePitchAction();
			}
		}

		public void CounterAttack(int whiteDie, int blackDie)
		{
			ClockTick(1);
			int greendie = Dice.Instance.d6.Roll();
			if (greendie == 6) greendie = 1;
			Player attacker = FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[greendie - 1], this.AttackingTeam.onCamera, (greendie - 1));
			if (defenseSucceedsAsTeam(this.DefendingTeam))
			{
				ShowMessage(attacker.fullName + " tries to brute force his way through the pack!");
				ShowMessage("A foot gets in his way, though, and forces the ball loose! A defender gets to it first leaving the attacking team flat footed!");
				ChangePossession();
				AddPlayerRating(attacker, -0.5);
			}
			else
			{
				ShowMessage(attacker.fullName + " tries to brute force his way through the pack!");
				ShowMessage("And is through! I think there may be a scoring chance on the horizon!");
				AddPlayerRating(attacker);
				Attack(whiteDie, blackDie, false);
			}
		}

		// TO DO: Improve;
		private bool defenseSucceedsAsTeam(Team defendingTeam)
		{
			//LogMessage("numberOfPlayersWithSquares");
			int numberOfSquares = 0;

			foreach (Player p in defendingTeam.onCamera)
			{
				if (!PlayerIsOffField(p))
				{
					numberOfSquares += p.defending;
				}
			}

			int d100RollValue = Dice.Instance.d100.Roll();

			return (d100RollValue < (numberOfSquares /5));
		}

		private void PenaltyKickOT()
		{
			homePKScore = 0;
			awayPKScore = 0;
			halfNumber = 5;
			currentPKRound = 1;
			NotifyHalfChanged();

			this.AttackingTeam = this.homeTeam;
			this.DefendingTeam = this.awayTeam;

			List<Player> homeOrder = new List<Player>();
			List<Player> awayOrder = new List<Player>();

			for (int i = 0; i < this.homeTeam.onCamera.Length; i++)
			{
				if (!PlayerIsOffField(this.homeTeam.onCamera[i]))
				{
					homeOrder.Add(this.homeTeam.onCamera[i]);
				}
			}

			for (int i = 0; i < this.homeTeam.offCamera.Length; i++)
			{
				if (!PlayerIsOffField(this.homeTeam.offCamera[i]))
				{
					homeOrder.Add(this.homeTeam.offCamera[i]);
				}
			}

			for (int i = 0; i < this.awayTeam.onCamera.Length; i++)
			{
				if (!PlayerIsOffField(this.awayTeam.onCamera[i]))
				{
					awayOrder.Add(this.awayTeam.onCamera[i]);
				}
			}

			for (int i = 0; i < this.awayTeam.offCamera.Length; i++)
			{
				if (!PlayerIsOffField(this.awayTeam.offCamera[i]))
				{
					awayOrder.Add(this.awayTeam.offCamera[i]);
				}
			}

			int totalRounds = 5;
			int homeAttemptsRemaining = 5;
			int awayAttempstRemaining = 5;
			bool pKOver = false;

			int awayShooterIndex = 0;
			int homeShooterIndex = 0;

			bool homeAttempt = false;

			while (!pKOver)
			{
				if (awayShooterIndex >= awayOrder.Count)
					awayShooterIndex = 0;
				if (homeShooterIndex >= homeOrder.Count)
					homeShooterIndex = 0;
				ShowMessage("Current round: " + currentPKRound);
				this.PKScored = false;
				homeAttempt = !homeAttempt;
				Player kickTaker = null;
				if (homeAttempt)
				{
					kickTaker = homeOrder[homeShooterIndex];
					homeAttemptsRemaining--;
				}
				else
				{
					kickTaker = awayOrder[awayShooterIndex];
					awayAttempstRemaining--;
				}
				PenaltyKick(kickTaker, true);

				if (this.PKScored)
				{
					if (homeAttempt)
					{
						homePKScore++;
					}
					else
					{
						awayPKScore++;
					}
					NotifyScoreChange();
				}
				if (!homeAttempt)
				{
					currentPKRound++;
					awayShooterIndex++;
					NotifyHalfChanged();
				}
				else
				{
					homeShooterIndex++;
				}
				pKOver = (homePKScore > (awayAttempstRemaining + awayPKScore)) || (awayPKScore > (homeAttemptsRemaining + homePKScore)) ||
					((currentPKRound > totalRounds) && (homePKScore != awayPKScore) && (homeAttemptsRemaining == 0) &&
					(awayAttempstRemaining == 0));
				if (!pKOver)
				{
					if (currentPKRound > 5)
					{
						if (homeAttemptsRemaining == 0 && awayAttempstRemaining == 0)
						{
							homeAttemptsRemaining = 1;
							awayAttempstRemaining = 1;
						}
					}
					ShowMessage("Current PK Score: " + homeTeam.Name + ", " + homePKScore + " - " + awayTeam.Name + ", " + awayPKScore);
				}
			}

			if (homePKScore > awayPKScore)
			{
				ShowMessage(homeTeam.Name + " wins on penalty kicks!");
				GameResult = RecordGameResult(homeTeam, GetHomeScore() + " (" + homePKScore.ToString() + ")", awayTeam,
					GetAwayScore() + " (" + awayPKScore.ToString() + ")", true);
			}
			else if (awayPKScore > homePKScore)
			{
				ShowMessage(awayTeam.Name + " wins on penalty kicks!");
				GameResult = RecordGameResult(awayTeam, GetHomeScore() + " (" + homePKScore.ToString() + ")", homeTeam,
					GetAwayScore() + " (" + awayPKScore.ToString() + ")", true);
			}

			//ShowMessage("Final score:", true);
			//ShowMessage(homeTeam.Name + ", " + homeScore + " (" + homePKScore + ") - " + 
			//    awayTeam.Name + ", " + awayScore + " (" + awayPKScore + ")", true);
			//ShowGoalTraditionalSummary();
			//ShowPlayerRatings();
			//ShowPlayerScoredSummary();

			gameIsGoing = false;
		}

		private void CheckCleanSheets()
		{
			if (awayScore == 0)
			{
				this.homeTeam.goalKeeper.gameStats.cleansheets++;
			}
			if (homeScore == 0)
			{
				this.awayTeam.goalKeeper.gameStats.cleansheets++;
			}
		}

		private bool PlayerIsOffField(Player playerInQuestion)
		{
			return (referee.PlayerAlreadyBookedRedCard(playerInQuestion) || playerInQuestion.gamesOutDueToInjury > 0);
		}

		private int OnPitchCount(Team team)
		{
			int numberOnPitch = 0;
			foreach (Player p in team.onCamera)
			{
				if (!PlayerIsOffField(p))
					numberOnPitch++;
			}
			foreach (Player p in team.offCamera)
			{
				if (!PlayerIsOffField(p))
					numberOnPitch++;
			}
			if (!PlayerIsOffField(team.goalKeeper))
				numberOnPitch++;
			return numberOnPitch;
		}

		private void HandleMissingPlayerFlavorTextAndChangePossession()
		{
			ballIsLive = false;
			ClockTick(1);
			ShowMessage(this.AttackingTeam.Name + " misplays the ball, though, and it goes out of bounds! That'll be a thrown in for " + this.DefendingTeam.Name);
			ChangePossession();
		}

		private Player FindPlayerWhenNoMatter(Player playerInQuestion, Player[] lineup, int currentIndex)
		{
			if (PlayerIsOffField(playerInQuestion))
			{
				int count = 0;
				Player replacementPlayer = playerInQuestion;
				while (PlayerIsOffField(replacementPlayer) || count > 5)
				{
					count++;
					currentIndex++;
					if (currentIndex > 4)
						currentIndex = 0;
					replacementPlayer = lineup[currentIndex];
				}
				return replacementPlayer;
			}
			else
			{
				return playerInQuestion;
			}
		}

		private bool PositionsCompatible(Player playerA, Player playerB)
		{
			return (PlayerIsForwardOrCAM(playerA) && PlayerIsForwardOrCAM(playerB))
				|| (PlayerIsMidfielder(playerA) && PlayerIsMidfielder(playerB))
				|| (PlayerIsDefender(playerA) && PlayerIsDefender(playerB));
		}

		private bool PlayerIsForwardOrCAM(Player playerA)
		{
			return playerA.Position == Enums.Positions.LeftWingForward ||
				playerA.Position == Enums.Positions.RightWingForward ||
				playerA.Position == Enums.Positions.Striker || playerA.Position == Enums.Positions.CentralAttackingMidfielder;
		}

		private bool PlayerIsMidfielder(Player playerA)
		{
			return playerA.Position == Enums.Positions.CentralAttackingMidfielder ||
				playerA.Position == Enums.Positions.LeftMidfielder ||
				playerA.Position == Enums.Positions.RightMidfielder || playerA.Position == Enums.Positions.CentralDefendingMidfielder ||
				playerA.Position == Enums.Positions.CentralMidfielder;
		}

		private bool PlayerIsDefender(Player playerA)
		{
			return playerA.Position == Enums.Positions.LeftBack ||
				playerA.Position == Enums.Positions.RightBack ||
				playerA.Position == Enums.Positions.CenterBack || playerA.Position == Enums.Positions.CentralDefendingMidfielder;
		}

		private void SubstitutePlayer(int playerIndex, Team team)
		{
			if (playerIndex == 5)
				playerIndex = 4;
			bool homeTeamSub = false;

			if (team == this.homeTeam)
			{
				homeTeamSub = true;
				if (numberOfHomeSubsLeft > 0)
				{
					numberOfHomeSubsLeft--;
				}
				else
				{
					return;
				}
			}
			else
			{
				if (numberOfAwaySubsLeft > 0)
				{
					numberOfAwaySubsLeft--;
				}
				else
				{
					return;
				}
			}

			Player playerToSubOut = team.onCamera[playerIndex];
			int indexToSwapIn = -1;
			for (int i = 0; i < team.bench.Count - 1; i++)
			{
				if (!homeSubs.Contains(team.bench[i]) && !awaySubs.Contains(team.bench[i]))
				{
					indexToSwapIn = i;
					if (PositionsCompatible(team.bench[i], playerToSubOut))
					{
						break;
					}
				}
			}
			if (indexToSwapIn > -1)
			{
				Player playerToSubIn = team.bench[indexToSwapIn];
				team.bench[indexToSwapIn] = playerToSubOut;
				team.onCamera[playerIndex] = playerToSubIn;
				if (homeTeamSub)
					homeSubs.Add(playerToSubOut);
				else
					awaySubs.Add(playerToSubOut);

				playerToSubIn.LeagueTeamSeasonStats[leagueName][team.Name].matchesPlayed++;
				NotifyOnCamera();
				ShowMessage("And during the stoppage of play, we're going to have a substitution. " + team.Name + " will sub out " + playerToSubIn.fullName + " for " + playerToSubOut.fullName + ".");
				gameSummary.Add("At minute: " + this.DetermineCurrentTime() + ", " + team.Name + " subbed in " + playerToSubIn.fullName + " for " + playerToSubOut.fullName + ".");
			}
		}

		private void AddGKRating(Player player, double rating = 0.5)
		{
			if (player.gameStats.Saves == 2 || player.gameStats.Saves == 4 || player.gameStats.Saves >= 6)
				player.gameStats.playerRating += rating; 
		}

		private void AddPlayerRating(Player player, double rating = 0.5, bool assister = false, bool goalScorer = false)
		{
			if (assister)
			{
				if (player.gameStats.assists > 0) player.gameStats.playerRating += 0.5; else player.gameStats.playerRating += 1;
			}
			else if (goalScorer)
			{
				if (player.gameStats.goals > 0) player.gameStats.playerRating += 1; else player.gameStats.playerRating += 1.5;
			}
			else
			{
				player.gameStats.playerRating += rating;
			}
			if (player.gameStats.playerRating > maxRating)
				player.gameStats.playerRating = maxRating;
			double lowRating = 3;
			if (player.Position == Enums.Positions.Goalkeeper)
			{
				if (player.gameStats.goalsConceded <= 2)
					lowRating = 4;
				else if (player.gameStats.goalsConceded > 2 && player.gameStats.goalsConceded < 5)
					lowRating = 3.5;
			}
			if (player.gameStats.playerRating < lowRating)
			{
				player.gameStats.playerRating = lowRating;
			}
		}

		//private void ShowPlayerRatings()
		//{
		//    ShowMessage(homeTeam.Name + " Game ratings:", true);
		//    ShowMessage(homeTeam.goalKeeper.fullName + ": " + homeTeam.goalKeeper.gameStats.playerRating.ToString("F1"), true);
		//    foreach (Player p in homeSubs)
		//    {
		//        ShowMessage(p.fullName + ": " + p.gameStats.playerRating.ToString("F1"), true);
		//    }
		//    foreach (Player p in homeTeam.onCamera)
		//    {
		//        ShowMessage(p.fullName + ": " + p.gameStats.playerRating.ToString("F1"), true);
		//    }
		//    foreach (Player p in homeTeam.offCamera)
		//    {
		//        ShowMessage(p.fullName + ": " + p.gameStats.playerRating.ToString("F1"), true);
		//    }
		//    ShowMessage(awayTeam.Name + " Game ratings:", true);
		//    ShowMessage(awayTeam.goalKeeper.fullName + ": " + awayTeam.goalKeeper.gameStats.playerRating.ToString("F1"), true);
		//    foreach (Player p in awaySubs)
		//    {
		//        ShowMessage(p.fullName + ": " + p.gameStats.playerRating.ToString("F1"), true);
		//    }
		//    foreach (Player p in awayTeam.onCamera)
		//    {
		//        ShowMessage(p.fullName + ": " + p.gameStats.playerRating.ToString("F1"), true);
		//    }
		//    foreach (Player p in awayTeam.offCamera)
		//    {
		//        ShowMessage(p.fullName + ": " + p.gameStats.playerRating.ToString("F1"), true);
		//    }
		//}

		//private void ShowPlayerScoredSummary()
		//{
		//    foreach (string s in gameSummary)
		//    {
		//        ShowMessage(s, true);
		//    }
		//}

		public void RareResult()
		{
			ClockTick(1);
			int sum = Dice.Instance.d6.Roll() + Dice.Instance.d6.Roll();

			if (sum == 2)
			{
				ShowMessage(this.DefendingTeam.goalKeeper.fullName + " runs out of the box to make the save!");
				ShowMessage("Nasty collission and the ball remains loose!");
				ShowMessage("Whistle blows, however. The referee jumps into the fray. Unbelievable! He's calling foul on the goalkeeper!");
				ShowMessage("A penalty kick for " + this.AttackingTeam.Name + " due to a keeper foul! I don't believe it!");
				PenaltyKick();
			}
			if (sum == 3)
			{
				UberRareHighlightReel();
			}
			if (sum == 4 || sum == 5)
			{
				BChart();
			}
			if (sum == 6 || sum == 8)
			{
				int greendie = Dice.Instance.d6.Roll();
				if (greendie == 6) greendie = 1;
				Player shooter = FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[greendie - 1], this.AttackingTeam.onCamera, (greendie - 1));
				ShowMessage(shooter.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
				ShowMessage(shooter.fullName + " takes a shot, but it is deflected off a defender.");
				greendie = Dice.Instance.d6.Roll();
				if (greendie == 6) greendie = 1;
				Player defender = FindPlayerWhenNoMatter(this.DefendingTeam.onCamera[greendie - 1], this.DefendingTeam.onCamera, (greendie - 1));
				ShowMessage(defender.fullName + " collects the ball... No wait, it goes off his foot and backward at an awkward angle!");
				ShowMessage(this.DefendingTeam.goalKeeper.fullName + " lunges for it but he's too late! It's an own goal!");
				ShowMessage(defender.fullName + " is lying on the turf in complete despair as teammates try to comfort him!");
				this.UpdateScore(null, null, this.DefendingTeam.goalKeeper, false);
				ChangePossession();
			}
			if (sum == 7)
			{
				int greendie = Dice.Instance.d6.Roll();
				Enums.Characteristic characteristic = (Enums.Characteristic)greendie;
				if (!DetermineTeamCharacteristicWinner(characteristic, false, 0))
				{
					ChangePossession();
				}
				AChart();
			}
			if (sum == 9)
			{
				if (!AttackingTeamMostCircles())
					ChangePossession();
				AChart();
			}
			if (sum == 10 || sum == 1)
			{
				int greendie = Dice.Instance.d6.Roll();
				if (greendie == 6) greendie = 1;

				Player attacker = FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[greendie - 1], this.AttackingTeam.onCamera, (greendie - 1));

				if (sum == 10)
				{
					ShowMessage("Loose ball just outside the box! " + this.DefendingTeam.goalKeeper.fullName + " dashes after it!");
					ShowMessage(attacker.fullName + " sprints toward it as well!");
					ShowMessage(attacker.fullName + " gets to it first and takes a shot at the open net!");
				}
				else
				{
					int defenderIndex = Dice.Instance.d6.Roll();
					if (defenderIndex == 6) defenderIndex = 5;
					this.LongDistanceShotText(attacker, FindPlayerWhenNoMatter(this.DefendingTeam.onCamera[defenderIndex - 1], this.DefendingTeam.onCamera, (defenderIndex - 1)), attacker);
				}

				int blackDieVal = Dice.Instance.d100.Roll();
				bool goalScored = false;

				if ((blackDieVal < attacker.shooting))
				{
					if (sum == 10)
					{
						ShowMessage("And the ball gets in! An easier goal won't be found! Oh, what a disaster for " + this.DefendingTeam.goalKeeper.fullName + ".");
					}
					else
					{
						ShowMessage("And the long distance shot makes it in! " + this.DefendingTeam.goalKeeper.fullName + " still hadn't moved from the initial shot attempt which left him vulnerable!");
						ShowMessage("And needless to say, " + attacker.fullName + " capitalized!");
					}
					goalScored = true;
					UpdateScore(null, attacker, this.DefendingTeam.goalKeeper);
				}
				else
				{
					if (sum == 10)
					{
						ShowMessage("But the ball goes wide! An easier goal won't be found yet he missed! " + this.DefendingTeam.goalKeeper.fullName + " cannot stop smiling, relieved his error goes unpunished.");
						ShowMessage("In contrast, " + attacker.fullName + " is almost double-overred in disbelief that he missed such an easy goal.");
					}
					else
					{
						ShowMessage("And the long distance shot is wide! " + this.DefendingTeam.goalKeeper.fullName + " still hadn't moved from the initial shot attempt which left him vulnerable!");
						ShowMessage("But " + attacker.fullName + " just couldn't quite capitalize!");
					}
				}

				attacker.gameStats.IncreaseShotOnGoal(goalScored);
				ChangePossession();
			}
			if (sum == 12)
			{
				BChart();
			}
		}

		private void AChart()
		{
			int sum = Dice.Instance.d6.Roll() + Dice.Instance.d6.Roll();
			int greendie = Dice.Instance.d6.Roll();
			if (greendie == 6) greendie = 1;

			Player attacker = FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[greendie - 1], this.AttackingTeam.onCamera, (greendie - 1));
			if (sum == 2 || sum == 12)
			{
				GoalkeeperDribblesBallOutsideBox();
				BeatGKOverheadKick();
				if (greendie == 6) greendie = 1;
				ShowMessage("Loose ball just outside the box! " + this.DefendingTeam.goalKeeper.fullName + " dashes after it!");
				ShowMessage(attacker.fullName + " sprints toward it as well!");
				ShowMessage("Both men get to it at the same time and the ball is in the air above them!");
				ShowMessage("Before the keeper can react, " + attacker.fullName + " leaps in the air and attempts an overhead kick!");
				int blackDieVal = Dice.Instance.d6.Roll();
				bool goalScored = false;

				if ((blackDieVal < attacker.shooting))
				{
					goalScored = true;
					UpdateScore(null, attacker, this.DefendingTeam.goalKeeper);
				}
				else
				{
					ShowMessage("And it's just wide of the net! Great effort, fun to see, but ultimately does not result in a goal.");
				}
				attacker.gameStats.IncreaseShotOnGoal(goalScored);
				ChangePossession();
			}
			if (sum == 3)
			{
				GoalkeeperDribblesBallOutsideBox();
				GoalKeeperScores(attacker);
				ChangePossession();
				this.AttackingTeam.goalKeeper.gameStats.IncreaseShotOnGoal();
				UpdateScore(null, this.AttackingTeam.goalKeeper, this.DefendingTeam.goalKeeper);
				ChangePossession();
			}
			if (sum == 4)
			{
				ShowMessage(attacker.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
				ShowMessage(attacker.fullName + " takes a shot!");
				ShowMessage("Off the crossbar and In!");
				ShowMessage("No, wait! Players on " + this.DefendingTeam.Name + " are arguing that the ball never quite completely crossed the line.");
				ShowMessage("The referee seems to agree it is worth a look. He'll check with VARS to see if the goal stands.");
				ShowMessage("The referee reviews...");
				ShowMessage("And now the review is over...");
				Enums.RefAttitude disposition = Enums.RefAttitude.Friendly;
				if (homeTeam == this.AttackingTeam)
				{
					disposition = referee.attitudeToHomeTeam;
				}
				else
				{
					disposition = referee.attitudeToVisitingTeam;
				}
				bool goalStands = false;
				if (disposition == Enums.RefAttitude.Friendly)
				{
					goalStands = true;
				}
				else if (disposition == Enums.RefAttitude.Neutral)
				{
					if (Dice.Instance.d6.Roll() > 3)
						goalStands = true;
				}
				if (goalStands)
				{
					ShowMessage("And the goal will stand! Players on" + this.DefendingTeam.Name + " still are arguing but it will not matter. The goal will stand!");
					UpdateScore(null, attacker, this.DefendingTeam.goalKeeper);
				}
				else
				{
					ShowMessage("And the goal has been waived off! As players on" + this.DefendingTeam.Name + " celebrate, players on " + this.AttackingTeam.Name + " argue vehemently.");
					ShowMessage("But it will not matter. The goal does not count! And " + this.DefendingTeam.Name + " will restart play with an indirect free kick.");
				}
				this.AttackingTeam.goalKeeper.gameStats.IncreaseShotOnGoal(goalStands);
				ChangePossession();
				//shot attempt. Flavor text go to VARS but underneath the hood, it depends on referee friendliness
			}
			if (sum == 5)
			{          
				greendie = Dice.Instance.d6.Roll();
				if (greendie == 6) greendie = 1;
				Player tackler = FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[greendie - 1], this.AttackingTeam.onCamera, (greendie - 1));
				ShowMessage("Vicious tackle by " + tackler.fullName + ". He tackled " + attacker.fullName + " needlessly from behind...");
				HandleCards(tackler, false, true);
				ShowMessage("A well deserved red if you ask me. Now, with that taken care of, play restarts.");
				ShowMessage("A free kick will be granted but not anywhere near enough to the opposition goal to be dangerous. He'll just pass it from here I'm sure.");
				ChangePossession();
			}
			if (sum == 6)
			{
				// the mechanisms might be set up for this one too. o1 misses, but o2 makes a long range shot.
				if (PlayerIsOffField(this.AttackingTeam.onCamera[1]) || PlayerIsOffField(this.AttackingTeam.onCamera[0]))
				{
					HandleMissingPlayerFlavorTextAndChangePossession();
					return;
				}
				int defenderIndex = Dice.Instance.d6.Roll();
				if (defenderIndex == 6) defenderIndex = 5;
				LongDistanceShotText(attacker, FindPlayerWhenNoMatter(this.DefendingTeam.onCamera[defenderIndex - 1], this.DefendingTeam.onCamera, (defenderIndex - 1)), this.AttackingTeam.onCamera[1]);
				LongDistanceShotAttempt(this.AttackingTeam.onCamera[1]);
			}
			if (sum == 7)
			{
				int defenderIndex = Dice.Instance.d6.Roll();
				if (defenderIndex == 6) defenderIndex = 5;
				LongDistanceShotText(attacker, FindPlayerWhenNoMatter(this.DefendingTeam.onCamera[defenderIndex - 1], this.DefendingTeam.onCamera, (defenderIndex - 1)), attacker);
				LongDistanceShotAttempt(attacker);
			}
			if (sum == 8 || sum == 9)
			{
				GoalkeeperDribblesBallOutsideBox();
				BeatGKShootEmptyNet();
				int maxValue = 3;
				bool goalSavedByHeader = false;
				int playerIndex = 4;
				if (sum == 9)
					playerIndex = 3;
				if (!PlayerIsOffField(this.DefendingTeam.onCamera[playerIndex]))
				{
					if (Dice.Instance.d6.Roll() <= maxValue)
					{
						goalSavedByHeader = true;
					}
				}

				if (goalSavedByHeader)
				{
					HeadsBallAway(this.DefendingTeam.onCamera[playerIndex]);
				}
				else
				{
					EmptyNetGoal();
					UpdateScore(null, attacker, this.DefendingTeam.goalKeeper);
				}
				attacker.gameStats.IncreaseShotOnGoal(!goalSavedByHeader);
			}
			if (sum == 10)
			{
				ShowMessage(attacker.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
				ShowMessage(attacker.fullName + " takes a shot!");
				ShowMessage("Off the crossbar and Out!");
				attacker.gameStats.IncreaseShotOnGoal(false);
				// the mechanisms might be set up for this one too. o1 misses, but o2 makes a long range shot.
				if (PlayerIsOffField(this.AttackingTeam.onCamera[1]))
				{
					HandleMissingPlayerFlavorTextAndChangePossession();
					return;
				}
				Player rebounder = this.AttackingTeam.onCamera[1];
				ShowMessage(rebounder.fullName + " rushes forward and performs an amazing diving header!");
				int successValue = 62;
				int maxGKSuccess = 95;
				successValue += (this.DefendingTeam.goalKeeper.goalKeepingPositioning - rebounder.header);
				if (successValue > maxGKSuccess)
					successValue = maxGKSuccess;

				int d100Roll = Dice.Instance.d100.Roll();

				bool goalScored = false;

				if (d100Roll <= successValue)
				{
					ShowMessage(this.DefendingTeam.goalKeeper.fullName + " is frozen on the play as the ball hits the back of the net! Goal!");
					goalScored = true;
					UpdateScore(null, rebounder, this.DefendingTeam.goalKeeper);
				}
				else
				{
					ShowMessage(this.DefendingTeam.goalKeeper.fullName + " is frozen on the play! But the ball goes wide of the net for a goal kick!");
				}
				attacker.gameStats.IncreaseShotOnGoal(goalScored);
				ChangePossession();
			}
			if (sum == 11)
			{
				Enums.RefAttitude disposition = Enums.RefAttitude.Friendly;
				if (homeTeam == this.AttackingTeam)
				{
					disposition = referee.attitudeToHomeTeam;
				}
				else
				{
					disposition = referee.attitudeToVisitingTeam;
				}
				bool goalStands = false;
				if (disposition == Enums.RefAttitude.Friendly)
				{
					goalStands = true;
				}
				ShowMessage(attacker.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
				ShowMessage(attacker.fullName + " takes a shot!");
				ShowMessage("Off the crossbar and In!");
				if (goalStands)
				{
					attacker.gameStats.IncreaseShotOnGoal(true);
					UpdateScore(null, attacker, this.DefendingTeam.goalKeeper);
				}
				else
				{
					ShowMessage("Oh, wait! No! The flag's been raised! He's offside! That'll negate the goal!");
					ShowMessage("Indirect free kick awarded to the defending team.");
					this.AttackingTeam.gameStats.offsides++;
				}
				ChangePossession();
			}
		}

		private void BChart()
		{
			int sum = Dice.Instance.d6.Roll() + Dice.Instance.d6.Roll();
			int greendie = Dice.Instance.d6.Roll();
			if (greendie == 6) greendie = 1;
			int defenderIndex = Dice.Instance.d6.Roll();
			if (defenderIndex == 6) defenderIndex = 5;
			Player attacker = FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[greendie - 1], this.AttackingTeam.onCamera, (greendie - 1));
			Player defender = FindPlayerWhenNoMatter(this.DefendingTeam.onCamera[defenderIndex - 1], this.DefendingTeam.onCamera, (greendie - 1));

			if (sum == 2 || sum == 12)
			{
				LargeFight();
			}
			if (sum == 3 || sum == 4 || sum == 5 || sum == 8)
			{
				ShowMessage(attacker.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
				ShowMessage(attacker.fullName + " takes a shot, but it is deflected off a defender.");
				ShowMessage("Wait! A whistle blows! The referee steps in. There's a bit of confusion on the field.");
				ShowMessage("Oh, wait! He's calling a handball on the defender " + defender.fullName + "! Oh, no!");
				if (sum == 5)
				{
					HandleCards(defender, true, false);
					ShowMessage("Apparently, the referee believed that he raised his arm intentionally. I'm not sure I agree but my opinion doesn't matter in this case.");
				}
				ShowMessage("Naturally, " + this.DefendingTeam.Name + " protest but that won't change a thing. Here comes a penalty kick!");
				PenaltyKick();
			}
			if (sum == 6)
			{
				ShowMessage("Vicious tackle " + defender.fullName + ". Certainly a foul.");
				ShowMessage("But " + defender.fullName + " is incensed with the call anyway and gets into the referee's face!");
				ShowMessage("The referee is now also incensed!");
				HandleCards(defender, true, false);
				ShowMessage(defender.fullName + " is still upset but has calmed down allowing play to finally resume.");
				ShowMessage("A free kick will now be granted but not anywhere near enough to the opposition goal to be dangerous. He'll just pass it from here I'm sure.");
				ChangePossession();
			}
			if (sum == 7 || sum == 9)
			{
				ShowMessage("Vicious tackle by " + defender.fullName + ". Certainly a foul. He tackled " + attacker.fullName + " needlessly from behind...");
				ShowMessage(attacker.fullName + " remains on the ground...");
				Injury(this.AttackingTeam, greendie);
				Enums.RefAttitude disposition = Enums.RefAttitude.Friendly;
				if (homeTeam == this.AttackingTeam)
					disposition = referee.attitudeToHomeTeam;
				else
					disposition = referee.attitudeToVisitingTeam;

				if (disposition == Enums.RefAttitude.Friendly)
				{
					HandleCards(defender, false, true);
					ShowMessage("A well deserved red if you ask me. Now, with that taken care of, play restarts.");
				}
				else if (disposition == Enums.RefAttitude.Neutral)
				{
					HandleCards(defender, true, false);
					ShowMessage("He's lucky to only get away with a yellow. Now, with that taken care of, play restarts.");
				}
				else
				{
					defender.gameStats.IncreaseFoulCount();
					ShowMessage("Unbelievable! No card! That was a tackle from behind yet the referee doesn't even look tempted to give him a card! I don't believe it!");
				}
				ShowMessage("A free kick will be granted but not anywhere near enough to the opposition goal to be dangerous. He'll just pass it from here I'm sure.");
				ChangePossession();
			}
			if (sum == 10)
			{
				minInjuryTime += 6;
				defender.gameStats.IncreaseFoulCount();
				ShowMessage("Vicious tackle by " + defender.fullName + ". Certainly a foul.");
				ShowMessage("Yet, somehow, players on " + this.DefendingTeam.Name + " take exception to that tackle. I can't say I blame them but...");
				ShowMessage("Several players are now in a heated argument...");
				ShowMessage("And now, I believe, a punch has been thrown! The referee needs to step in and he does!");
				greendie = Dice.Instance.d6.Roll();
				if (greendie == 6) greendie = 5;
				Player playerBooted = FindPlayerWhenNoMatter(this.DefendingTeam.onCamera[greendie - 1], this.DefendingTeam.onCamera, (greendie - 1));
				if (Dice.Instance.d6.Roll() < 3)
				{
					playerBooted = FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[greendie - 1], this.AttackingTeam.onCamera, (greendie - 1));
				}
				ShowMessage(playerBooted.fullName + " was the man who threw the punch...");
				HandleCards(playerBooted, false, true);
				ShowMessage("I'm sure the league will have a few things to say to that player with regard to this incident as well.");
				ShowMessage("That card proved to be the proverbial hose cooling tempers down. Now, with that taken care of, play restarts.");
				ShowMessage("A free kick will be granted but not anywhere near enough to the opposition goal to be dangerous. He'll just pass it from here I'm sure.");
				ChangePossession();
			}
			if (sum == 11)
			{
				minInjuryTime += 4;
				ShowMessage("Vicious tackle by " + defender.fullName + ". Certainly a foul.");
				ShowMessage("But" + defender.fullName + " is incensed with the call anyway and gets into the referee's face!");
				ShowMessage("Good lord! He just shoved the referee!");
				HandleCards(defender, false, true);
				ShowMessage("I'm sure the league will have a few things to say to that player with regard to this incident as well.");
				ShowMessage("That card proved to be the proverbial hose cooling tempers down. Now, with that taken care of, play restarts.");
				ShowMessage("A free kick will be granted but not anywhere near enough to the opposition goal to be dangerous. He'll just pass it from here I'm sure.");
				ChangePossession();
			}
		}

		private void UberRareHighlightReel()
		{
			int sum = Dice.Instance.d6.Roll() + Dice.Instance.d6.Roll();
			int greendie = Dice.Instance.d6.Roll();
			if (greendie == 6) greendie = 1;
			int defenderIndex = Dice.Instance.d6.Roll();
			if (defenderIndex == 6) defenderIndex = 5;
			Player attacker = FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[greendie - 1], this.AttackingTeam.onCamera, (greendie - 1));
			Player defender = FindPlayerWhenNoMatter(this.DefendingTeam.onCamera[defenderIndex - 1], this.DefendingTeam.onCamera, (greendie - 1));
			if (sum == 2)
				SuperUberRareHighlightReel();
			if (sum == 3)
				FreakGoalChart();
			if (sum == 4 || sum == 6 || sum == 9)
			{
				minInjuryTime += 4;
				ShowMessage("Vicious tackle by " + defender.fullName + ". Certainly a foul.");
				ShowMessage("But " + defender.fullName + " is incensed with the call anyway and gets into the referee's face!");
				ShowMessage("Good lord! He just shoved the referee!");
				HandleCards(defender, false, true);
				ShowMessage("I'm sure the league will have a few things to say to that player with regard to this incident as well.");
				ShowMessage("That card proved to be the proverbial hose cooling tempers down. Now, with that taken care of, play restarts.");
				ShowMessage("A free kick will be granted but not anywhere near enough to the opposition goal to be dangerous. He'll just pass it from here I'm sure.");
				ChangePossession();
			}
			if (sum == 5)
			{
				minInjuryTime += 10;
				defender.gameStats.IncreaseFoulCount();
				ShowMessage("Vicious tackle by " + defender.fullName + ". Certainly a foul.");
				ShowMessage("Yet, somehow, players on " + this.DefendingTeam.Name + " take exception to that tackle. I can't say I blame them but...");
				ShowMessage("Several players are now in a heated argument...");
				ShowMessage("Now the " + this.homeTeam.Name + " fans are throwing objects onto the field! Security is rushing to stop the madness!");
				ShowMessage("They eventually do, but now the referee is awarding " + awayTeam.Name + " a penalty kick due to the fans' actions!");
				ShowMessage("Way to go, fans. Way to help your team. Was it worth it?");
				if (this.AttackingTeam == homeTeam)
					ChangePossession();
				PenaltyKick();
			}
			if (sum == 7)
			{
				WeatherIssue();
			}
			if (sum == 8 || sum == 10)
			{
				ShowMessage(attacker.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
				ShowMessage("He trips and falls forward!");
				ShowMessage("And he won't be getting up! His teammate kicks the ball out of bounds so a physio can take care of him.");
				Injury(this.AttackingTeam , greendie);
				ShowMessage("Play restarts with a throw in. Oh, look at that! " + this.DefendingTeam.Name + " kicks the ball out of bounds to return the ball to " + this.AttackingTeam.Name);
				ShowMessage("They didn't want to take possession due to an injury! The crowd applauds this display of sportsmanship.");
				ShowMessage(this.AttackingTeam.Name + " will thusly throw the ball back in.");
			}
			if (sum == 11)
			{
				minInjuryTime += 12;
				ShowMessage("Vicious tackle by " + defender.fullName + ". Certainly a foul.");
				ShowMessage("Yet, somehow, players on " + this.DefendingTeam.Name + " take exception to that tackle. I can't say I blame them but...");
				ShowMessage("Several players are now in a heated argument...");
				ShowMessage("And now a player is getting into an argument with a fan! He reaches up to slap a fan but misses! And now security intervenes.");
				ShowMessage("Thank goodness. The alteraction has been quelled. That could've gotten REALLY ugly without them.");
				HandleCards(defender, false, true);
				ShowMessage("Further discipline is certainly in the cards in the future as well.");
				ShowMessage("A free kick will be granted but not anywhere near enough to the opposition goal to be dangerous. He'll just pass it from here I'm sure.");
				ChangePossession();
			}
			if (sum == 12)
			{
				LargeFight();
			}
		}

		private void LargeFight()
		{
			minInjuryTime += 12;
			int greendie = Dice.Instance.d6.Roll();
			if (greendie == 6) greendie = 1;
			int defenderIndex = Dice.Instance.d6.Roll();
			if (defenderIndex == 6) defenderIndex = 5;
			Player attacker = FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[greendie - 1], this.AttackingTeam.onCamera, (greendie - 1));
			Player defender = FindPlayerWhenNoMatter(this.DefendingTeam.onCamera[defenderIndex - 1], this.DefendingTeam.onCamera, (greendie - 1));
			ShowMessage("Vicious tackle " + defender.fullName + ". Certainly a foul.");
			defender.gameStats.IncreaseFoulCount();
			List<Player> hardAttackingPlayers = new List<Player>();
			List<Player> hardDefendingPlayers = new List<Player>();
			foreach (Player p in this.AttackingTeam.onCamera)
			{
				if (p.aggression > 85)
					//if player is on the field
					hardAttackingPlayers.Add(p);
			}
			foreach (Player p in this.AttackingTeam.offCamera)
			{
				if (p.aggression > 85)
					//if player is on the field
					hardAttackingPlayers.Add(p);
			}
			foreach (Player p in this.DefendingTeam.onCamera)
			{
				if (p.physicality > 85)
					//if player is on the field
					hardAttackingPlayers.Add(p);
			}
			foreach (Player p in this.DefendingTeam.onCamera)
			{
				if (p.physicality > 85)
					//if player is on the field
					hardAttackingPlayers.Add(p);
			}
			if (hardAttackingPlayers.Count == 0 && hardDefendingPlayers.Count == 0)
			{
				HandleCards(defender, false, true);
			}
			else
			{
				Player manSeeingRed = null;

				bool hardAttackerGetsRed = false;
				if (hardDefendingPlayers.Count == 0)
					hardAttackerGetsRed = true;
				else if (hardAttackingPlayers.Count == 0)
					hardAttackerGetsRed = false;
				else
				{
					if (Dice.Instance.d6.Roll() > 3)
						hardAttackerGetsRed = true;
				}

				if (hardAttackerGetsRed)
				{
					Die randomIndex = new Die(hardAttackingPlayers.Count);
					//randomIndex.Roll()
					manSeeingRed = hardAttackingPlayers[randomIndex.Roll() - 1];
				}
				else
				{
					Die randomIndex = new Die(hardDefendingPlayers.Count);
					manSeeingRed = hardDefendingPlayers[randomIndex.Roll() - 1];
				}

				ShowMessage("Yet, somehow, players on " + this.DefendingTeam.Name + " take exception to that tackle. I can't say I blame them but...");
				ShowMessage("Several players are now in a heated argument...");
				ShowMessage("Oh no! A shoving match begins! And now, I believe, a punch has been thrown! Maybe even two!");
				ShowMessage("The referee needs to step in and he does!");
				HandleCards(defender, false, true);
				foreach (Player p in hardAttackingPlayers)
					HandleCards(p, true, false);
				foreach (Player p in hardDefendingPlayers)
					HandleCards(p, true, false);
				ShowMessage("Those cards proved to be the proverbial hose cooling tempers down. Now, with that taken care of, play restarts.");
				ShowMessage("A free kick will be granted but not anywhere near enough to the opposition goal to be dangerous. He'll just pass it from here I'm sure.");
				ChangePossession();
			}
		}

		private void SuperUberRareHighlightReel()
		{
			// Most are funny but result in the game being suspended. Hence, I'll just make it a weather issue.
			WeatherIssue();
		}

		private void WeatherIssue()
		{
			ShowMessage("A light rain has suddenly started.");
			// Initially make the effects light.
			Die die = new Die(6);
			int dieRoll = die.Roll();
			if (dieRoll > 2)
			{
				// Make the moderate.
				ShowMessage("And now it's coming down steadily. This may affect the play on the field.");
			}
			if (dieRoll > 4)
			{
				// Make the heavy.
				ShowMessage("And now it's coming down incredibly hard. It'll be difficult to get any kind of offense going in this weather.");
			}
		}

		private void FreakGoalChart()
		{
			int sum = Dice.Instance.d6.Roll() + Dice.Instance.d6.Roll();
			int greendie = Dice.Instance.d6.Roll();
			if (greendie == 6) greendie = 1;
			Player attacker = FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[greendie - 1], this.AttackingTeam.onCamera, (greendie - 1));
			if (sum == 2)
			{
				ShowMessage(attacker.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
				ShowMessage(attacker.fullName + " takes a shot, but it is deflected off a defender.");
				ShowMessage("Ball takes an odd hop off him, though, giving " + attacker.fullName + " an opportunity for a header.");
				ShowMessage("Which he drives home as a stunned goalkeeper can only watch! What a goal!");
			}
			if (sum == 3)
			{
				ShowMessage(attacker.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
				ShowMessage(attacker.fullName + " takes a shot!");
				ShowMessage(this.DefendingTeam.goalKeeper.fullName + " is able to slap it away in the air and makes an excellent save.");
				ShowMessage("Did it hit him in the face? Seems like it as the keeper is a bit wobbly.");
				ShowMessage("He goes to pick up the ball... but it slips from his hand! He bends down again but now has kicked it into his net for an own goal!");
				ShowMessage("The players on " + this.DefendingTeam.Name + " can only stare in stunned silence as their opponents celebrate.");
			}
			if (sum == 4)
			{
				ShowMessage(attacker.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
				ShowMessage(attacker.fullName + " takes a shot!");
				ShowMessage("Through the legs of his opponent and into the net! A nutmeg for a goal!");
				ShowMessage(attacker.fullName + " takes a shot!");
			}
			if (sum == 5)
			{
				ShowMessage(attacker.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
				ShowMessage(attacker.fullName + " takes a shot!");
				ShowMessage("It misses completely but hits a teammate in the back! It deflects off the crossbar and tumbles into the goal! I don't believe it!");
				ShowMessage("And neither do the players on " + this.DefendingTeam.Name + " who can only stare in stunned silence as their opponents celebrate.");
				ShowMessage(attacker.fullName + " takes a shot!");
			}
			if (sum > 6)
			{
				ShowMessage(attacker.fullName + " makes tries a pass but completely misses his teammate.");
				ShowMessage("But none of the defenders can get to it either as the pass hits off the post!");
				ShowMessage(attacker.fullName + " charges forward and strikes the ball and hammers it into the back of the net! I don't believe it!");
				ShowMessage("And neither do the players on " + this.DefendingTeam.Name + " who can only stare in stunned silence as their opponents celebrate.");
				ShowMessage(attacker.fullName + " takes a shot!");
			}
			if (sum == 7)
			{
				ShowMessage(attacker.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
				ShowMessage(attacker.fullName + " takes a shot!");
				ShowMessage(this.DefendingTeam.goalKeeper.fullName + " is able to slap it away in the air and makes an excellent save.");
				ShowMessage("Scrum in front of the net as a bunch of feet kick at each other but not the ball. Did they forget their objective?");
				if (greendie == 6) greendie = 1;
				attacker = FindPlayerWhenNoMatter(this.AttackingTeam.onCamera[greendie - 1], this.AttackingTeam.onCamera, (greendie - 1));
				ShowMessage("No, wait! " + attacker.fullName + " hasn't! He strikes the ball and buries it into the back of the net! A goal!");
				ShowMessage(this.DefendingTeam.goalKeeper.fullName + "can only stare in stunned silence as his opponents celebrate.");
			}
			if (sum == 8)
			{
				int defenderIndex = Dice.Instance.d6.Roll();
				if (defenderIndex == 6) defenderIndex = 5;
				this.LongDistanceShotText(attacker, FindPlayerWhenNoMatter(this.DefendingTeam.onCamera[defenderIndex - 1], this.DefendingTeam.onCamera, (defenderIndex - 1)), attacker);
				ShowMessage("And the long distance shot hits the post, then  " + this.DefendingTeam.goalKeeper + " and bangs into the net! What an outstanding stroke of luck for " + this.AttackingTeam.Name);
				ShowMessage("And neither do the players on " + this.DefendingTeam.Name + " who can only stare in stunned silence as their opponents celebrate.");
			}
			if (sum == 9)
			{
				ShowMessage(attacker.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
				ShowMessage(attacker.fullName + " takes a shot, but it is deflected off a defender.");
				ShowMessage("Wait, but the ball keeps rolling toward the net... and the keeper is nowhwere near it! It's gonna get in for a goal! I don't believe it!");
				ShowMessage("And neither do the players on " + this.DefendingTeam.Name + " who can only stare in stunned silence as their opponents celebrate.");
			}
			if (sum == 10)
			{
				GoalkeeperDribblesBallOutsideBox();
				GoalKeeperScores(attacker);
				ChangePossession();
				attacker = this.AttackingTeam.goalKeeper;
			}
			if (sum == 11)
			{
				ShowMessage(attacker.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
				ShowMessage("He trips and falls forward!");
				ShowMessage("But the ball bounces off his head and sails... right into the net! It's a goal! I don't believe it!");
				ShowMessage("And neither do the players on " + this.DefendingTeam.Name + " who can only stare in stunned silence as their opponents celebrate.");
			}
			if (sum == 12)
			{
				ShowMessage(attacker.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
				ShowMessage(attacker.fullName + " takes a shot, but it is deflected off a defender.");
				int defenderIndex = Dice.Instance.d6.Roll();
				if (defenderIndex == 6) defenderIndex = 5;
				Player defender = FindPlayerWhenNoMatter(this.DefendingTeam.onCamera[defenderIndex - 1], this.DefendingTeam.onCamera, (defenderIndex - 1));
				ShowMessage(defender.fullName + " collects the ball and clears it from the box. But the attacking team catches right up to it and maintains possession.");
				ShowMessage(attacker.fullName + " takes another shot, this time from distance.");
				ShowMessage(this.DefendingTeam.goalKeeper.fullName + " moves toward the ball... and the stadium rocks as an overhead jet passes by distracting the keeper!");
				ShowMessage("Undeterred by the noise or the rocking, the ball hits the back of the net!");
				ShowMessage(this.DefendingTeam.goalKeeper.fullName + "can only stare in stunned silence as his opponents celebrate.");
			}
			attacker.gameStats.IncreaseShotOnGoal();
			UpdateScore(null, attacker, this.DefendingTeam.goalKeeper);
			ChangePossession();
		}

		private void EmptyNetGoal()
		{
			ShowMessage("Players on the " + this.DefendingTeam.Name + " can only stare listlessly as the ball slowly goes into their net!");
		}

		private void GoalKeeperScores(Player attacker)
		{
			ShowMessage(attacker.fullName + " is too late, though, and " + this.DefendingTeam.goalKeeper.fullName + " is able to deliver a tremendous boot to the ball.");
			ShowMessage("The wind catches the ball taking the kick just past his original target.");
			ShowMessage(this.AttackingTeam.goalKeeper.fullName + " thusly goes out of the box to advance a counter-attack.");
			ShowMessage("Wait, the ball takes an awkward bounce in front of him! It bounces past him and is headed toward the net!");
			ShowMessage(this.AttackingTeam.Name + " can only stare listlessly as the ball slowly goes into their net!");
			ShowMessage("As the players on " + this.AttackingTeam.Name + " hit the ground in utter despair, players on " + this.DefendingTeam.Name + " mob their goalkeeper congratulating on his most unusual goal!");
		}

		private void GoalkeeperDribblesBallOutsideBox()
		{
			ShowMessage("Goalkeeper dribbles the ball outside the box. He's currently acting as though he's a sweeper keeper.");
			ShowMessage("Now he takes a couple of steps back to line up a big boot.");
			ShowMessage("As he does, attacker sprints forward in an effort to get in front of the ball.");
		}

		private void BeatGKShootEmptyNet()
		{
			ShowMessage("And he does! The ball bounces off of him and heads toward the front of the box!");
			ShowMessage("The goalkeeper dashes toward it but just a step before he can reach it, the attacker gives the ball a giant kick and sends it soaring toward the empty net!");
		}

		private void HeadsBallAway(Player defender)
		{
			ShowMessage("No, wait! Luckily for them, " + defender.fullName + " anticipated where the play was going and stepped in front of the shot to head the ball away! What a head's up play by " + defender.fullName + "! He saved his team there!");
		}

		private void BeatGKOverheadKick()
		{
			ShowMessage("And he does! The ball bounces off of him and floats in the air!");
			ShowMessage("Goalkeeper attempts to head it away but before he can, the attacker boots it away with a tremendous overhead kick!");
		}

		private void LongDistanceShotText(Player attacker, Player defender, Player rebounder)
		{
			ShowMessage(attacker.fullName + " makes a nice move and dribbles toward the goal and finds himself with a golden opportunity!");
			ShowMessage(attacker.fullName + " takes a shot, but it is deflected off a defender.");
			int defenderIndex = Dice.Instance.d6.Roll();
			if (defenderIndex == 6) defenderIndex = 5;
			ShowMessage(defender.fullName + " collects the ball and clears it from the box. But the attacking team catches right up to it and maintains possession.");
			if (attacker == rebounder) ShowMessage(attacker.fullName + " takes another shot, this time from distance."); else ShowMessage(rebounder.fullName + " collects the ball and takes a shot from distance.");
			ShowMessage("Should be an easy one for " + this.DefendingTeam.goalKeeper.fullName + "... No, wait! he's out of position!");
		}

		private void LongDistanceShotAttempt(Player attacker)
		{
			int blackDieVal = Dice.Instance.d100.Roll();
			bool goalScored = false;

			if ((blackDieVal < attacker.shooting))
			{
				ShowMessage("And the long distance shot makes it in! " + this.DefendingTeam.goalKeeper.fullName + " still hadn't moved from the initial shot attempt which left him vulnerable!");
				ShowMessage("And needless to say, " + attacker.fullName + " capitalized!");
				goalScored = true;
				UpdateScore(null, attacker, this.DefendingTeam.goalKeeper);
			}
			else
			{
				ShowMessage("And the long distance shot is wide! " + this.DefendingTeam.goalKeeper.fullName + " still hadn't moved from the initial shot attempt which left him vulnerable!");
				ShowMessage("But " + attacker.fullName + " just couldn't quite capitalize!");
			}
			this.AttackingTeam.goalKeeper.gameStats.IncreaseShotOnGoal(goalScored);
			ChangePossession();
		}

		public bool AttackingTeamMostCircles()
		{
			double attackingTeamCircles = 0;
			double defendingTeamCircles = 0;

			foreach (Player p in this.AttackingTeam.onCamera)
			{
				if (!PlayerIsOffField(p))
				{
					attackingTeamCircles += p.acceleration;
				}
			}

			attackingTeamCircles /= 5;

			foreach (Player p in this.DefendingTeam.onCamera)
			{
				if (!PlayerIsOffField(p))
				{
					defendingTeamCircles += p.acceleration;
				}
			}

			defendingTeamCircles /= 5;

			return (attackingTeamCircles >= defendingTeamCircles);
		}

		private Enums.TierDifference LeageRankDifference()
		{
			int homeTier = homeTeam.tier;
			int awayTier = awayTeam.tier;

			if ((homeTier - awayTier) < 0)
				return Enums.TierDifference.HomeSuperior;
			if ((awayTier - homeTier) < 0)
				return Enums.TierDifference.AwaySuperior;
			return Enums.TierDifference.None;
		}

		private int TotalHomeScore()
		{
			int hFL = 0;
			if (homeFirstLeg > -1)
				hFL = homeFirstLeg;
			return (homeScore + hFL);
		}

		private int TotalAwayScore()
		{
			int afl = 0;
			if (awayFirstLeg > -1)
				afl = awayFirstLeg;
			return (awayScore + afl);
		}

		public string GetHomeScore()
		{
			string score = homeScore.ToString();
			if (homeFirstLeg > -1)
				score = "(" + homeFirstLeg.ToString() + " + " + homeScore.ToString() + " - " + (homeFirstLeg + homeScore).ToString() + ")";
			return score;
		}

		public string GetAwayScore()
		{
			string score = awayScore.ToString();
			if (awayFirstLeg > -1)
				score = "(" + awayFirstLeg.ToString() + " + " + awayScore.ToString() + " - " + (awayFirstLeg + awayScore).ToString() + ")";
			return score;
		}
}
