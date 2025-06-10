using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Season
{
	Dictionary<Team, int> teamRank = new Dictionary<Team, int>();
		List<int> ranks = new List<int>();
		public Dictionary<Player, Team> playerTeamMap = new Dictionary<Player, Team>();
		public string[] teamRanksOutput = new string[4];
		private Dictionary<int, int> internationalBreakWeeks = new Dictionary<int, int>();
		public string TopTeamEPL = "";
		public Dictionary<string, List<List<Game>>> cupGames = new Dictionary<string, List<List<Game>>>();
		Dictionary<string, List<Team>> countryCupRoundOneTeams = new Dictionary<string, List<Team>>();
		Dictionary<string, List<Team>> countryCupRoundTwoTeams = new Dictionary<string, List<Team>>();
		Dictionary<string, int> cupRoundByCountry = new Dictionary<string, int>();
		DateTime earlierGame = DateTime.MaxValue;
		DateTime latestGame = DateTime.MinValue;
		public Dictionary<string, List<Team>> leagueDictionary = new Dictionary<string, List<Team>>();
		public List<Player> playersInLeague = new List<Player>();
		public Dictionary<string, Team> teamDictionary = new Dictionary<string, Team>();
		public Dictionary<string, Dictionary<DateTime, List<Game>>> countryFixtures = new Dictionary<string, Dictionary<DateTime, List<Game>>>();
		public Dictionary<string, Dictionary<DateTime, List<Game>>> leagueFixtures = new Dictionary<string, Dictionary<DateTime, List<Game>>>();
		public Dictionary<DateTime, List<string>> countriesLeagueMatchesScheduledOnDay = new Dictionary<DateTime, List<string>>();
		public Dictionary<DateTime, List<string>> countriesCupMatchesScheduledOnDay = new Dictionary<DateTime, List<string>>();
		public Dictionary<string, Dictionary<DateTime, List<Game>>> cupFixtures = new Dictionary<string, Dictionary<DateTime, List<Game>>>();
		private static Season m_instance = null;

		public List<Dictionary<DateTime, Dictionary<string, List<Game>>>> promotionPlayoffs =
			new List<Dictionary<DateTime, Dictionary<string, List<Game>>>>();

		public Dictionary<Game, Game> gameLeggedPairs = new Dictionary<Game, Game>();
		public Dictionary<string, Dictionary<Team, int>> teamRankInLeague = new Dictionary<string, Dictionary<Team, int>>();


		public DateTime seasonGameDate = DateTime.MaxValue;
		public DateTime viewableGameDate = DateTime.MaxValue;
		public DateTime startOfSeasonDate = DateTime.MaxValue;
		public DateTime endOfSeasonDate = DateTime.MinValue;

		public Team m_UefaChampion = null;
		public Team m_EuropaChampion = null;
		public Team m_TrebleWinner = null;

		public Dictionary<string, Team> cupChampions = new Dictionary<string, Team>();
		public Dictionary<string, Team> promotionalPlayoffWinners = new Dictionary<string, Team>();

		public Dictionary<string, string> countryCupNames = new Dictionary<string, string>();
		public Dictionary<DateTime, Dictionary<string, string>> countryLegName = new Dictionary<DateTime, Dictionary<string, string>>();

		public List<Dictionary<string, Team>> leagueChampions = new List<Dictionary<string, Team>>();
		public List<Dictionary<string, List<Team>>> leaguePromotions = new List<Dictionary<string, List<Team>>>();
		public List<Dictionary<string, List<Team>>> leagueDemotions = new List<Dictionary<string, List<Team>>>();

		public static Season Instance
		{
			get
			{
				if (m_instance == null)
				{
					m_instance = new Season();
				}
				return m_instance;
			}
		}

		public Team TrebleWinner
		{
			get
			{
				return m_TrebleWinner;
			}
			set
			{
				m_TrebleWinner = value;
			}
		}

		public Team CupChampion(string country)
		{
			Team cupChampion = null;
			if (!cupChampions.ContainsKey(country))
			{
				if (Season.Instance.cupGames.ContainsKey(country))
				{
					List<List<Game>> cupGames = Season.Instance.cupGames[country];
					if (cupGames[cupGames.Count - 1].Count == 1)
					{
						if (cupGames[cupGames.Count - 1][0].GamePlayed)
						{
							cupChampion = cupGames[cupGames.Count - 1][0].Winner;
						}
					}
				}
			}
			else
			{
				cupChampion = cupChampions[country];
			}
			if (cupChampion != null)
			{
				if (!cupChampions.ContainsKey(country))
					cupChampions.Add(country, cupChampion);
			}
			return cupChampion;
		}

		public Team UEFAChampion
		{
			get
			{
				if (m_UefaChampion == null)
				{
					if (Season.Instance.cupGames.ContainsKey("UEFA Champions League"))
					{
						List<List<Game>> UEFAGames = Season.Instance.cupGames["UEFA Champions League"];
						if (UEFAGames[UEFAGames.Count - 1].Count == 1)
						{
							if (UEFAGames[UEFAGames.Count - 1][0].GamePlayed)
							{
								m_UefaChampion = UEFAGames[UEFAGames.Count - 1][0].Winner;
							}
						}
					}
				}
				return m_UefaChampion;
			}
		}

		public Team EuropChampion
		{
			get
			{
				if (m_EuropaChampion == null)
				{
					if (Season.Instance.cupGames.ContainsKey("UEFA Europa League"))
					{
						List<List<Game>> UEFAGames = Season.Instance.cupGames["UEFA Europa League"];
						if (UEFAGames[UEFAGames.Count - 1].Count == 1)
						{
							if (UEFAGames[UEFAGames.Count - 1][0].GamePlayed)
							{
								m_EuropaChampion = UEFAGames[UEFAGames.Count - 1][0].Winner;
							}
						}
					}
				}
				return m_EuropaChampion;
			}
		}

		public Team PromotionPlayoffWinner(string league)
		{
			Team promotionalPlayoffWinner = null;
			if (!this.promotionalPlayoffWinners.ContainsKey(league))
			{
				if (Season.Instance.promotionPlayoffs.Count > 0)
				{
					Dictionary<DateTime, Dictionary<string, List<Game>>> dt = Season.Instance.promotionPlayoffs[Season.Instance.promotionPlayoffs.Count - 1];
					DateTime date = DateTime.MinValue;
					foreach (KeyValuePair<DateTime, Dictionary<string, List<Game>>> entry in dt)
					{
						date = entry.Key;
					}
					if (Season.Instance.promotionPlayoffs[Season.Instance.promotionPlayoffs.Count - 1][date].ContainsKey(league))
					{
						List<Game> games = Season.Instance.promotionPlayoffs[Season.Instance.promotionPlayoffs.Count - 1][date][league];
						if (games.Count == 1 && games[games.Count - 1].GamePlayed)
						{
							promotionalPlayoffWinner = games[games.Count - 1].Winner;
						}
					}
				}
			}
			else
			{
				promotionalPlayoffWinner = promotionalPlayoffWinners[league];
			}
			if (promotionalPlayoffWinner != null)
			{
				if (!promotionalPlayoffWinners.ContainsKey(league))
					promotionalPlayoffWinners.Add(league, promotionalPlayoffWinner);
			}
			return promotionalPlayoffWinner;
		}

		public Season()
		{
			// International breaks are universal so we should only schedule them once.
			Die d4 = new Die(4);
			internationalBreakWeeks.Add(9, d4.Roll()); // september
			internationalBreakWeeks.Add(10, d4.Roll()); // october
			internationalBreakWeeks.Add(11, d4.Roll()); // november
			internationalBreakWeeks.Add(12, 4); // december
			internationalBreakWeeks.Add(3, d4.Roll()); // march
		}

		public void ScheduleSeason()
		{
			countryCupNames.Add("England", "F.A. Cup");
			countryCupNames.Add("Italy", "Coppa Italia");
			countryCupNames.Add("Spain", "Copa del Rey");
			countryCupNames.Add("France", "Coupe de France");
			countryCupNames.Add("Germany", "DFB-Pokal");
			countryCupNames.Add("Netherlands", "KNVB Cup");
			countryCupNames.Add("Portugal", "Taça de Portugal");
			countryCupNames.Add("Sweden", "Svenska Cupen");
			countryCupNames.Add("Belgium", "Belgium Cup");
			countryCupNames.Add("Turkey", "Turkish Cup");
			countryCupNames.Add("Norway", "Norweigan Cup");
			countryCupNames.Add("Scotland", "Scottish Cup");
			countryCupNames.Add("Poland", "Polish Cup");
			countryCupNames.Add("Denmark", "Danish Cup");
			countryCupNames.Add("Switzerland", "Swiss Cup");
			countryCupNames.Add("Ireland", "FAI Cup");
			countryCupNames.Add("Romania", "Cupa României");
			countryCupNames.Add("Austria", "Austria Cup");
			countryCupNames.Add("UEFA Champions League", "UEFA Champions League");
			countryCupNames.Add("UEFA Europa League", "UEFA Europa League");
			foreach (string country in TeamRepository.Instance.EuropeCountries)
			{
				List<League> leagues = new List<League>();
				cupRoundByCountry.Add(country, 1);
				leagues = TeamRepository.Instance.countries[TeamRepository.Instance.countryIndex[country]].leagues;
				Dictionary<DateTime, List<Game>> allLeagueFixtures = new Dictionary<DateTime, List<Game>>();
				for (int i = 0; i < leagues.Count; i++)
				{
					List<Team> teams = leagues[i].teams;
					foreach (Team t in teams)
						teamDictionary.Add(t.Name, t);
					leagueDictionary.Add(leagues[i].LeagueName, teams);
					ShuffleTeams(teams);
					Dictionary<DateTime, List<Game>> fixturesForLeague = new Dictionary<DateTime, List<Game>>();
					ScheduleDoubleRoundRobin(teams, i == 0, allLeagueFixtures, fixturesForLeague, country, leagues[i].LeagueName);
					leagueFixtures.Add(leagues[i].LeagueName, fixturesForLeague);
				}
				countryFixtures.Add(country, allLeagueFixtures);

				Dictionary<DateTime, List<Game>> fixturesCup = new Dictionary<DateTime, List<Game>>();
				ScheduleCupRound1(fixturesCup, country);
				foreach (KeyValuePair<DateTime, List<Game>> entry in allLeagueFixtures)
				{
					if (entry.Key < seasonGameDate)
					{
						seasonGameDate = entry.Key;
						viewableGameDate = entry.Key;
						startOfSeasonDate = entry.Key;
					}
					if (entry.Key > endOfSeasonDate)
					{
						endOfSeasonDate = entry.Key;
					}
				}
				foreach (KeyValuePair<DateTime, List<Game>> entry in cupFixtures[country])
				{
					if (entry.Key < seasonGameDate)
					{
						seasonGameDate = entry.Key;
						viewableGameDate = entry.Key;
						startOfSeasonDate = entry.Key;
					}
					if (entry.Key > endOfSeasonDate)
					{
						endOfSeasonDate = entry.Key;
					}
				}
			}

			Dictionary<DateTime, List<Game>> uefaChampionsLeagueFixtures = new Dictionary<DateTime, List<Game>>();
			ScheduleUEFAMatches(uefaChampionsLeagueFixtures, "UEFA Champions League");
			countryFixtures.Add("UEFA Champions League", uefaChampionsLeagueFixtures);
			cupRoundByCountry.Add("UEFA Champions League", 1);

			Dictionary<DateTime, List<Game>> uefaEuropaLeagueFixtures = new Dictionary<DateTime, List<Game>>();
			ScheduleUEFAMatches(uefaEuropaLeagueFixtures, "UEFA Europa League");
			countryFixtures.Add("UEFA Europa League", uefaEuropaLeagueFixtures);
			cupRoundByCountry.Add("UEFA Europa League", 1);
		}

		public void DetermineFinalLeagueResults()
		{
			if (leagueChampions.Count == 0)
			{
				for (int i = 0; i < TeamRepository.Instance.EuropeCountries.Length; i++)
				{
					string country = TeamRepository.Instance.EuropeCountries[i];
					List<League> l = TeamRepository.Instance.countries[TeamRepository.Instance.countryIndex[country]].leagues;
					for (int j = 0; j < l.Count; j++)
					{
						Dictionary<string, Team> leagueChampion = new Dictionary<string, Team>();
						SortTeamBySeasonStandingsDESC(l[j].teams, l[j].LeagueName);
						leagueChampion.Add(l[j].LeagueName, l[j].teams[0]);
						this.leagueChampions.Add(leagueChampion);
						if (l[j].teams[0] == Season.Instance.CupChampion(country) &&
							l[j].teams[0] == Season.Instance.UEFAChampion)
						{
							Season.Instance.TrebleWinner = l[j].teams[0];
						}

						if (l.Count > 0)
						{
							if ((j + 1) < l.Count)
							{
								Dictionary<string, List<Team>> leagueDemotions = new Dictionary<string, List<Team>>();
								List<Team> demotedTeams = new List<Team>();
								int teamsCount = l[j].teams.Count;
								demotedTeams.Add(l[j].teams[teamsCount - 1]);
								demotedTeams.Add(l[j].teams[teamsCount - 2]);
								demotedTeams.Add(l[j].teams[teamsCount - 3]);
								if (country == "England" && j == 2)
								{
									demotedTeams.Add(l[j].teams[teamsCount - 4]);
								}
								leagueDemotions.Add(l[j].LeagueName, demotedTeams);
								this.leagueDemotions.Add(leagueDemotions);
							}
							if (j > 0)
							{
								Dictionary<string, List<Team>> leaguePromotions = new Dictionary<string, List<Team>>();
								List<Team> promotedTeams = new List<Team>();
								int index = 0;
								promotedTeams.Add(l[j].teams[index++]);
								promotedTeams.Add(l[j].teams[index++]);
								if (j == 3 && country == "England")
								{
									promotedTeams.Add(l[j].teams[index++]);
								}
								if (PromotionPlayoffWinner(l[j].LeagueName) != null)
								{
									promotedTeams.Add(PromotionPlayoffWinner(l[j].LeagueName));
								}
								else
								{
									promotedTeams.Add(l[j].teams[index++]);
								}
								leaguePromotions.Add(l[j].LeagueName, promotedTeams);
								this.leaguePromotions.Add(leaguePromotions);
							}
						}
					}
				}
			}
		}

		private void ScheduleUEFAMatches(Dictionary<DateTime, List<Game>> allLeagueFixtures, string championshipTpe)
		{
			List<League> UEFAleagues = TeamRepository.Instance.countries[TeamRepository.Instance.countryIndex[championshipTpe]].leagues;
			List<Team> UEFATeams = UEFAleagues[0].teams;
			leagueDictionary.Add(UEFAleagues[0].LeagueName, UEFATeams);
			ShuffleTeams(UEFATeams);
			Stack<Team> teamStack = new Stack<Team>();

			int month = 9;
			int weekMonthCount = 0;
			bool scheduleTuesday = false;
			bool tuesdayScheduledLast = false;

			foreach (Team t in UEFATeams)
			{
				teamStack.Push(t);
			}

			while (teamStack.Count > 0)
			{
				DateTime date = DateTime.Now;
				date = GETEUefaDate(date);
				List<Team> teamsInGroup = new List<Team>();
				for (int i = 0; i < 9; i++)
				{
					teamsInGroup.Add(teamStack.Pop());
				}
				Dictionary<DateTime, List<Game>> fixturesForLeague = new Dictionary<DateTime, List<Game>>();
				this.ScheduleSingleRoundRobin(teamsInGroup, true, allLeagueFixtures, fixturesForLeague, championshipTpe, false,
					ref date, ref scheduleTuesday, ref tuesdayScheduledLast, ref month, ref weekMonthCount, championshipTpe);
			}

			leagueFixtures.Add(championshipTpe, allLeagueFixtures);
		}

		public void SimulateSeason()
		{
			SimulateSeason(countryFixtures["England"], cupFixtures["England"]);
			Dictionary<int, List<Team>> championsAndRelegations = new Dictionary<int, List<Team>>();
			List<League> leagues = TeamRepository.Instance.countries[TeamRepository.Instance.countryIndex["England"]].leagues;
			for (int i = 0; i < leagues.Count; i++)
			{
				championsAndRelegations = new Dictionary<int, List<Team>>();
				DetermineEndOfSeasonResults(leagues[i].teams, ref teamRanksOutput[i], championsAndRelegations, "Premier League");
				if (i == 0) TopTeamEPL = leagues[i].teams[0].Name;
			}
		}

		public void ResetSeason()
		{
			m_instance = new Season();
		}

		public List<string> GetTeamsAsListOfStrings(string leagueName)
		{
			List<Team> teams = leagueDictionary[leagueName];
			List<string> teamStrings = new List<string>();
			teamStrings.Add("(all)");
			foreach (Team t in teams)
				teamStrings.Add(t.Name);
			return teamStrings;
		}

		public string GetStatString(string leagueName, string teamName)
		{
			string teamRankString = "";
			List<Player> players = new List<Player>();
			if (teamDictionary.ContainsKey(teamName))
			{
				foreach (Player p in teamDictionary[teamName].completeRoster)
				{
					if (TeamRepository.Instance.teamNameDict[teamName].playerStats[leagueName].ContainsKey(p.ID))
					{
						Player pClone = p.Clone();
						pClone.InitializePlayerStats(leagueName, teamName);
						pClone.LeagueTeamSeasonStats[leagueName][teamName] = TeamRepository.Instance.teamNameDict[teamName].playerStats[leagueName][pClone.ID];
						players.Add(pClone);
					}
				}
			}
			else
			{
				List<Team> teams = leagueDictionary[leagueName];
				foreach (Team t in teams)
				{
					foreach (Player p in t.completeRoster)
					{
						if (TeamRepository.Instance.teamNameDict[t.Name].playerStats[leagueName].ContainsKey(p.ID))
						{
							Player pClone = p.Clone();
							pClone.InitializePlayerStats(leagueName, t.Name);
							pClone.LeagueTeamSeasonStats[leagueName][t.Name] = TeamRepository.Instance.teamNameDict[t.Name].playerStats[leagueName][pClone.ID];
							players.Add(pClone);
						}
					}
				}
			}

			SortPlayersLeague(players, false, false, false, false, false, false, false, false, false, true, leagueName,
				teamName);
			teamRankString += "Matches Played:" + System.Environment.NewLine;
			for (int i = 0; i < 20; i++)
			{
				if (players[i].LeagueTeamSeasonStats[leagueName][teamName].matchesPlayed > 0)
					teamRankString += players[i].fullName + "(" + (playerTeamMap[players[i]].Name) + "): "
						+ players[i].LeagueTeamSeasonStats[leagueName][teamName].matchesPlayed + System.Environment.NewLine;
			}

			SortPlayersLeague(players, true, false, false, false, false, false, false, false, false, false, leagueName,
				teamName);
			teamRankString += "Goals leaders:" + System.Environment.NewLine;
			for (int i = 0; i < 20; i++)
			{
				if (players[i].LeagueTeamSeasonStats[leagueName][teamName].goals > 0)
					teamRankString += players[i].fullName + "(" + (playerTeamMap[players[i]].Name) + "): "
						+ players[i].LeagueTeamSeasonStats[leagueName][teamName].goals + System.Environment.NewLine;
			}
			SortPlayersLeague(players, false, false, false, false, false, false, true, false, false, false, leagueName,
				teamName);
			teamRankString += System.Environment.NewLine;
			teamRankString += "Goals efficiency leaders (minimum ten shots):" + System.Environment.NewLine;
			for (int i = 0; i < 20; i++)
			{
				if (players[i].LeagueTeamSeasonStats[leagueName][teamName].shotsTotal > 0)
					teamRankString += players[i].fullName + "(" + (playerTeamMap[players[i]].Name) + "): " +
						(double)players[i].LeagueTeamSeasonStats[leagueName][teamName].goals /
						(double)(players[i].LeagueTeamSeasonStats[leagueName][teamName].goals + players[i].LeagueTeamSeasonStats[leagueName][teamName].shotsTotal)
						+ System.Environment.NewLine;
			}
			SortPlayersLeague(players, false, true, false, false, false, false, false, false, false, false, leagueName,
				teamName);
			teamRankString += System.Environment.NewLine;
			teamRankString += "Assists leaders:" + System.Environment.NewLine;
			for (int i = 0; i < 20; i++)
			{
				if (players[i].LeagueTeamSeasonStats[leagueName][teamName].assists > 0)
					teamRankString += players[i].fullName + "(" + (playerTeamMap[players[i]].Name) + "): "
						+ players[i].LeagueTeamSeasonStats[leagueName][teamName].assists + System.Environment.NewLine;
			}
			SortPlayersLeague(players, false, false, true, false, false, false, false, false, false, false, leagueName,
				teamName);
			teamRankString += System.Environment.NewLine;
			teamRankString += "Most carded (yellow):" + System.Environment.NewLine;
			for (int i = 0; i < 20; i++)
			{
				if (players[i].LeagueTeamSeasonStats[leagueName][teamName].yellowCards > 0)
					teamRankString += players[i].fullName + "(" + (playerTeamMap[players[i]].Name) + "): "
						+ players[i].LeagueTeamSeasonStats[leagueName][teamName].yellowCards + System.Environment.NewLine;
			}
			SortPlayersLeague(players, false, false, false, true, false, false, false, false, false, false, leagueName,
				teamName);
			teamRankString += System.Environment.NewLine;
			teamRankString += "Most carded (red):" + System.Environment.NewLine;
			for (int i = 0; i < 20; i++)
			{
				if (players[i].LeagueTeamSeasonStats[leagueName][teamName].redCards > 0)
					teamRankString += players[i].fullName + "(" + (playerTeamMap[players[i]].Name) + "): "
						+ players[i].LeagueTeamSeasonStats[leagueName][teamName].redCards + System.Environment.NewLine;
			}
			SortPlayersLeague(players, false, false, false, false, false, false, false, false, true, false, leagueName,
				teamName);
			teamRankString += System.Environment.NewLine;
			teamRankString += "Clean Sheets Leaders:" + System.Environment.NewLine;
			for (int i = 0; i < 20; i++)
			{
				if (players[i].LeagueTeamSeasonStats[leagueName][teamName].cleansheets > 0)
				{
					teamRankString += players[i].fullName + "(" + (playerTeamMap[players[i]].Name) + "): "
						+ players[i].LeagueTeamSeasonStats[leagueName][teamName].cleansheets + System.Environment.NewLine;
				}
			}
			SortPlayersLeague(players, false, false, false, false, true, false, false, false, false, false, leagueName,
				teamName);
			teamRankString += System.Environment.NewLine;
			teamRankString += "Save leaders:" + System.Environment.NewLine;
			for (int i = 0; i < 20; i++)
			{
				if (players[i].LeagueTeamSeasonStats[leagueName][teamName].Saves > 0)
				{
					double savePct = (double)players[i].LeagueTeamSeasonStats[leagueName][teamName].Saves / (double)(players[i].LeagueTeamSeasonStats[leagueName][teamName].goalsConceded + players[i].LeagueTeamSeasonStats[leagueName][teamName].Saves);
					teamRankString += players[i].fullName + "(" + (playerTeamMap[players[i]].Name) + "): "
						+ players[i].LeagueTeamSeasonStats[leagueName][teamName].Saves + System.Environment.NewLine;
				}
			}
			SortPlayersLeague(players, false, false, false, false, false, false, false, true, false, false, leagueName, teamName);
			teamRankString += System.Environment.NewLine;
			teamRankString += "Most goals conceded:" + System.Environment.NewLine;
			for (int i = 0; i < 20; i++)
			{
				if (players[i].LeagueTeamSeasonStats[leagueName][teamName].goalsConceded > 0)
				{
					teamRankString += players[i].fullName + "(" + (playerTeamMap[players[i]].Name) + "): "
						+ players[i].LeagueTeamSeasonStats[leagueName][teamName].goalsConceded + System.Environment.NewLine;
				}
			}
			SortPlayersLeague(players, false, false, false, false, false, true, false, false, false, false, leagueName,
				teamName);
			teamRankString += System.Environment.NewLine;
			teamRankString += "Best save pct:" + System.Environment.NewLine;
			for (int i = 0; i < 20; i++)
			{
				if (players[i].LeagueTeamSeasonStats[leagueName][teamName].Saves > 0)
					teamRankString += players[i].fullName + "(" + (playerTeamMap[players[i]].Name) + "): " +
						(double)players[i].LeagueTeamSeasonStats[leagueName][teamName].Saves /
						(double)(players[i].LeagueTeamSeasonStats[leagueName][teamName].goalsConceded + players[i].LeagueTeamSeasonStats[leagueName][teamName].Saves)
						+ System.Environment.NewLine;
			}

			return teamRankString;
		}

		private void DetermineEndOfSeasonResults(List<Team> leagueTeams, ref string teamRankString,
			Dictionary<int, List<Team>> championsAndRelegations, string leagueName)
		{
			int totalGoals = 0;
			List<Player> everyPlayerInLeague = new List<Player>();
			Dictionary<int, string> rankStringStandings = new Dictionary<int, string>();

			RankTeamsSeasonStandings(leagueTeams, rankStringStandings, championsAndRelegations, leagueName);
			DateTime date = DateTime.Now;
			teamRankString += date.Year.ToString() + " " + leagueName + " Season: " + System.Environment.NewLine + System.Environment.NewLine;

			foreach (Team team in leagueTeams)
			{
				team.CreateSeasonStats(leagueName);

				foreach (Player p in team.completeRoster)
				{
					playersInLeague.Add(p);
					playerTeamMap.Add(p, team);
				}
				teamRankString += rankStringStandings[teamRank[team]].ToString() +
					team.Name + ", Matches Played: " + (team.seasonStats[leagueName].wins + team.seasonStats[leagueName].losses + team.seasonStats[leagueName].draws).ToString() + ", Pts: " + CalculateTeamPoints(team, leagueName).ToString() + ", Wins: "
					+ team.seasonStats[leagueName].wins.ToString() + ", Draws: " + team.seasonStats[leagueName].draws.ToString() + ", Losses: " + team.seasonStats[leagueName].losses.ToString() +
					", Goals for: " + team.seasonStats[leagueName].goals.ToString() + ", Goals against: " + team.seasonStats[leagueName].goalsConceded.ToString() + ", Goal Differential: " +
					(team.seasonStats[leagueName].goals - team.seasonStats[leagueName].goalsConceded).ToString() + System.Environment.NewLine;
				totalGoals += team.seasonStats[leagueName].goals;
			}
			teamRankString += "Total goals: " + totalGoals.ToString() + "(" + ((double)totalGoals / (38.0 * 10.0)).ToString() + " avg.)" +
				System.Environment.NewLine + System.Environment.NewLine;
		}

		private void SimulateSeason(Dictionary<DateTime, List<Game>> leagueFixtures,
			Dictionary<DateTime, List<Game>> faCupFixtures)
		{
			DateTime currentDate = earlierGame;
			int month = earlierGame.Month;

			while (Season.Instance.seasonGameDate <= latestGame)
			{
				// break for loop when we need to schedule FA cup games, schedule them, and then 
				// continue the loop.
				if (leagueFixtures.ContainsKey(currentDate))
				{
					List<Game> leagueWeekGames = leagueFixtures[currentDate];
					SimulateGames(leagueWeekGames);
				}

				if (faCupFixtures.ContainsKey(currentDate))
				{
					List<Game> faCupWeekFixtures = faCupFixtures[currentDate];
					SimulateGames(faCupWeekFixtures);
				}

				Season.Instance.AddDays(Season.Instance.seasonGameDate, 1);
			}
		}

		public void AddDays(DateTime date, int numberOfDays)
		{
			Season.Instance.seasonGameDate = Season.Instance.seasonGameDate.AddDays(1);
			int month = Season.Instance.seasonGameDate.Month;
			foreach (KeyValuePair<string, string> entry in Season.Instance.countryCupNames)
			{
				if (entry.Key == "UEFA Champions League" || entry.Key == "UEFA Europa League")
				{
					if (month == 12 || month == 2 || month == 3 || month == 4 || month == 5)
					{
						if (Season.Instance.seasonGameDate.Day == 1)
						{
							if (!cupFixtures.ContainsKey(entry.Key))
							{
								Dictionary<DateTime, List<Game>> CupFixtures = new Dictionary<DateTime, List<Game>>();
								cupFixtures.Add(entry.Key, CupFixtures);
							}
							if (month == 12)
							{
								ScheduleEuropeKnockoutRounds(cupFixtures[entry.Key], entry.Key);
							}
							else
							{
								ScheduleCupRound(cupFixtures[entry.Key], entry.Key, month, false, true, true, DayOfWeek.Friday);
							}
						}
					}
				}
				else
				{
					if (month == 11 || month == 12 || month == 1 || month == 3 || month == 4 || month == 5)
					{
						if (Season.Instance.seasonGameDate.Day == 1)
						{
							if (month != 11 || entry.Key == "England") ScheduleCupRound(cupFixtures[entry.Key], entry.Key, month, true, false, false, DayOfWeek.Wednesday);
						}

					}
				}
			}
			if (Season.Instance.seasonGameDate > latestGame)
			{
				if (promotionPlayoffs.Count <= 2)
				{
					if (promotionPlayoffs.Count == 0)
					{
						SchedulePromotionPlayOffRound(promotionPlayoffs.Count + 1, latestGame.AddDays(7));
					}
					else
					{
						if (!promotionPlayoffs[0].ContainsKey(Season.Instance.latestGame.AddDays(7)))
						{
							SchedulePromotionPlayOffRound(promotionPlayoffs.Count + 1, latestGame.AddDays(7));
						}
					}
				}
				else
				{
					if (Season.Instance.PromotionPlayoffWinner("EFL Championship") != null)
					{
						Season.Instance.DetermineFinalLeagueResults();
					}
					if (Season.Instance.UEFAChampion != null)
					{
						if (TeamRepository.Instance.UEFAChampionsLeagueNextSeason.Count == 0)
						{
							TeamRepository.Instance.DetermineEuropeanTeams();
						}
					}
				}
			}
		}

		private void SchedulePromotionPlayOffRound(int round, DateTime newLatestGame)
		{
			List<League> leagues = TeamRepository.Instance.countries[TeamRepository.Instance.countryIndex["England"]].leagues;
			List<Team> eflChampionshipTeams = leagues[1].teams;
			List<Team> eflLeagueOneTeams = leagues[2].teams;
			List<Team> eflLeagueTwoTeams = leagues[3].teams;

			SortTeamBySeasonStandingsDESC(eflChampionshipTeams, "EFL Championship");
			SortTeamBySeasonStandingsDESC(eflLeagueOneTeams, "EFL League One");
			SortTeamBySeasonStandingsDESC(eflLeagueTwoTeams, "EFL League Two");
			DateTime currentLatest = newLatestGame;
			List<Game> games = new List<Game>();
			Dictionary<DateTime, Dictionary<string, List<Game>>> dateOfGames = new Dictionary<DateTime, Dictionary<string, List<Game>>>();
			Dictionary<string, List<Game>> leagueRound = new Dictionary<string, List<Game>>();
			Dictionary<DateTime, List<Game>> fixturesForLeague = countryFixtures["England"];

			if (round == 1)
			{
				games.Add(new Game(eflChampionshipTeams[5], eflChampionshipTeams[2], 45, false, false, false, 3, currentLatest,
					"EFL Championship", round, "EFL Championship Playoff Round:",
					true, false, true));
				games.Add(new Game(eflChampionshipTeams[4], eflChampionshipTeams[3], 45, false, false, false, 3, currentLatest,
					"EFL Championship", round, "EFL Championship Playoff Round:",
					true, false, true));

				leagueRound.Add("EFL Championship", games);

				games = new List<Game>();
				games.Add(new Game(eflLeagueOneTeams[5], eflLeagueOneTeams[2], 45, false, false, false, 3, currentLatest,
					"EFL League One", round, "EFL League One Playoff Round:",
					true, false, true));
				games.Add(new Game(eflLeagueOneTeams[4], eflLeagueOneTeams[3], 45, false, false, false, 3, currentLatest,
					"EFL League One", round, "EFL League One Playoff Round:",
					true, false, true));
				leagueRound.Add("EFL League One", games);

				games = new List<Game>();
				games.Add(new Game(eflLeagueTwoTeams[6], eflLeagueTwoTeams[3], 45, false, false, false, 3, currentLatest,
					"EFL League Two", round, "EFL League Two Playoff Round:",
					true, false, true));
				games.Add(new Game(eflLeagueTwoTeams[5], eflLeagueTwoTeams[4], 45, false, false, false, 3, currentLatest,
					"EFL League Two", round, "EFL League Two Playoff Round:",
					true, false, true));
				leagueRound.Add("EFL League Two", games);
				dateOfGames.Add(currentLatest, leagueRound);

				promotionPlayoffs.Add(dateOfGames);
			}
			if (round == 2)
			{
				List<Game> round1Games = promotionPlayoffs[0][latestGame]["EFL Championship"];

				Game game = new Game(eflChampionshipTeams[2], eflChampionshipTeams[5], 45, false, true, false, 3, currentLatest,
					"EFL Championship", round, "EFL Championship Playoff Round:",
					true, false, true);

				game.awayFirstLeg = round1Games[0].homeScore;
				game.homeFirstLeg = round1Games[0].awayScore;
				games.Add(game);
				game = new Game(eflChampionshipTeams[3], eflChampionshipTeams[4], 45, false, true, false, 3, currentLatest,
					"EFL Championship", round, "EFL Championship Playoff Round:",
					true, false, true);
				game.awayFirstLeg = round1Games[1].homeScore;
				game.homeFirstLeg = round1Games[1].awayScore;
				games.Add(game);
				leagueRound.Add("EFL Championship", games);

				round1Games = promotionPlayoffs[0][latestGame]["EFL League One"];
				games = new List<Game>();
				game = new Game(eflLeagueOneTeams[2], eflLeagueOneTeams[5], 45, false, true, false, 3, currentLatest,
					"EFL League One", round, "EFL League One Playoff Round:",
					true, false, true);
				game.awayFirstLeg = round1Games[0].homeScore;
				game.homeFirstLeg = round1Games[0].awayScore;
				games.Add(game);
				game = new Game(eflLeagueOneTeams[3], eflLeagueOneTeams[4], 45, false, true, false, 3, currentLatest,
					"EFL League One", round, "EFL League One Playoff Round:",
					true, false, true);
				game.awayFirstLeg = round1Games[1].homeScore;
				game.homeFirstLeg = round1Games[1].awayScore;
				games.Add(game);
				leagueRound.Add("EFL League One", games);

				round1Games = promotionPlayoffs[0][latestGame]["EFL League Two"];
				games = new List<Game>();
				game = new Game(eflLeagueTwoTeams[3], eflLeagueTwoTeams[6], 45, false, true, false, 3, currentLatest,
					"EFL League Two", round, "EFL League Two Playoff Round:",
					true, false, true);
				game.awayFirstLeg = round1Games[0].homeScore;
				game.homeFirstLeg = round1Games[0].awayScore;
				games.Add(game);
				game = new Game(eflLeagueTwoTeams[4], eflLeagueTwoTeams[5], 45, false, true, false, 3, currentLatest,
					"EFL League Two", round, "EFL League Two Playoff Round:",
					true, false, true);
				game.awayFirstLeg = round1Games[1].homeScore;
				game.homeFirstLeg = round1Games[1].awayScore;
				games.Add(game);
				leagueRound.Add("EFL League Two", games);

				dateOfGames.Add(currentLatest, leagueRound);
				promotionPlayoffs.Add(dateOfGames);
			}
			if (round == 3)
			{
				List<Game> round1Games = promotionPlayoffs[1][latestGame]["EFL Championship"];

				Game game = new Game(round1Games[0].Winner, round1Games[1].Winner, 45, false, true, false, 3, currentLatest,
					"EFL Championship", round, "EFL Championship Playoff Round:",
					true, false, true);
				games.Add(game);
				leagueRound.Add("EFL Championship", games);

				round1Games = promotionPlayoffs[1][latestGame]["EFL League One"];
				games = new List<Game>();
				game = new Game(round1Games[0].Winner, round1Games[1].Winner, 45, false, true, false, 3, currentLatest,
					"EFL League One", round, "EFL League One Playoff Round:",
					true, false, true);
				games.Add(game);
				leagueRound.Add("EFL League One", games);

				round1Games = promotionPlayoffs[1][latestGame]["EFL League Two"];
				games = new List<Game>();
				game = new Game(round1Games[0].Winner, round1Games[1].Winner, 45, false, true, false, 3, currentLatest,
					"EFL League Two", round, "EFL League Two Playoff Round:",
					true, false, true);
				games.Add(game);
				leagueRound.Add("EFL League Two", games);

				dateOfGames.Add(currentLatest, leagueRound);
				promotionPlayoffs.Add(dateOfGames);
			}

			//List<string> countries = new List<string>();

			//fixturesForLeague.Add(currentLatest, games);

			//if (!this.countriesLeagueMatchesScheduledOnDay.ContainsKey(currentLatest))
			//{
			//    countries.Add("England");
			//    this.countriesLeagueMatchesScheduledOnDay.Add(currentLatest, countries);
			//}
			//else
			//{
			//    countries = this.countriesLeagueMatchesScheduledOnDay[currentLatest];
			//    if (!countries.Contains("England")) countries.Add("England");
			//    this.countriesLeagueMatchesScheduledOnDay[currentLatest] = countries;
			//}

			if (round < 3) latestGame = newLatestGame;
		}

		public void SimulateGame(Game g)
		{
			LowerGamesOut(g.homeTeam);
			LowerGamesOut(g.awayTeam);
			g.homeTeam.ResetStats();
			g.homeTeam.ConfigureRoster();
			g.awayTeam.ResetStats();
			g.awayTeam.ConfigureRoster();
			if (!g.GamePlayed)
			{
				if (Season.Instance.gameLeggedPairs.ContainsKey(g))
				{
					Game game1 = Season.Instance.gameLeggedPairs[g];
					g.awayFirstLeg = game1.homeScore;
					g.homeFirstLeg = game1.awayScore;
				}
				g.BeginGame(false);
			}
		}

		public void SimulateGames(List<Game> games)
		{
			foreach (Game g in games)
				SimulateGame(g);
		}

		public void LowerGamesOut(Team team)
		{
			foreach (Player p in team.completeRoster)
			{
				p.gamesOutDueToInjury--; // Be sure to add one game to injury so that when I hit next, it'll subtract the extra
				p.gamesOutDueToSuspension--; // Be sure to add one game to suspension, it'll subtract the extra
				if (p.gamesOutDueToInjury < 0)
					p.gamesOutDueToInjury = 0;
				if (p.gamesOutDueToSuspension < 0)
					p.gamesOutDueToSuspension = 0;
			}
		}

		private void RankTeamsSeasonStandings(List<Team> teams, Dictionary<int, string> rankStringStandings,
			Dictionary<int, List<Team>> championsAndRelegations, string leagueName)
		{
			int rank = 1;
			SortTeamBySeasonStandingsDESC(teams, leagueName);
			int previousPoint = this.CalculateTeamPoints(teams[0], leagueName);
			int previousGoalDiff = teams[0].seasonStats[leagueName].goals - teams[0].seasonStats[leagueName].goalsConceded;
			int previousGoalAgainst = teams[0].seasonStats[leagueName].goalsConceded;
			List<Team> teamsInGivenRank = new List<Team>();
			teamsInGivenRank.Add(teams[0]);
			teamRank.Add(teams[0], rank);
			for (int i = 1; i < teams.Count; i++)
			{
				bool increaseRank = true;
				if (this.CalculateTeamPoints(teams[i], leagueName) == previousPoint)
					if ((teams[i].seasonStats[leagueName].goals - teams[i].seasonStats[leagueName].goalsConceded) == previousGoalDiff)
						if (teams[i].seasonStats[leagueName].goalsConceded == previousGoalAgainst)
							increaseRank = false;
				previousPoint = this.CalculateTeamPoints(teams[i], leagueName);
				previousGoalDiff = teams[i].seasonStats[leagueName].goals - teams[i].seasonStats[leagueName].goalsConceded;
				previousGoalAgainst = teams[i].seasonStats[leagueName].goalsConceded;
				if (increaseRank)
				{
					championsAndRelegations.Add(rank, teamsInGivenRank);
					ranks.Add(rank);
					string rnk = "";
					if (teamsInGivenRank.Count > 1)
						rnk += "T. ";
					rankStringStandings.Add(rank, rnk + rank.ToString() + ". ");
					rank = i + 1;
					teamsInGivenRank = new List<Team>();
				}
				teamRank.Add(teams[i], rank);
				teamsInGivenRank.Add(teams[i]);
			}
			rankStringStandings.Add(rank, rank.ToString() + ". ");
			ranks.Add(rank);
			championsAndRelegations.Add(rank, teamsInGivenRank);
		}

		public void SortPlayersLeague(List<Player> players, bool goals, bool assists, bool yellowCards, bool redCards,
			bool saves, bool savePercentage, bool goalPercentage, bool goalsConceded, bool cleansheets, bool matchesPlayed,
			string league, string team)
		{
			players.Sort((x, y) =>
			{
				string xTeamName = team;
				string yTeam = team;

				if (team == "(all)")
				{
					xTeamName = x.teamName;
					yTeam = y.teamName;
				}

				int ret = 0;
				if (matchesPlayed)
					ret = y.LeagueTeamSeasonStats[league][yTeam].matchesPlayed.CompareTo(x.LeagueTeamSeasonStats[league][xTeamName].matchesPlayed);
				if (goals)
					ret = y.LeagueTeamSeasonStats[league][yTeam].goals.CompareTo(x.LeagueTeamSeasonStats[league][xTeamName].goals);
				if (ret == 0 && assists)
					ret = y.LeagueTeamSeasonStats[league][yTeam].assists.CompareTo(x.LeagueTeamSeasonStats[league][xTeamName].assists);
				if (ret == 0 && yellowCards)
					ret = y.LeagueTeamSeasonStats[league][yTeam].yellowCards.CompareTo(x.LeagueTeamSeasonStats[league][xTeamName].yellowCards);
				if (ret == 0 && redCards)
					ret = y.LeagueTeamSeasonStats[league][yTeam].redCards.CompareTo(x.LeagueTeamSeasonStats[league][xTeamName].redCards);
				if (ret == 0 && saves)
					ret = y.LeagueTeamSeasonStats[league][yTeam].Saves.CompareTo(x.LeagueTeamSeasonStats[league][xTeamName].Saves);
				if (ret == 0 && goalsConceded)
					ret = y.LeagueTeamSeasonStats[league][yTeam].goalsConceded.CompareTo(x.LeagueTeamSeasonStats[league][xTeamName].goalsConceded);
				if (ret == 0 && cleansheets)
					ret = y.LeagueTeamSeasonStats[league][yTeam].cleansheets.CompareTo(x.LeagueTeamSeasonStats[league][xTeamName].cleansheets);
				if (ret == 0 && savePercentage)
				{
					double ySavePct = 0.0;
					double xSavePct = 0.0;
					if (y.LeagueTeamSeasonStats[league][yTeam].Saves > 0)
						ySavePct = (double)y.LeagueTeamSeasonStats[league][yTeam].Saves / (double)(y.LeagueTeamSeasonStats[league][yTeam].goalsConceded + y.LeagueTeamSeasonStats[league][yTeam].Saves);
					if (x.LeagueTeamSeasonStats[league][xTeamName].Saves > 0)
						xSavePct = (double)x.LeagueTeamSeasonStats[league][xTeamName].Saves / (double)(x.LeagueTeamSeasonStats[league][xTeamName].goalsConceded + x.LeagueTeamSeasonStats[league][xTeamName].Saves);
					ret = ySavePct.CompareTo(xSavePct);
				}
				if (ret == 0 && goalPercentage)
				{
					double yGoalPct = 0.0;
					double xGoalPct = 0.0;
					if (y.LeagueTeamSeasonStats[league][yTeam].goals > 0 && y.LeagueTeamSeasonStats[league][yTeam].shotsTotal >= 10)
						yGoalPct = (double)y.LeagueTeamSeasonStats[league][yTeam].goals / (double)(y.LeagueTeamSeasonStats[league][yTeam].goals + y.LeagueTeamSeasonStats[league][yTeam].shotsTotal);
					if (x.LeagueTeamSeasonStats[league][xTeamName].goals > 0 && x.LeagueTeamSeasonStats[league][xTeamName].shotsTotal >= 10)
						xGoalPct = (double)x.LeagueTeamSeasonStats[league][xTeamName].goals / (double)(x.LeagueTeamSeasonStats[league][xTeamName].goals + x.LeagueTeamSeasonStats[league][xTeamName].shotsTotal);
					ret = yGoalPct.CompareTo(xGoalPct);
				}
				if (ret == 0)
					ret = x.fullName.CompareTo(y.fullName);
				return ret;
			});
		}

		public void SortTeamBySeasonStandingsDESC(List<Team> teams, string leagueName, bool seasonStandings = true, bool wins = false, bool losses = false, bool draws = false, bool goalsFor = true, bool goalsAgainst = false,
			bool goaldifferential = true)
		{
			teams.Sort((x, y) => {

				x.CreateSeasonStats(leagueName);
				y.CreateSeasonStats(leagueName);

				if (seasonStandings)
				{
					goaldifferential = true;
					goalsFor = true;
				}
				int xPoints = CalculateTeamPoints(x, leagueName);
				int yPoints = CalculateTeamPoints(y, leagueName);

				int xGoalDifferential = x.seasonStats[leagueName].goals - x.seasonStats[leagueName].goalsConceded;
				int yGoalDifferential = y.seasonStats[leagueName].goals - y.seasonStats[leagueName].goalsConceded;

				int ret = 0;
				if (seasonStandings)
					ret = yPoints.CompareTo(xPoints);
				if (ret == 0 && goaldifferential)
					ret = yGoalDifferential.CompareTo(xGoalDifferential);
				if (ret == 0 && wins)
					ret = y.seasonStats[leagueName].wins.CompareTo(x.seasonStats[leagueName].wins);
				if (ret == 0 && losses)
					ret = y.seasonStats[leagueName].losses.CompareTo(x.seasonStats[leagueName].losses);
				if (ret == 0 && draws)
					ret = y.seasonStats[leagueName].draws.CompareTo(x.seasonStats[leagueName].draws);
				if (ret == 0 && goalsFor)
					ret = y.seasonStats[leagueName].goals.CompareTo(x.seasonStats[leagueName].goals);
				if (ret == 0 && goalsAgainst)
					ret = x.seasonStats[leagueName].goalsConceded.CompareTo(y.seasonStats[leagueName].goalsConceded);
				if (ret == 0)
					ret = x.Name.CompareTo(y.Name);
				return ret;
			});
		}

		public void SortTeamBySeasonStandingsASC(List<Team> teams, string leagueName, bool seasonStandings = true, bool wins = false, bool losses = false, bool draws = false, bool goalsFor = false, bool goalsAgainst = true,
			bool goaldifferential = true)
		{
			teams.Sort((x, y) => {

				x.CreateSeasonStats(leagueName);
				y.CreateSeasonStats(leagueName);

				if (seasonStandings)
				{
					goaldifferential = true;
					goalsAgainst = true;
				}
				int xPoints = CalculateTeamPoints(x, leagueName);
				int yPoints = CalculateTeamPoints(y, leagueName);

				int xGoalDifferential = x.seasonStats[leagueName].goals - x.seasonStats[leagueName].goalsConceded;
				int yGoalDifferential = y.seasonStats[leagueName].goals - y.seasonStats[leagueName].goalsConceded;

				int ret = 0;
				if (seasonStandings)
					ret = xPoints.CompareTo(yPoints);
				if (ret == 0 && goaldifferential)
					ret = xGoalDifferential.CompareTo(yGoalDifferential);
				if (ret == 0 && wins)
					ret = x.seasonStats[leagueName].wins.CompareTo(y.seasonStats[leagueName].wins);
				if (ret == 0 && losses)
					ret = x.seasonStats[leagueName].losses.CompareTo(y.seasonStats[leagueName].losses);
				if (ret == 0 && draws)
					ret = x.seasonStats[leagueName].draws.CompareTo(y.seasonStats[leagueName].draws);
				if (ret == 0 && goalsFor)
					ret = x.seasonStats[leagueName].goals.CompareTo(y.seasonStats[leagueName].goals);
				if (ret == 0 && goalsAgainst)
					ret = y.seasonStats[leagueName].goals.CompareTo(x.seasonStats[leagueName].goals);
				if (ret == 0)
					ret = x.Name.CompareTo(y.Name);
				return ret;
			});
		}

		public int CalculateTeamPoints(Team team, string leagueName)
		{
			return (team.seasonStats[leagueName].wins * 3 + team.seasonStats[leagueName].draws * 1);
		}

		public int CalculateOffset(DayOfWeek current, DayOfWeek desired)
		{
			// f( c, d ) = [7 - (c - d)] mod 7
			// f( c, d ) = [7 - c + d] mod 7
			// c is current day of week and 0 <= c < 7
			// d is desired day of the week and 0 <= d < 7
			int c = (int)current;
			int d = (int)desired;
			int offset = (7 - c + d) % 7;
			return offset == 0 ? 7 : offset;
		}

		public void ScheduleEuropeKnockoutRounds(Dictionary<DateTime, List<Game>> CupFixtures, string leagueName)
		{
			DateTime seed = DateTime.Now;
			int year = seed.Year + 1;
			List<League> leagues = TeamRepository.Instance.countries[TeamRepository.Instance.countryIndex[leagueName]].leagues;
			List<Team> totalTeams = leagues[0].teams;

			SortTeamBySeasonStandingsDESC(totalTeams, leagueName);

			Dictionary<Team, int> teamRank = new Dictionary<Team, int>();

			this.teamRankInLeague.Add(leagueName, teamRank);
			
			for (int i = 0; i < totalTeams.Count; i++)
			{
				teamRank.Add(totalTeams[i], (i + 1));
			}

			List<Team> cupRoundOneTeams = new List<Team>();
			List<Team> cupRoundTwoTeams = new List<Team>();
			List<Team> unRankedTeams = new List<Team>();

			int round = cupRoundByCountry[leagueName];

			//TO DO: I think we should schedule all of the games. We can. We can schedule games on January,
			//figure out home and away scores based on away and home aggregates and setting 1 vs. winner of 9 and 24, etc.
			//The schedule will only show something like: Manchest U vs. Null. After the games are over, it'll be 
			//filled.

			DateTime date = new DateTime(year, 1, 1);
			date = date.AddDays(CalculateOffset(date.DayOfWeek, DayOfWeek.Friday));
			if (leagueName == "UEFA Champions League")
				date = date.AddDays(14);
			DateTime date2 = date.AddDays(7);

			for (int i = 0; i < 8; i++)
			{
				cupRoundTwoTeams.Add(totalTeams[i]);
			}
			for (int i = 8; i < 16; i++)
			{
				cupRoundOneTeams.Add(totalTeams[i]);
			}
			for (int i = 16; i < 24; i++)
			{
				unRankedTeams.Add(totalTeams[i]);
			}

			ShuffleTeams(unRankedTeams);
			for (int i = 0; i < unRankedTeams.Count; i++)
			{
				cupRoundOneTeams.Add(unRankedTeams[i]);
			}

			int totalGames = 8;
			int gamesScheduled = 0;
			int lastRank = 15;
			int firstRank = 0;

			List<Game> games = new List<Game>();
			List<Game> games2 = new List<Game>();

			while (gamesScheduled < totalGames)
			{
				Team teamA = null;
				Team teamB = null;
				if (round == 2)
				{
					teamA = cupRoundOneTeams[lastRank];
					teamB = cupRoundOneTeams[firstRank];
				}
				else
				{
					teamA = cupRoundOneTeams[firstRank];
					teamB = cupRoundOneTeams[lastRank];
				}

				Game game1 = new Game(teamA, teamB, 45, false, false, false, 3, date,
					leagueName, 1, leagueName + " Round 1 - Leg:",
					true, true, false, true);

				games.Add(game1);

				List<string> countries = new List<string>();

				if (!this.countriesCupMatchesScheduledOnDay.ContainsKey(date))
				{
					countries.Add(leagueName);
					this.countriesCupMatchesScheduledOnDay.Add(date, countries);
				}
				else
				{
					countries = this.countriesCupMatchesScheduledOnDay[date];
					if (!countries.Contains(leagueName)) countries.Add(leagueName);
					this.countriesCupMatchesScheduledOnDay[date] = countries;
				}

				Game game2 = new Game(teamB, teamA, 45, false, true, false, 3, date2,
					leagueName, 1, leagueName + " Round 2 - Leg:",
					true, true, false, true);

				games2.Add(game2);
				gamesScheduled++;
				lastRank--;
				firstRank++;

				this.gameLeggedPairs.Add(game2, game1);

				countries = new List<string>();

				if (!this.countriesCupMatchesScheduledOnDay.ContainsKey(date2))
				{
					countries.Add(leagueName);
					this.countriesCupMatchesScheduledOnDay.Add(date2, countries);
				}
				else
				{
					countries = this.countriesCupMatchesScheduledOnDay[date2];
					if (!countries.Contains(leagueName)) countries.Add(leagueName);
					this.countriesCupMatchesScheduledOnDay[date2] = countries;
				}
			}

			this.countryCupRoundOneTeams.Add(leagueName, cupRoundOneTeams);
			this.countryCupRoundTwoTeams.Add(leagueName, cupRoundTwoTeams);

			Dictionary<string, string> leagueNameDict = new Dictionary<string, string>();
			leagueNameDict.Add(leagueName, "Leg 1");

			countryLegName.Add(date, leagueNameDict);
			leagueNameDict = new Dictionary<string, string>();
			leagueNameDict.Add(leagueName, "Leg 2");
			countryLegName.Add(date2, leagueNameDict);

			CupFixtures.Add(date, games);
			CupFixtures.Add(date2, games2);
			List<List<Game>> allGames = new List<List<Game>>();
			allGames.Add(games);
			allGames.Add(games2);
			cupFixtures[leagueName] = CupFixtures;
			cupGames.Add(leagueName, allGames);
		}

		public void ScheduleCupRound1(Dictionary<DateTime, List<Game>> CupFixtures, string country)
		{
			DateTime seed = DateTime.Now;
			DateTime date = new DateTime(seed.Year, 10, 01);
			date = date.AddDays(CalculateOffset(date.DayOfWeek, DayOfWeek.Wednesday));
			List<League> leagues = TeamRepository.Instance.countries[TeamRepository.Instance.countryIndex[country]].leagues;
			Stack<Team> totalTeams = new Stack<Team>();
			foreach (League l in leagues)
			{
				List<Team> leagueTeams = l.teams;
				ShuffleTeams(leagueTeams);
				foreach (Team t in leagueTeams)
				{
					totalTeams.Push(t);
				}
			}
			int theTotal = totalTeams.Count;
			int totalInSecondRound = 64;
			if (theTotal < 64)
				totalInSecondRound = 32;
			if (theTotal < 32)
				totalInSecondRound = 16;
			if (theTotal < 16)
				totalInSecondRound = 8;
			if (theTotal < 8)
				totalInSecondRound = 4;
			if (theTotal < 4)
				totalInSecondRound = 2;

			List<Team> cupRoundOneTeams = new List<Team>();
			List<Team> cupRoundTwoTeams = new List<Team>();

			while (true)
			{
				if (theTotal - (cupRoundOneTeams.Count / 2) == totalInSecondRound)
					break;
				cupRoundOneTeams.Add(totalTeams.Pop());
			}

			if (cupRoundOneTeams.Count == 0)
			{
				while (totalTeams.Count > 0)
				{
					cupRoundOneTeams.Add(totalTeams.Pop());
				}
			}
			else
			{
				while (totalTeams.Count > 0)
				{
					cupRoundTwoTeams.Add(totalTeams.Pop());
				}
			}
			
			this.countryCupRoundOneTeams.Add(country, cupRoundOneTeams);
			this.countryCupRoundTwoTeams.Add(country, cupRoundTwoTeams);

			ScheduleCUPRoundOne(country, CupFixtures, cupRoundOneTeams, date);
		}

		public void ScheduleFACupRound1(Dictionary<DateTime, List<Game>> CupFixtures, string country)
		{
			DateTime seed = DateTime.Now;
			DateTime date = new DateTime(seed.Year, 10, 01);
			date = date.AddDays(CalculateOffset(date.DayOfWeek, DayOfWeek.Wednesday));
			List<League> leagues = TeamRepository.Instance.countries[TeamRepository.Instance.countryIndex[country]].leagues;
			List<Team> eflLeagueTwoTeams = leagues[3].teams;
			List<Team> eflLeagueOneTeams = leagues[2].teams;
			List<Team> eflChampionshipTeams = leagues[1].teams;
			List<Team> eplTeams = leagues[0].teams;
			List<Team> cupRoundOneTeams = new List<Team>();
			List<Team> cupRoundTwoTeams = new List<Team>();

			foreach (Team t in eflLeagueTwoTeams)
			{
				cupRoundOneTeams.Add(t);
			}
			foreach (Team t in eflLeagueOneTeams)
			{
				cupRoundOneTeams.Add(t);
			}

			ShuffleTeams(eflChampionshipTeams);
			for (int i = 0; i < eflChampionshipTeams.Count; i++)
			{
				if (i < 8)
					cupRoundOneTeams.Add(eflChampionshipTeams[i]);
				else
					cupRoundTwoTeams.Add(eflChampionshipTeams[i]);
			}
			foreach (Team t in eplTeams)
			{
				cupRoundTwoTeams.Add(t);
			}
			this.countryCupRoundOneTeams.Add(country, cupRoundOneTeams);
			this.countryCupRoundTwoTeams.Add(country, cupRoundTwoTeams);

			ScheduleCUPRoundOne(country, CupFixtures, cupRoundOneTeams, date);
		}

		public void ScheduleCupRound(Dictionary<DateTime, List<Game>> fixturesCup, string country,
			int month, bool shuffleTeams, bool scheduleDoubleLeg, bool scheduleEndOfMonth, DayOfWeek day)
		{
			DateTime seed = DateTime.Now;
			DateTime date = new DateTime(seed.Year, 10, 01);
			date = date.AddDays(CalculateOffset(date.DayOfWeek, day));
			if (scheduleEndOfMonth) date = date.AddDays(14);
			int cupRound = cupRoundByCountry[country];
			List<Game> games = cupGames[country][cupRound - 1];
			if (scheduleDoubleLeg)
			{
				games = cupGames[country][cupGames[country].Count - 1];
			}
			List<Team> teams = new List<Team>();
			foreach (Game g in games)
			{
				teams.Add(g.Winner);
			}
			if (teams.Count <= 1)
				return;

			cupRound++;
			cupRoundByCountry[country] = cupRound;
			List<List<Game>> allGames = cupGames[country];
			List<Team> cupRoundTwoTeams = this.countryCupRoundTwoTeams[country];
			int gamesPerWeek = 8; // 32 games
			int gameCount = 1;

			int year = seed.Year;
			if (month != 11 && month != 12) year++;
			date = new DateTime(year, month, 01);

			if (cupRound == 2)
			{
				if (!scheduleDoubleLeg)
				{
					foreach (Team t in cupRoundTwoTeams)
					{
						teams.Add(t);
					}
				}
				else
				{
					ShuffleTeams(teams);
					List<Team> winners = new List<Team>();
					foreach (Team t in teams)
					{
						winners.Add(t);
					}
					teams = new List<Team>();
					int[] ranks = { 0, 7, 3, 4, 2, 5, 1, 6 };
					for (int i = 0; i < cupRoundTwoTeams.Count; i++)
					{
						teams.Add(cupRoundTwoTeams[ranks[i]]);
						teams.Add(winners[i]);
					}
				}
			}

			if (teams.Count == 64)
			{
			}
			if (teams.Count == 32)
			{
				gamesPerWeek = 4; // 16 games
			}
			if (teams.Count == 16)
			{
				gamesPerWeek = 2; // 8 games
			}
			if (teams.Count == 8)
			{
				gamesPerWeek = 1; // 4 games
			}
			if (teams.Count == 4)
			{
				gamesPerWeek = 1; // 2 games;
			}
			if (teams.Count == 2)
			{
				gamesPerWeek = 1; // 1 game;
			}
			if (scheduleDoubleLeg)
				gamesPerWeek = (teams.Count / 2);
			date = date.AddDays(1);
			date = date.AddDays(CalculateOffset(date.DayOfWeek, day));
			if (scheduleDoubleLeg)
			{
				if (country == "UEFA Champions League")
					date = date.AddDays(14);
			}
			if (date < earlierGame)
				date = earlierGame;

			DateTime date2 = date.AddDays(7);

			games = new List<Game>();
			List<Game> games2 = new List<Game>();
			List<Game> firstLegGames = new List<Game>();
			List<Game> secondLegGames = new List<Game>();
			if (shuffleTeams) ShuffleTeams(teams); // remove this line if you want to use a straightforward bracket.

			for (int i = 0; i < teams.Count; i += 2)
			{
				if (gameCount > gamesPerWeek)
				{
					fixturesCup.Add(date, games);
					games = new List<Game>();
					gameCount = 1;
					date = date.AddDays(7);
					if (date > latestGame)
						date = latestGame;
				}
				
				Team teamA = teams[i];
				Team teamB = teams[i + 1];

				if (Season.Instance.teamRankInLeague.ContainsKey(country))
				{
					if (Season.Instance.teamRankInLeague[country].ContainsKey(teams[i]))
					{
						if (Season.Instance.teamRankInLeague[country][teams[i]] >
							Season.Instance.teamRankInLeague[country][teams[i + 1]])
						{
							teamB = teams[i];
							teamA = teams[i + 1];
						}
					}
				}

				string gameName = Season.Instance.countryCupNames[country] + " Round:";

				if (scheduleDoubleLeg)
				{
					if (teams.Count == 16)
					{
						gameName = "Round of 16 Leg:";
						cupRound = 1;
					}
					if (teams.Count == 8)
					{
						gameName = "Quarterfinal Leg:";
						cupRound = 1;
					}
					if (teams.Count == 4)
					{
						gameName = "Semifinal Leg:";
						cupRound = 1;
					}
					if (teams.Count == 2)
					{
						gameName = "Final:";
						cupRound = -1;
						scheduleDoubleLeg = false;
						date2.AddDays(7);
					}
				}

				Game game = new Game(teamA, teamB, 45, false, 
					(country != "UEFA Champions League" && country != "UEFA Europa League") || cupRound == -1, false, 3,
					date, Season.Instance.countryCupNames[country], cupRound, country, true, true, false,
					country == "UEFA Champions League" || country == "UEFA Europa League");
				
				games.Add(game);
				firstLegGames.Add(game);

				if (scheduleDoubleLeg)
				{
					cupRound = 2;

					Game game2 = new Game(teamB, teamA, 45, false, true, false, 3,
						date2, Season.Instance.countryCupNames[country], cupRound, gameName, true, true, false,
						country == "UEFA Champions League" || country == "UEFA Europa League");

					games2.Add(game2);
					secondLegGames.Add(game2);

					this.gameLeggedPairs.Add(game2, game);
				}

				gameCount++;

				List<string> countries = new List<string>();

				if (!this.countriesCupMatchesScheduledOnDay.ContainsKey(date))
				{
					countries.Add(country);
					this.countriesCupMatchesScheduledOnDay.Add(date, countries);
				}
				else
				{
					countries = this.countriesCupMatchesScheduledOnDay[date];
					if (!countries.Contains(country)) countries.Add(country);
					this.countriesCupMatchesScheduledOnDay[date] = countries;
				}

				if (scheduleDoubleLeg)
				{
					if (!this.countriesCupMatchesScheduledOnDay.ContainsKey(date2))
					{
						countries.Add(country);
						this.countriesCupMatchesScheduledOnDay.Add(date2, countries);
					}
					else
					{
						countries = this.countriesCupMatchesScheduledOnDay[date2];
						if (!countries.Contains(country)) countries.Add(country);
						this.countriesCupMatchesScheduledOnDay[date2] = countries;
					}
				}
			}

			if (date > latestGame)
				date = latestGame;

			if (scheduleDoubleLeg)
			{
				Dictionary<string, string> leagueNameDict = new Dictionary<string, string>();
				leagueNameDict.Add(country, "Leg 1");

				countryLegName.Add(date, leagueNameDict);
				leagueNameDict = new Dictionary<string, string>();
				leagueNameDict.Add(country, "Leg 2");
				countryLegName.Add(date2, leagueNameDict);
			}

			fixturesCup.Add(date, games);
			if (games2.Count > 0) fixturesCup.Add(date2, games2);
			allGames.Add(firstLegGames);
			if (secondLegGames.Count > 0) allGames.Add(secondLegGames);
			cupFixtures[country] = fixturesCup;
			cupGames[country] = allGames;
		}

		private void ScheduleCUPRoundOne(string country, Dictionary<DateTime, List<Game>> CupFixtures,
			List<Team> cupRoundOneTeams, DateTime date)
		{
			ShuffleTeams(cupRoundOneTeams);
			int gameCount = 1;
			List<Game> games = null;
			List<Game> totalGames = new List<Game>();
			if (!CupFixtures.ContainsKey(date))
				games = new List<Game>();
			else
				games = CupFixtures[date];
			if (date < earlierGame)
				date = earlierGame;
			for (int i = 0; i < cupRoundOneTeams.Count; i += 2)
			{
				if (gameCount > 7)
				{
					CupFixtures.Add(date, games);
					games = new List<Game>();
					gameCount = 1;
					date = date.AddDays(7);
				}
				Game game = new Game(cupRoundOneTeams[i], cupRoundOneTeams[i + 1], 45, false, true, false, 3,
					date, Season.Instance.countryCupNames[country], 1, Season.Instance.countryCupNames[country], true, true);
				games.Add(game);
				totalGames.Add(game);
				gameCount++;

				List<string> countries = new List<string>();

				if (!this.countriesCupMatchesScheduledOnDay.ContainsKey(date))
				{
					countries.Add(country);
					this.countriesCupMatchesScheduledOnDay.Add(date, countries);
				}
				else
				{
					countries = this.countriesCupMatchesScheduledOnDay[date];
					if (!countries.Contains(country)) countries.Add(country);
					this.countriesCupMatchesScheduledOnDay[date] = countries;
				}
			}

			if (date > latestGame)
				date = latestGame;

			CupFixtures.Add(date, games);
			List<List<Game>> allGames = new List<List<Game>>();
			allGames.Add(totalGames);
			cupFixtures.Add(country, CupFixtures);
			cupGames.Add(country, allGames);
		}

		public void ScheduleDoubleRoundRobin(List<Team> ListTeam, bool premierLeague, Dictionary<DateTime, List<Game>> allLeagueFixtures,
			Dictionary<DateTime, List<Game>> leagueFixtures, string country, string leagueName)
		{
			DateTime date = DateTime.Now;
			if (premierLeague)
				date = GetDatePremierLeagueDate(date);
			else
				date = GetNonPremierLeagueDate(date);

			int month = 8;
			int weekMonthCount = 0;
			bool scheduleTuesday = true;
			bool tuesdayScheduledLast = false;

			ScheduleSingleRoundRobin(ListTeam, premierLeague, allLeagueFixtures, leagueFixtures, country, false, ref date,
				ref scheduleTuesday, ref tuesdayScheduledLast, ref month, ref weekMonthCount, leagueName);

			ScheduleSingleRoundRobin(ListTeam, premierLeague, allLeagueFixtures, leagueFixtures, country, true, ref date,
				ref scheduleTuesday, ref tuesdayScheduledLast, ref month, ref weekMonthCount, leagueName);

			foreach (Team t in ListTeam)
			{
				if (t.Name == "Bye Week")
				{
					ListTeam.Remove(t);
					break;
				}
			}
		}

		public void ScheduleSingleRoundRobin(List<Team> ListTeam, bool premierLeague, Dictionary<DateTime, List<Game>> allLeagueFixtures,
			Dictionary<DateTime, List<Game>> leagueFixtures, string country, bool scheduleSecondTeamFirst,
			ref DateTime date, ref bool scheduleTuesday, ref bool tuesdayScheduledLast, ref int month, ref int weekMonthCount,
			string leagueName)
		{
			if (ListTeam.Count % 2 != 0)
			{
				ListTeam.Add(new Team("Bye Week", "Bye Week"));
			}

			if (date < earlierGame)
			{
				earlierGame = date;
			}

			int numberOfMatchesToSchedule = (ListTeam.Count - 1);
			int halfSize = ListTeam.Count / 2;
			int fixtureNumber = 1;
			if (scheduleSecondTeamFirst) fixtureNumber = 20;

			List<Team> teams = new List<Team>();

			teams.AddRange(ListTeam.Skip(halfSize).Take(halfSize));
			teams.AddRange(ListTeam.Skip(1).Take(halfSize - 1).ToArray().Reverse());

			int teamsSize = teams.Count;

			for (int week = 0; week < numberOfMatchesToSchedule; week++)
			{
				if (month == date.Month)
					weekMonthCount++;
				else
				{
					month = date.Month;
					weekMonthCount = 1;
				}

				if (internationalBreakWeeks.ContainsKey(month))
					if (internationalBreakWeeks[month] == weekMonthCount)
						if (month == 12)
							date = date.AddDays(21);
						else
							date = date.AddDays(7);

				List<Game> games = null;
				List<Game> leagueGames = new List<Game>();
				if (!allLeagueFixtures.ContainsKey(date))
					games = new List<Game>();
				else
					games = allLeagueFixtures[date];

				if (date > latestGame)
					latestGame = date;

				int teamIdx = week % teamsSize;

				Game game = null;
				if (scheduleSecondTeamFirst)
					game = new Game(ListTeam[0], teams[teamIdx], 45, false, false, false, 3, date, leagueName,
						fixtureNumber, leagueDictionary + " Fixture:");
				else
					game = new Game(teams[teamIdx], ListTeam[0], 45, false, false, false, 3, date, leagueName,
						fixtureNumber, leagueDictionary + " Fixture:");
				if (game.homeTeam.Name != "Bye Week" && game.awayTeam.Name != "Bye Week")
				{
					games.Add(game);
					leagueGames.Add(game);
				}

				for (int idx = 1; idx < halfSize; idx++)
				{
					int firstTeam = (week + idx) % teamsSize;
					int secondTeam = (week + teamsSize - idx) % teamsSize;
					if (scheduleSecondTeamFirst)
						game = new Game(teams[secondTeam], teams[firstTeam], 45, false, false, false, 3, date, leagueName,
						fixtureNumber, leagueDictionary + " Fixture:");
					else
						game = new Game(teams[firstTeam], teams[secondTeam], 45, false, false, false, 3, date, leagueName,
						fixtureNumber, leagueDictionary + " Fixture:");
					if (game.homeTeam.Name != "Bye Week" && game.awayTeam.Name != "Bye Week")
					{
						games.Add(game);
						leagueGames.Add(game);
					}
				}

				if (!allLeagueFixtures.ContainsKey(date))
					allLeagueFixtures.Add(date, games);
				else
				{
					allLeagueFixtures[date] = games;
				}
				leagueFixtures.Add(date, leagueGames);

				List<string> countries = new List<string>();

				if (!this.countriesLeagueMatchesScheduledOnDay.ContainsKey(date))
				{
					countries.Add(country);
					this.countriesLeagueMatchesScheduledOnDay.Add(date, countries);
				}
				else
				{
					countries = this.countriesLeagueMatchesScheduledOnDay[date];
					if (!countries.Contains(country)) countries.Add(country);
					this.countriesLeagueMatchesScheduledOnDay[date] = countries;
				}

				if (date > latestGame)
					latestGame = date;

				if ((date.AddDays(3).Month == 9 && !premierLeague) || date.AddDays(3).Month == 2)
					if (scheduleTuesday)
					{
						scheduleTuesday = false;
						tuesdayScheduledLast = true;
						date = date.AddDays(3);
					}
					else
					{
						scheduleTuesday = true;
						tuesdayScheduledLast = false;
						date = date.AddDays(4);
					}
				else
				{
					scheduleTuesday = true;
					if (tuesdayScheduledLast)
					{
						tuesdayScheduledLast = false;
						date = date.AddDays(4);
					}
					else
					{
						if ((date.AddDays(3).Month == 9 && !premierLeague) || date.AddDays(3).Month == 2)
						{
							scheduleTuesday = false;
							tuesdayScheduledLast = true;
							date = date.AddDays(3);
						}
						else
						{
							date = date.AddDays(7);
						}
					}
				}
				fixtureNumber++;
			}
		}

		private DateTime GetNonPremierLeagueDate(DateTime seed)
		{
			DateTime dateTime = new DateTime(seed.Year, 08, 01);
			DateTime saturday = dateTime.AddDays(CalculateOffset(dateTime.DayOfWeek, DayOfWeek.Saturday));
			return saturday;
		}

		private DateTime GETEUefaDate(DateTime seed)
		{
			DateTime dateTime = new DateTime(seed.Year, 09, 01);
			DateTime friday = dateTime.AddDays(CalculateOffset(dateTime.DayOfWeek, DayOfWeek.Friday));
			return friday;
		}

		private DateTime GetDatePremierLeagueDate(DateTime seed)
		{
			DateTime dateTime = new DateTime(seed.Year, 08, 01);
			DateTime sunday = dateTime.AddDays(CalculateOffset(dateTime.DayOfWeek, DayOfWeek.Sunday));
			sunday = sunday.AddDays(1);
			sunday = sunday.AddDays(CalculateOffset(sunday.DayOfWeek, DayOfWeek.Sunday));
			sunday = sunday.AddDays(1);
			// guarantees third sunday
			sunday = sunday.AddDays(CalculateOffset(sunday.DayOfWeek, DayOfWeek.Sunday));
			return sunday;
		}

		private void ShuffleTeams(List<Team> teams)
		{
			Random r = new Random();
			//Step 1: For each unshuffled item in the collection
			for (int n = teams.Count - 1; n > 0; --n)
			{
				//Step 2: Randomly pick an item which has not been shuffled
				int k = r.Next(n + 1);

				//Step 3: Swap the selected item with the last "unstruck" letter in the collection
				Team temp = teams[n];
				teams[n] = teams[k];
				teams[k] = temp;
			}
		}

		public String DetermineRoundString(string leagueName, DateTime date)
		{
			String roundString = Season.Instance.countryCupNames[leagueName];
			List<List<Game>> cupGames = Season.Instance.cupGames[leagueName];
			List<Game> games = cupGames[cupGames.Count - 1];
			if (cupGames.Count < 2)
			{
				roundString = roundString + ": Opening Round";
			}
			if (games.Count == 64)
			{
				roundString += ": Round of 128";
			}
			if (games.Count == 32)
			{
				roundString += ": Round of 64";
			}
			if (games.Count == 16)
			{
				roundString += ": Round of 32";
			}
			if (games.Count == 8)
			{
				roundString += ": Round of 16";
			}
			if (games.Count == 4)
			{
				roundString += ": Quarterfinal";
			}
			if (games.Count == 2)
			{
				roundString += ": Semi-final";
			}
			if (games.Count == 1)
			{
				roundString += ": Final";
			}
			if (leagueName == "UEFA Champions League" || leagueName == "UEFA Europa League")
			{
				if (cupGames.Count == 2)
					roundString = this.countryCupNames[leagueName] + ": Knockout Phase ";
				if (this.countryLegName.ContainsKey(date))
				{
					roundString += this.countryLegName[date][leagueName];
				}
			}

			return roundString;
		}
}
