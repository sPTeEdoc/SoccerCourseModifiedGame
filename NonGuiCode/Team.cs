using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Team
{
	public string m_Name;
		public string Name
		{
			get
			{
				return this.m_Name;
			}
			set
			{
				this.m_Name = value;
			}
		}
		public string NickName;
		public Player[] onCamera;
		public Player[] offCamera;
		public List<Player> bench;
		public List<Player> reserves;
		public List<Player> completeRoster;
		public Player goalKeeper;
		public GameStats gameStats;
		public Dictionary<string, GameStats> seasonStats;
		public string imageFile = "";
		public string formation = "";
		public string LeagueName = "";
		public int tier; // 1 = premier, 2 = 2nd or 3rd division, 3 = 4th or 5th, etc.
		Dictionary<Team, List<int>> opponentsFixtures;
		public Player[] StartingEleven;
		public Dictionary<string, Dictionary<int, GameStats>> playerStats = new Dictionary<string, Dictionary<int, GameStats>>();

		public Team(string name, string nickName, string leagueName)
		{
			this.m_Name = name;
			this.NickName = nickName;
			this.LeagueName = leagueName;
			//this.onCamera = onCamera;
			//this.offCamera = offCamera;
			//this.goalKeeper = goalKeeper;
			this.bench = new List<Player>();
			gameStats = new GameStats();
			seasonStats = new Dictionary<string, GameStats>();
			this.completeRoster = new List<Player>();
			this.opponentsFixtures = new Dictionary<Team, List<int>>();
			tier = 99;
			this.StartingEleven = new Player[10];
		}

		public Team(string name, string leagueName)
		{
			this.m_Name = name;
			this.LeagueName = leagueName;
			//this.onCamera = onCamera;
			//this.offCamera = offCamera;
			//this.goalKeeper = goalKeeper;
			this.bench = new List<Player>();
			gameStats = new GameStats();
			seasonStats = new Dictionary<string, GameStats>();
			this.completeRoster = new List<Player>();
			this.opponentsFixtures = new Dictionary<Team, List<int>>();
			tier = 99;
			this.StartingEleven = new Player[10];
		}

		public Team Clone()
		{
			Player[] onCameraClone = new Player[5];
			Player[] offCameraClone = new Player[5];
			List<Player> benchClone = new List<Player>();
			List<Player> reservesClone = new List<Player>();
			Player[] StartingEleven = new Player[10];
			Player gk = this.goalKeeper.Clone();

			for (int i = 0; i < this.StartingEleven.Length; i ++)
			{
				StartingEleven[i] = this.StartingEleven[i];
			}

			for (int i = 0; i < this.onCamera.Length; i++)
			{
				onCameraClone[i] = this.onCamera[i].Clone();
			}
			for (int i = 0; i < this.offCamera.Length; i++)
			{
				offCameraClone[i] = this.offCamera[i].Clone();
			}
			foreach (Player p in this.bench)
				benchClone.Add(p.Clone());
			foreach (Player p in this.reserves)
				reservesClone.Add(p.Clone());

			Team team = new Team(this.Name, this.NickName);
			team.onCamera = onCameraClone;
			team.offCamera = offCameraClone;
			team.bench = benchClone;
			team.reserves = reservesClone;
			team.StartingEleven = StartingEleven;
			team.goalKeeper = gk;
			return team;
		}

		public void StoreGameStats()
		{
			TotalTeamGameStats();
		}

		public void StoreSeriesOfStats(string leagueName)
		{
			StorePlayerSeasonStats(leagueName);
			StoreTeamSeasonStats(leagueName);
		}

		public void ResetStats()
		{
			gameStats = new GameStats();
			goalKeeper.gameStats = new GameStats();
			ResetStats(onCamera);
			ResetStats(offCamera);
			ResetStats(bench.ToArray());
			ResetStats(reserves.ToArray());
		}

		private void ResetStats(Player[] players)
		{
			foreach (Player p in players)
				p.gameStats = new GameStats();
		}

		private void StorePlayerSeasonStats(string leagueName)
		{
			StoreSeasonStats(onCamera, leagueName);
			StoreSeasonStats(offCamera, leagueName);
			StoreSeasonStats(bench.ToArray(), leagueName);
			StoreSeasonStats(reserves.ToArray(), leagueName);
			StorePlayerSeasonStats(goalKeeper, leagueName);
			RecordPlayerStats(leagueName);
		}

		private void RecordPlayerStats(string leagueName)
		{
			foreach (Player p in completeRoster)
			{
				GameStats gs = new GameStats();

				if (playerStats.ContainsKey(leagueName))
				{
					if (playerStats[leagueName].ContainsKey(p.ID))
					{
						gs = playerStats[leagueName][p.ID];
					}
					else
					{
						playerStats[leagueName].Add(p.ID, gs);
					}
				}
				else
				{
					Dictionary<int, GameStats> ds = new Dictionary<int, GameStats>();
					ds.Add(p.ID, gs);
					playerStats.Add(leagueName, ds);
				}

				gs.assists += p.gameStats.assists;
				gs.fouls += p.gameStats.fouls;
				gs.goals += p.gameStats.goals;
				gs.goalsConceded += p.gameStats.goalsConceded;
				gs.penaltyKickAttempts += p.gameStats.penaltyKickAttempts;
				gs.penaltyKickGoals += p.gameStats.penaltyKickGoals;
				gs.redCards += p.gameStats.redCards;
				gs.Saves += p.gameStats.Saves;
				gs.shotsOnGoal += p.gameStats.shotsOnGoal;
				gs.shotsTotal += p.gameStats.shotsTotal;
				gs.yellowCards += p.gameStats.yellowCards;
				gs.cleansheets += p.gameStats.cleansheets;
				gs.matchesPlayed = p.LeagueTeamSeasonStats[leagueName][this.Name].matchesPlayed;
			}
		}

		//private void StoreCupStats()
		//{
		//    this.cupStats.assists += this.gameStats.assists;
		//    this.cupStats.fouls += this.gameStats.fouls;
		//    this.cupStats.goals += this.gameStats.goals;
		//    this.cupStats.goalsConceded += this.gameStats.goalsConceded;
		//    this.cupStats.penaltyKickAttempts += this.gameStats.penaltyKickAttempts;
		//    this.cupStats.penaltyKickGoals += this.gameStats.penaltyKickGoals;
		//    this.cupStats.redCards += this.gameStats.redCards;
		//    this.cupStats.Saves += this.gameStats.Saves;
		//    this.cupStats.shotsOnGoal += this.gameStats.shotsOnGoal;
		//    this.cupStats.shotsTotal += this.gameStats.shotsTotal;
		//    this.cupStats.yellowCards += this.gameStats.yellowCards;
		//    this.cupStats.cleansheets += this.gameStats.cleansheets;
		//}

		private void StoreTeamSeasonStats(string leagueName)
		{
			this.CreateSeasonStats(leagueName);

			this.seasonStats[leagueName].assists += this.gameStats.assists;
			this.seasonStats[leagueName].fouls += this.gameStats.fouls;
			this.seasonStats[leagueName].goals += this.gameStats.goals;
			this.seasonStats[leagueName].goalsConceded += this.gameStats.goalsConceded;
			this.seasonStats[leagueName].penaltyKickAttempts += this.gameStats.penaltyKickAttempts;
			this.seasonStats[leagueName].penaltyKickGoals += this.gameStats.penaltyKickGoals;
			this.seasonStats[leagueName].redCards += this.gameStats.redCards;
			this.seasonStats[leagueName].Saves += this.gameStats.Saves;
			this.seasonStats[leagueName].shotsOnGoal += this.gameStats.shotsOnGoal;
			this.seasonStats[leagueName].shotsTotal += this.gameStats.shotsTotal;
			this.seasonStats[leagueName].yellowCards += this.gameStats.yellowCards;
			this.seasonStats[leagueName].cleansheets += this.gameStats.cleansheets;
		}

		public void CreateSeasonStats(string leagueName)
		{
			GameStats gs = new GameStats();

			if (!seasonStats.ContainsKey(leagueName))
			{
				seasonStats.Add(leagueName, gs);
			}
		}

		private void StoreSeasonStats(Player[] players, string leagueName)
		{
			foreach (Player p in players)
			{
				StorePlayerSeasonStats(p, leagueName);
			}
		}

		private void StorePlayerSeasonStats(Player p, string leagueName)
		{
			p.LeagueTeamSeasonStats[leagueName][Name].assists += p.gameStats.assists;
			p.LeagueTeamSeasonStats[leagueName][Name].fouls += p.gameStats.fouls;
			p.LeagueTeamSeasonStats[leagueName][Name].goals += p.gameStats.goals;
			p.LeagueTeamSeasonStats[leagueName][Name].goalsConceded += p.gameStats.goalsConceded;
			p.LeagueTeamSeasonStats[leagueName][Name].penaltyKickAttempts += p.gameStats.penaltyKickAttempts;
			p.LeagueTeamSeasonStats[leagueName][Name].penaltyKickGoals += p.gameStats.penaltyKickGoals;
			p.LeagueTeamSeasonStats[leagueName][Name].redCards += p.gameStats.redCards;
			p.LeagueTeamSeasonStats[leagueName][Name].Saves += p.gameStats.Saves;
			p.LeagueTeamSeasonStats[leagueName][Name].shotsOnGoal += p.gameStats.shotsOnGoal;
			p.LeagueTeamSeasonStats[leagueName][Name].shotsTotal += p.gameStats.shotsTotal;
			p.LeagueTeamSeasonStats[leagueName][Name].yellowCards += p.gameStats.yellowCards;
			p.LeagueTeamSeasonStats[leagueName][Name].cleansheets += p.gameStats.cleansheets;
		}

		public void TotalTeamGameStats()
		{
			TotalTeamGameStats(onCamera);
			TotalTeamGameStats(offCamera);
			TotalTeamGameStats(bench.ToArray());
			TotalTeamGameStats(reserves.ToArray());
			AddPlayerStatsToTeamStats(goalKeeper);
		}

		private void TotalTeamGameStats(Player[] players)
		{
			foreach (Player p in players)
			{
				AddPlayerStatsToTeamStats(p);
			}
		}

		private void AddPlayerStatsToTeamStats(Player p)
		{
			gameStats.assists += p.gameStats.assists;
			gameStats.fouls += p.gameStats.fouls;
			gameStats.goals += p.gameStats.goals;
			gameStats.goalsConceded += p.gameStats.goalsConceded;
			gameStats.penaltyKickAttempts += p.gameStats.penaltyKickAttempts;
			gameStats.penaltyKickGoals += p.gameStats.penaltyKickGoals;
			gameStats.redCards += p.gameStats.redCards;
			gameStats.Saves += p.gameStats.Saves;
			gameStats.shotsOnGoal += p.gameStats.shotsOnGoal;
			gameStats.shotsTotal += p.gameStats.shotsTotal;
			gameStats.yellowCards += p.gameStats.yellowCards;
			gameStats.cleansheets += p.gameStats.cleansheets;
		}

		public void FillRoster()
		{
			foreach (Player p in onCamera)
				completeRoster.Add(p);
			foreach (Player p in offCamera)
				completeRoster.Add(p);
			foreach (Player p in bench)
				completeRoster.Add(p);
			foreach (Player p in reserves)
				completeRoster.Add(p);
			completeRoster.Add(goalKeeper);
		}

		public void ConfigureRoster()
		{
			Dictionary<Player, int> CenterBacks = new Dictionary<Player, int>();
			Dictionary<Player, int> LeftBacks = new Dictionary<Player, int>();
			Dictionary<Player, int> RightBacks = new Dictionary<Player, int>();

			Dictionary<Player, int> RightWingForwards = new Dictionary<Player, int>();
			Dictionary<Player, int> LeftWingForwards = new Dictionary<Player, int>();
			Dictionary<Player, int> Strikers = new Dictionary<Player, int>();

			Dictionary<Player, int> CentralAttackingMidfielders = new Dictionary<Player, int>();
			Dictionary<Player, int> CentralDefendingMidfielders = new Dictionary<Player, int>();
			Dictionary<Player, int> CentralMidfielders = new Dictionary<Player, int>();
			Dictionary<Player, int> LeftMidfielders = new Dictionary<Player, int>();
			Dictionary<Player, int> RightMidfielders = new Dictionary<Player, int>();

			Dictionary<Player, int> goalkeepers = new Dictionary<Player, int>();
			Dictionary<Player, int> benchAndReserves = new Dictionary<Player, int>();

			bench = new List<Player>();
			reserves = new List<Player>();
			onCamera = new Player[5];
			offCamera = new Player[5];

			foreach (Player p in completeRoster)
			{
				if (p.gamesOutDueToInjury > 0 || p.gamesOutDueToSuspension > 0)
					reserves.Add(p);
				else
				{
					int playerScore = CalculatePlayerScore(p);

					if (p.Position == Enums.Positions.Goalkeeper)
						goalkeepers.Add(p, playerScore);
					//if (p.Position == Enums.Positions.MidFielder || p.Position == Enums.Positions.CentralAttackingMidfielder ||
					//    p.Position == Enums.Positions.CentralDefendingMidfielder || p.Position == Enums.Positions.CentralMidfielder ||
					//    p.Position == Enums.Positions.LeftMidfielder || p.Position == Enums.Positions.RightMidfielder)
					//    midfielders.Add(p, playerScore);
					//if (p.Position == Enums.Positions.Forward || p.Position == Enums.Positions.LeftWingForward ||
					//    p.Position == Enums.Positions.Striker || p.Position == Enums.Positions.RightWingForward)
					//    forwards.Add(p, playerScore);
					//if (p.Position == Enums.Positions.Defender || p.Position == Enums.Positions.CenterBack 
					//    || p.Position == Enums.Positions.LeftBack || p.Position == Enums.Positions.RightBack)
					//    defenders.Add(p, playerScore);

					if (p.Position == Enums.Positions.CentralAttackingMidfielder)
						CentralAttackingMidfielders.Add(p, playerScore);
					if (p.Position == Enums.Positions.CentralDefendingMidfielder)
						CentralDefendingMidfielders.Add(p, playerScore);
					if (p.Position == Enums.Positions.CentralMidfielder)
						CentralMidfielders.Add(p, playerScore);
					if (p.Position == Enums.Positions.LeftMidfielder)
						LeftMidfielders.Add(p, playerScore);
					if (p.Position == Enums.Positions.RightMidfielder)
						RightMidfielders.Add(p, playerScore);

					if (p.Position == Enums.Positions.Striker)
						Strikers.Add(p, playerScore);
					if (p.Position == Enums.Positions.LeftWingForward)
						LeftWingForwards.Add(p, playerScore);
					if (p.Position == Enums.Positions.RightWingForward)
						RightWingForwards.Add(p, playerScore);

					if (p.Position == Enums.Positions.CenterBack)
						CenterBacks.Add(p, playerScore);
					if (p.Position == Enums.Positions.RightBack)
						RightBacks.Add(p, playerScore);
					if (p.Position == Enums.Positions.LeftBack)
						LeftBacks.Add(p, playerScore);
				}
			}

			List<Player> gks = SortListByPlayerScore(goalkeepers);

			List<Player> cams = SortListByPlayerScore(CentralAttackingMidfielders);
			List<Player> cdms = SortListByPlayerScore(CentralDefendingMidfielders);
			List<Player> cmfs = SortListByPlayerScore(CentralMidfielders);
			List<Player> lmfs = SortListByPlayerScore(LeftMidfielders);
			List<Player> rmfs = SortListByPlayerScore(RightMidfielders);

			List<Player> rwfs = SortListByPlayerScore(RightWingForwards);
			List<Player> lwfs = SortListByPlayerScore(LeftWingForwards);
			List<Player> sts = SortListByPlayerScore(Strikers);

			List<Player> cbs = SortListByPlayerScore(CenterBacks);
			List<Player> lbs = SortListByPlayerScore(LeftBacks);
			List<Player> rbs = SortListByPlayerScore(RightBacks);

//            Basically,
//https://www.soccercoachingpro.com/soccer-formations/

//public Player[] StartingEleven

//StartingEleven = new Player[10];

//4-2-1-3 Aston Villa, Brighton, Boulton, Chelsea, Fulham, Man City, Man U, Spurs, West Hampton
//https://www.soccercoachingpro.com/4-2-3-1-formation/
//https://duckduckgo.com/?q=4-2-1-3&iax=images&ia=images&iai=https%3A%2F%2Fwww.footballizer.com%2Fimg%2Fpublic%2Fformation-4-2-1-3.jpg
//4-3-3, Arsenal, Crystal Palace, Liverpool, NewCastle
//https://www.soccercoachingpro.com/4-3-3-formation/
//3-4-2-1 Luton, Wolves
//https://www.soccercoachingpro.com/3-4-2-1-formation/
//https://duckduckgo.com/?q=3-4-2-1&iax=images&ia=images&iai=https%3A%2F%2Fgmbosk2005.com%2Fwp-content%2Fuploads%2F2022%2F12%2F3-4-2-1-1536x1152.png
//3-5-2, Brenton, South Hampton
//4-4-1-1, Everton
//4-4-2, Burnley
//https://www.soccercoachingpro.com/4-4-2-formation/
//4-2-3-1, Nottingham Forest
//https://www.soccercoachingpro.com/4-2-3-1-formation/
//3-4-3
//[9][8][7][6][5][4][3][2][1][0]
//CB,CB,CB,LCM,CM,CM,RCM,LWF,RWF,ST
//https://www.soccercoachingpro.com/3-4-3-formation/
//4-1-2-1-2
//https://www.soccercoachingpro.com/4-1-2-1-2-formation/

			if (formation == "433")
			{
				//[9][8][7][6][5][4][3][2][1][0]
				//FB,CB,CB,FB,LCM,RCM,CAM,LWF,RWF,ST
				StartingEleven[9] = GetNextPlayerAvailable(lbs, rbs, cbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[8] = GetNextPlayerAvailable(cbs, rbs, lbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[7] = GetNextPlayerAvailable(cbs, lbs, rbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[6] = GetNextPlayerAvailable(rbs, lbs, cbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts);

				StartingEleven[5] = GetNextPlayerAvailable(lmfs, cmfs, cdms, rmfs, cams, sts, lbs, rbs, cbs, rwfs, lwfs);
				StartingEleven[4] = GetNextPlayerAvailable(cmfs, cams, cdms, rmfs, lmfs, sts, lbs, rbs, cbs, rwfs, lwfs);
				StartingEleven[3] = GetNextPlayerAvailable(rmfs, cmfs, cams, lmfs, cdms, sts, lbs, rbs, cbs, rwfs, lwfs);

				StartingEleven[2] = GetNextPlayerAvailable(lwfs, rwfs, sts, cams, cmfs, lmfs, rmfs, cdms, lbs, rbs, cbs);
				StartingEleven[1] = GetNextPlayerAvailable(rwfs, lwfs, sts, cams, cmfs, lmfs, rmfs, cdms, lbs, rbs, cbs);
				StartingEleven[0] = GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, rmfs, lmfs, cdms, lbs, rbs, cbs);
			}
			else if (formation == "3421")
			{
				//[9][8][7][6][5][4][3][2][1][0]
				//CB,CB,CB,LCM,CM,CM,RCM,LWF,RWF,ST
				StartingEleven[9] = GetNextPlayerAvailable(cbs, rbs, lbs, cmfs, cdms, lmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[8] = GetNextPlayerAvailable(cbs, rbs, lbs, cmfs, cdms, lmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[7] = GetNextPlayerAvailable(cbs, lbs, rbs, cmfs, cdms, lmfs, rmfs, cams, lwfs, rwfs, sts);

				StartingEleven[6] = GetNextPlayerAvailable(cdms, cmfs, cams, lmfs, rmfs, lwfs, rwfs, lbs, rbs, cbs, sts);
				StartingEleven[5] = GetNextPlayerAvailable(cmfs, lmfs, cdms, rmfs, cams, lwfs, rwfs, lbs, rbs, cbs, sts);
				StartingEleven[4] = GetNextPlayerAvailable(cmfs, rmfs, cdms, lmfs, cams, lwfs, rwfs, lbs, rbs, cbs, sts);
				StartingEleven[3] = GetNextPlayerAvailable(cams, cmfs, cdms, rmfs, lmfs, lwfs, rwfs, lbs, rbs, cbs, sts);

				StartingEleven[2] = GetNextPlayerAvailable(lmfs, sts, lwfs, cams, cmfs, lmfs, rmfs, cdms, rbs, lbs, cbs);
				StartingEleven[1] = GetNextPlayerAvailable(rmfs, sts, rwfs, cams, cmfs, lmfs, rmfs, cdms, rbs, lbs, cbs);
				StartingEleven[0] = GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, lmfs, rmfs, cdms, rbs, lbs, cbs);
			}
			else if (formation == "352")
			{
				//[9][8][7][6][5][4][3][2][1][0]
				//CB,CB,CB,LCM,CM,CM,CM,RCM,ST,ST
				StartingEleven[9] = GetNextPlayerAvailable(cbs, rbs, lbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[8] = GetNextPlayerAvailable(cbs, rbs, lbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[7] = GetNextPlayerAvailable(cbs, lbs, rbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts);

				StartingEleven[6] = GetNextPlayerAvailable(cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, lbs, rbs, cbs, sts);
				StartingEleven[5] = GetNextPlayerAvailable(lmfs, cmfs, cams, rmfs, cdms, lwfs, rwfs, lbs, rbs, cbs, sts);
				StartingEleven[4] = GetNextPlayerAvailable(rmfs, cmfs, cdms, lmfs, cams, lwfs, rwfs, lbs, rbs, cbs, sts);
				StartingEleven[3] = GetNextPlayerAvailable(cmfs, rmfs, cams, lmfs, cdms, lwfs, rwfs, lbs, rbs, cbs, sts);
				StartingEleven[2] = GetNextPlayerAvailable(cams, cmfs, cdms, lmfs, rmfs, lwfs, rwfs, lbs, rbs, cbs, sts);

				StartingEleven[1] = GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, lmfs, rmfs, cdms, lbs, rbs, cbs);
				StartingEleven[0] = GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, lmfs, rmfs, cdms, lbs, rbs, cbs);
			}
			else if (formation == "4411")
			{
				//[9][8][7][6][5][4][3][2][1][0]
				//CB,CB,CB,LCM,CM,CM,CM,RCM,ST,ST

				StartingEleven[9] = GetNextPlayerAvailable(lbs, rbs, cbs, cdms, lmfs, cmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[8] = GetNextPlayerAvailable(cbs, rbs, lbs, cdms, lmfs, cmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[7] = GetNextPlayerAvailable(cbs, lbs, rbs, cdms, lmfs, cmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[6] = GetNextPlayerAvailable(rbs, lbs, cbs, cdms, lmfs, cmfs, rmfs, cams, lwfs, rwfs, sts);

				StartingEleven[5] = GetNextPlayerAvailable(lmfs, cdms, cmfs, rmfs, cams, lwfs, rwfs, lbs, rbs, cbs, sts);
				StartingEleven[4] = GetNextPlayerAvailable(cmfs, cdms, lmfs, rmfs, cams, rwfs, lwfs, lbs, rbs, cbs, sts);
				StartingEleven[3] = GetNextPlayerAvailable(cmfs, cdms, rmfs, lmfs, cams, lwfs, rwfs, lbs, rbs, cbs, sts);
				StartingEleven[2] = GetNextPlayerAvailable(rmfs, cmfs, cams, lmfs, cdms, rwfs, lwfs, lbs, rbs, cbs, sts);

				StartingEleven[1] = GetNextPlayerAvailable(cams, sts, lwfs, rwfs, cmfs, rmfs, lmfs, cdms, lbs, cbs, rbs);
				StartingEleven[0] = GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, lmfs, rmfs, cdms, lbs, cbs, rbs);
			}
			else if (formation == "442")
			{
				//[9][8][7][6][5][4][3][2][1][0]
				//LB,CB,CB,RB,CM,CM,CM,CM,ST,ST
				StartingEleven[9] = GetNextPlayerAvailable(lbs, rbs, cbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[8] = GetNextPlayerAvailable(cbs, rbs, lbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[7] = GetNextPlayerAvailable(cbs, lbs, rbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[6] = GetNextPlayerAvailable(rbs, lbs, cbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts);

				StartingEleven[5] = GetNextPlayerAvailable(lmfs, cmfs, cdms, cmfs, cams, lwfs, rwfs, lbs, cbs, rbs, sts);
				StartingEleven[4] = GetNextPlayerAvailable(cmfs, cdms, lmfs, rmfs, cams, rwfs, lwfs, lbs, cbs, rbs, sts);
				StartingEleven[3] = GetNextPlayerAvailable(cmfs, cams, rmfs, lmfs, cams, lwfs, rwfs, lbs, cbs, rbs, sts);
				StartingEleven[2] = GetNextPlayerAvailable(rmfs, cmfs, cams, lmfs, cdms, rwfs, lwfs, lbs, cbs, rbs, sts);

				StartingEleven[1] = GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, lmfs, rmfs, cdms, lbs, cbs, rbs);
				StartingEleven[0] = GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, lmfs, rmfs, cdms, lbs, cbs, rbs);
			}
			else if (formation == "4231")
			{
				//[9][8][7][6][5][4][3][2][1][0]
				//LB,CB,CB,RB,CDM,CDM,CM,CAM,ST,ST
				StartingEleven[9] = GetNextPlayerAvailable(lbs, rbs, cbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[8] = GetNextPlayerAvailable(cbs, rbs, lbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[7] = GetNextPlayerAvailable(cbs, lbs, rbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[6] = GetNextPlayerAvailable(rbs, lbs, cbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts);

				StartingEleven[5] = GetNextPlayerAvailable(cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, lbs, rbs, cbs, sts);
				StartingEleven[4] = GetNextPlayerAvailable(cdms, cmfs, rmfs, lmfs, cams, lwfs, rwfs, lbs, rbs, cbs, sts);

				StartingEleven[3] = GetNextPlayerAvailable(lmfs, cmfs, rmfs, cams, cdms, lwfs, rwfs, lbs, rbs, cbs, sts);
				StartingEleven[2] = GetNextPlayerAvailable(cams, cmfs, lmfs, rmfs, cdms, lwfs, rwfs, lbs, rbs, cbs, sts);
				StartingEleven[1] = GetNextPlayerAvailable(rmfs, cams, cmfs, lmfs, cdms, lwfs, rwfs, lbs, rbs, cbs, sts);

				StartingEleven[0] = GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, lmfs, rmfs, cdms, lbs, rbs, cbs);
			}
			else if (formation == "41212")
			{
				//[9][8][7][6][5][4][3][2][1][0]
				//LB,CB,CB,RB,CDM,CM,CM,CAM,ST,ST
				StartingEleven[9] = GetNextPlayerAvailable(lbs, rbs, cbs, cdms, lmfs, rmfs, cams, cmfs, lwfs, rwfs, sts);
				StartingEleven[8] = GetNextPlayerAvailable(cbs, rbs, lbs, cdms, lmfs, rmfs, cams, cmfs, lwfs, rwfs, sts);
				StartingEleven[7] = GetNextPlayerAvailable(cbs, lbs, rbs, cdms, lmfs, rmfs, cams, cmfs, lwfs, rwfs, sts);
				StartingEleven[6] = GetNextPlayerAvailable(rbs, lbs, cbs, cdms, lmfs, rmfs, cams, cmfs, lwfs, rwfs, sts);

				StartingEleven[5] = GetNextPlayerAvailable(cdms, cmfs, cams, lmfs, rmfs, rwfs, lwfs, lbs, rbs, cbs, sts);

				StartingEleven[4] = GetNextPlayerAvailable(lmfs, cmfs, rmfs, cams, cdms, lwfs, rwfs, lbs, rbs, cbs, sts);
				StartingEleven[3] = GetNextPlayerAvailable(rmfs, cmfs, lmfs, cams, cdms, rwfs, lwfs, lbs, rbs, cbs, sts);

				StartingEleven[2] = GetNextPlayerAvailable(cams, cmfs, sts, lmfs, rmfs, lwfs, rwfs, lbs, rbs, cbs, cdms);

				StartingEleven[1] = GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, rmfs, lmfs, lbs, rbs, cbs, cdms);
				StartingEleven[0] = GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, lmfs, rmfs, lbs, rbs, cbs, cdms);
			}
			else if (formation == "343")
			{
				StartingEleven[9] = GetNextPlayerAvailable(cbs, rbs, lbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[8] = GetNextPlayerAvailable(cbs, rbs, lbs, cdms, lmfs, cmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[7] = GetNextPlayerAvailable(cbs, lbs, rbs, cdms, lmfs, rmfs, cmfs, cams, lwfs, rwfs, sts);

				StartingEleven[6] = GetNextPlayerAvailable(cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, lbs, rbs, cbs, sts);
				StartingEleven[5] = GetNextPlayerAvailable(cmfs, lmfs, rmfs, cams, cdms, rwfs, lwfs, lbs, rbs, cbs, sts);
				StartingEleven[4] = GetNextPlayerAvailable(cmfs, rmfs, lmfs, cams, cdms, lwfs, rwfs, lbs, rbs, cbs, sts);
				StartingEleven[3] = GetNextPlayerAvailable(cams, cmfs, rmfs, lmfs, cdms, rwfs, lwfs, lbs, rbs, cbs, sts);

				StartingEleven[2] = GetNextPlayerAvailable(lmfs, sts, lwfs, cams, cmfs, rmfs, lmfs, lbs, rbs, cbs, cdms);
				StartingEleven[1] = GetNextPlayerAvailable(rmfs, sts, rwfs, cams, cmfs, lmfs, rmfs, lbs, rbs, cbs, cdms);
				StartingEleven[0] = GetNextPlayerAvailable(sts, lwfs, rwfs, cams, rmfs, cmfs, lmfs, lbs, rbs, cbs, cdms);
			}
			else
			{
				//4213
				//[9][8][7][6][5][4][3][2][1][0]
				//LB,CB,CB,RB,CDM,CDM,CAM,LWF,RWF,ST
				StartingEleven[9] = GetNextPlayerAvailable(lbs, rbs, cbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[8] = GetNextPlayerAvailable(cbs, rbs, lbs, cdms, lmfs, cmfs, rmfs, cams, lwfs, rwfs, sts);
				StartingEleven[7] = GetNextPlayerAvailable(cbs, lbs, rbs, cdms, lmfs, rmfs, cmfs, cams, lwfs, rwfs, sts);
				StartingEleven[6] = GetNextPlayerAvailable(rbs, lbs, cbs, cdms, lmfs, rmfs, cmfs, cams, lwfs, rwfs, sts);

				StartingEleven[5] = GetNextPlayerAvailable(cdms, cmfs, lmfs, rmfs, cdms, lwfs, rwfs, lbs, rbs, cbs, sts);
				StartingEleven[4] = GetNextPlayerAvailable(cams, cmfs, cdms, lmfs, rmfs, rwfs, lwfs, lbs, rbs, cbs, sts);
				StartingEleven[3] = GetNextPlayerAvailable(cams, rmfs, lmfs, cmfs, cdms, lwfs, rwfs, lbs, rbs, cbs, sts);

				StartingEleven[2] = GetNextPlayerAvailable(lwfs, rwfs, sts, cams, cmfs, lmfs, rmfs, lbs, rbs, cbs, cdms);
				StartingEleven[1] = GetNextPlayerAvailable(rwfs, lwfs, sts, cams, cmfs, rmfs, lmfs, lbs, rbs, cbs, cdms);
				StartingEleven[0] = GetNextPlayerAvailable(sts, lwfs, rwfs, cams, rmfs, cmfs, lmfs, lbs, rbs, cbs, cdms);
			}

			offCamera[4] = StartingEleven[9];
			offCamera[3] = StartingEleven[7];
			offCamera[2] = StartingEleven[5];
			offCamera[1] = StartingEleven[3];
			offCamera[0] = StartingEleven[1];

			onCamera[4] = StartingEleven[8];
			onCamera[3] = StartingEleven[6];
			onCamera[2] = StartingEleven[4];
			onCamera[1] = StartingEleven[2];
			onCamera[0] = StartingEleven[0];

			//OffScreen[4]=Starting[9], OnScreen[4]=Starting[8]
			//OffScreen[3]=Starting[7], OnScreen[3]=Starting[6]
			//OffScreen[2]=Starting[5], OnScreen[2]=Starting[4]
			//OffScreen[1]=Starting[3], OnScreen[1]=Starting[2]
			//OffScreen[0]=Starting[1], OnScreen[0]=Starting[0]

			////slots 4-5, fill with best dfs
			//onCamera[4] = GetNextPlayerAvailable(dfs, mfs, fs);
			//offCamera[4] = GetNextPlayerAvailable(dfs, mfs, fs);
			//onCamera[3] = GetNextPlayerAvailable(dfs, mfs, fs);
			//offCamera[3] = GetNextPlayerAvailable(dfs, mfs, fs);

			//onCamera[0] = GetNextPlayerAvailable(fs, mfs, dfs);
			//offCamera[0] = GetNextPlayerAvailable(fs, mfs, dfs); ;
			////slot 2, fill with best two mfs or best mf and next best f
			//bool midfielderaddedInOneSlot = false;

			//// because rearlier removal, fs[0] is now the next forward
			//if (fs.Count == 0)
			//{
			//    onCamera[1] = GetNextPlayerAvailable(mfs, fs, dfs); ;
			//    //mfs.Remove(onCamera[1]);
			//    midfielderaddedInOneSlot = true;
			//}
			//else
			//{
			//    if (midfielders[mfs[0]] >= forwards[fs[0]])
			//    {
			//        onCamera[1] = GetNextPlayerAvailable(fs, mfs, dfs);
			//        //mfs.Remove(onCamera[1]);
			//        midfielderaddedInOneSlot = true;
			//    }
			//    else
			//    {
			//        onCamera[1] = GetNextPlayerAvailable(fs, mfs, dfs);
			//        //fs.Remove(onCamera[1]);
			//    }
			//}

			//if (midfielderaddedInOneSlot)
			//{
			//    if (fs.Count == 0)
			//    {
			//        offCamera[1] = GetNextPlayerAvailable(mfs, fs, dfs); ;
			//        //mfs.Remove(offCamera[1]);
			//    }
			//    else
			//    {
			//        // slot 0 still contains the best remaining midfielder
			//        if (midfielders[mfs[0]] >= forwards[fs[0]] && formation != "FourThreeThree")
			//        {
			//            offCamera[1] = GetNextPlayerAvailable(fs, mfs, dfs);
			//            //mfs.Remove(offCamera[1]);
			//        }
			//        else
			//        {
			//            offCamera[1] = GetNextPlayerAvailable(fs, mfs, dfs);
			//            //fs.Remove(offCamera[1]);
			//        }
			//    }
			//}
			//else
			//{
			//    offCamera[1] = GetNextPlayerAvailable(mfs, fs, dfs);
			//    //mfs.Remove(offCamera[1]);
			//}

			////slot 3, fill with remaining best mfs
			//onCamera[2] = GetNextPlayerAvailable(mfs, fs, dfs);
			//offCamera[2] = GetNextPlayerAvailable(mfs, fs, dfs);

			this.goalKeeper = gks[0];
			gks.Remove(gks[0]);

			foreach (Player p in cbs)
			{
				benchAndReserves[p] = CenterBacks[p];
			}
			foreach (Player p in lbs)
			{
				benchAndReserves[p] = LeftBacks[p];
			}
			foreach (Player p in rbs)
			{
				benchAndReserves[p] = RightBacks[p];
			}

			foreach (Player p in cmfs)
			{
				benchAndReserves[p] = CentralMidfielders[p];
			}
			foreach (Player p in cdms)
			{
				benchAndReserves[p] = CentralDefendingMidfielders[p];
			}
			foreach (Player p in lmfs)
			{
				benchAndReserves[p] = LeftMidfielders[p];
			}
			foreach (Player p in rmfs)
			{
				benchAndReserves[p] = RightMidfielders[p];
			}
			foreach (Player p in cams)
			{
				benchAndReserves[p] = CentralAttackingMidfielders[p];
			}

			foreach (Player p in sts)
			{
				benchAndReserves[p] = Strikers[p];
			}
			foreach (Player p in lwfs)
			{
				benchAndReserves[p] = LeftWingForwards[p];
			}
			foreach (Player p in rwfs)
			{
				benchAndReserves[p] = RightWingForwards[p];
			}

			int benchIndex = 0;

			//fill bench with goalkeeper, top remaining forward, midfielder, defender, if any are available, then fill the bench with the top players until all players are exhausted or no players remain
			for (int i = 0; i < gks.Count; i++)
			{
				if (i == 0)
					bench.Add(gks[benchIndex++]);
				else
					reserves.Add(gks[i]);
			}

			List<Player> benchPlayersAndReserves = SortListByPlayerScore(benchAndReserves);

			while (benchPlayersAndReserves.Count > 0)
			{
				if (benchIndex > 5)
					reserves.Add(benchPlayersAndReserves[0]);
				else
					bench.Add(benchPlayersAndReserves[0]);
				benchPlayersAndReserves.Remove(benchPlayersAndReserves[0]);
				benchIndex++;
			}

			if (!TeamIsValid())
				throw new ArgumentException("It's not a valid roster!");
		}

		private Player GetNextPlayerAvailable(List<Player> priority, List<Player> secondary, List<Player> tertiary,
			List<Player> fourthTier, List<Player> fifthTier, List<Player> sixTier, List<Player> sevenTier,
			List<Player> eightTier, List<Player> ninthTier, List<Player> tenthTier, List<Player> elevenTier)
		{
			Player p = null;
			if (priority.Count > 0)
			{
				p = priority[0];
				priority.Remove(p);
			}
			else if (secondary.Count > 0)
			{
				p = secondary[0];
				secondary.Remove(p);
			}
			else if (tertiary.Count > 0)
			{
				p = tertiary[0];
				tertiary.Remove(p);
			}
			else if (fourthTier.Count > 0)
			{
				p = fourthTier[0];
				fourthTier.Remove(p);
			}
			else if (fifthTier.Count > 0)
			{
				p = fifthTier[0];
				fifthTier.Remove(p);
			}
			else if (sixTier.Count > 0)
			{
				p = sixTier[0];
				sixTier.Remove(p);
			}
			else if (sevenTier.Count > 0)
			{
				p = sevenTier[0];
				sevenTier.Remove(p);
			}
			else if (eightTier.Count > 0)
			{
				p = eightTier[0];
				eightTier.Remove(p);
			}
			else if (ninthTier.Count > 0)
			{
				p = ninthTier[0];
				ninthTier.Remove(p);
			}
			else if (tenthTier.Count > 0)
			{
				p = tenthTier[0];
				tenthTier.Remove(p);
			}
			else if (elevenTier.Count > 0)
			{
				p = elevenTier[0];
				elevenTier.Remove(p);
			}
			else
			{
				p = new Player(-1, "Generic Reserve");
				p.InitializePlayerStats(LeagueName, Name);
				p.shooting = 0;
				p.passing = 0;
			}
			return p;
		}

		public bool TeamIsValid()
		{
			if (!DetermineIfAPlayerInArrayIsInvalid(onCamera))
				return false;
			if (!DetermineIfAPlayerInArrayIsInvalid(offCamera))
				return false;
			if (!PlayerIsValid(this.goalKeeper))
				return false;
			foreach (Player p in completeRoster)
			{
				int countNumberOfApperances = 0;
				if (onCamera.Contains(p))
					countNumberOfApperances++;
				if (offCamera.Contains(p))
					countNumberOfApperances++;
				if (bench.Contains(p))
					countNumberOfApperances++;
				if (reserves.Contains(p))
					countNumberOfApperances++;
				if (this.goalKeeper == p)
					countNumberOfApperances++;
				if (countNumberOfApperances != 1)
					return false;
			}
			return true;
		}

		private bool DetermineIfAPlayerInArrayIsInvalid(Player[] players)
		{
			foreach (Player p in players)
				if (!PlayerIsValid(p))
					return false;
			return true;
		}

		private bool PlayerIsValid(Player p)
		{
			return p.gamesOutDueToInjury == 0 || p.gamesOutDueToSuspension == 0;
		}

		private List<Player> SortListByPlayerScore(Dictionary<Player, int> positionSet)
		{
			List<KeyValuePair<Player, int>> myList = new List<KeyValuePair<Player, int>>(positionSet);
			myList.Sort(
				delegate(KeyValuePair<Player, int> firstPair,
				KeyValuePair<Player, int> nextPair)
				{
					return nextPair.Value.CompareTo(firstPair.Value);
				}
			);

			List<Player> players = new List<Player>();

			foreach (KeyValuePair<Player, int> plyr in myList)
			{
				players.Add(plyr.Key);
			}

			return players;
		}

		private int CalculatePlayerScore(Player player)
		{
			return player.overall;
		}

		private int CharacteristicGrade(bool characteristic)
		{
			if (characteristic)
				return 1;
			else
				return 0;
		}

		private int GradeForAbility(Enums.Ability ability)
		{
			if (ability == Enums.Ability.Amazing)
				return 2;
			if (ability == Enums.Ability.AboveAverage)
				return 1;
			if (ability == Enums.Ability.Average)
				return 0;
			return -1;
		}

		private int GradeForAbilityCard(Enums.Ability ability)
		{
			if (ability == Enums.Ability.Amazing)
				return -2;
			if (ability == Enums.Ability.AboveAverage)
				return -1;
			if (ability == Enums.Ability.Average)
				return 0;
			return 1;
		}

		private Enums.Ability SetGrade(int ability)
		{
			if (ability == 2)
				return Enums.Ability.Amazing;
			if (ability == 1)
				return Enums.Ability.AboveAverage;
			if (ability == 0)
				return Enums.Ability.Average;
			return Enums.Ability.BelowAverage;
		}
}
