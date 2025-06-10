using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TeamRepository
{
	private static TeamRepository m_instance = null;
		List<string> teamNames = new List<string>();
		public List<Team> teams = new List<Team>();
		public List<League> leagues = new List<League>();
		public List<LeagueCountry> countries = new List<LeagueCountry>();
		public Dictionary<string, League> leagueNameDict = new Dictionary<string, League>();
		public Dictionary<string, Team> teamNameDict = new Dictionary<string, Team>();
		public Dictionary<string, LeagueCountry> leagueCountryDict = new Dictionary<string, LeagueCountry>();
		public Dictionary<string, int> leagueTier = new Dictionary<string, int>();
		public Dictionary<string, string> formations = new Dictionary<string, string>();
		public Dictionary<string, int> countryIndex = new Dictionary<string, int>();
		public Dictionary<int, Player> playerIDDict = new Dictionary<int, Player>();
		public List<Team> UEFAChampionsLeagueNextSeason = new List<Team>();
		public List<Team> UEFAEuropaLeagueNextSeason = new List<Team>();

		public string[] EuropeCountries = { "England", "Italy", "Spain", "France", "Germany", "Netherlands", "Portugal",
			"Sweden", "Belgium", "Turkey", "Norway", "Scotland", "Poland", "Denmark", "Switzerland", "Ireland", "Austria",
			"Romania" };

		public string[] UEFAChampionsLeagueTeams = { "Liverpool", "FC Barcelona", "Arsenal", "Inter Milan", "Atlético de Madrid", "Leverkusen", "LOSC Lille", "Aston Villa"
			, "Atalanta", "Borussia Dortmund", "Real Madrid", "FC Bayern München", "AC Milan", "PSV", "Paris SG"
			, "SL Benfica", "AS Monaco", "Stade Brestois 29", "Feyenoord", "Juventus", "Celtic", "Manchester City", "Sporting CP", "Club Brugge"
			, "Malmö FF", "VfB Stuttgart", "FK Bodø/Glimt", "Bologna", "FC Midtjylland", "Galatasaray", "PAOK FC", "RB Leipzig"
			, "Girona FC", "FC Porto", "Lech Poznań", "BSC Young Boys" };

		public string[] UEFAEuropaLeagueTeams = { "Manchester United", "SS Lazio", "Athletic Club", "Spurs", "Frankfurt",
												   "OL", "Olympiacos FC", "Rangers", "Hearts", "Toulouse FC", "Molde FK",
												   "Ajax", "Real Sociedad", "Servette FC", "AS Roma", "Wolves", "FC Lugano",
												   "FC Porto", "AZ", "Everton", "R. Union St.-G.", "Fiorentina", "FC Twente",
												   "Fenerbahçe", "SC Braga", "IF Elfsborg", "TSG Hoffenheim", "Beşiktaş",
												   "Jagiellonia", "Sparta Rotterdam", "SSC Napoli", "Rayo Vallecano", "Getafe CF",
												   "OM", "OGC Nice", "Shamrock Rovers" };

		//List<Division> divisions;
		public TeamRepository()
		{
			//divisions = new List<Division>();
			SaveData();
			LoadTeams();
		}

		public static TeamRepository Instance
		{
			get
			{
				if (m_instance == null)
				{
					m_instance = new TeamRepository();
				}
				return m_instance;
			}
		}

		public Team getTeam(int i)
		{
			if (i < teams.Count)
			{
				return (Team)teams[i];
			}
			else
			{
				return (Team)teams[0];
			}
		}

		private void SaveData()
		{
			PopulateLeagueCountry();
			PopulateLeagueTier();
			PopulateFormations();

			using (StreamReader readtext = new StreamReader(@"Data\\male_players.csv"))
			{
				string readText = readtext.ReadLine();
				using (StreamWriter writer = new StreamWriter(@"Data\\default_players.dat"))
				{
					int rowNumber = 0;
					while (readText != null)
					{
						string[] stringofdata = readText.Split(',');
						writer.Write(stringofdata[0] + ",");
						writer.Write(stringofdata[1] + ","); // at some point, think of a way to create random names
						writer.Write(stringofdata[2] + ",");
						writer.Write(stringofdata[3] + ",");
						writer.Write(stringofdata[4] + ",");
						writer.Write(stringofdata[5] + ",");
						writer.Write(stringofdata[6] + ",");
						writer.Write(stringofdata[7] + ",");
						writer.Write(stringofdata[8] + ",");
						writer.Write(stringofdata[9] + ",");
						writer.Write(stringofdata[10] + ",");
						writer.Write(stringofdata[11] + ",");
						writer.Write(stringofdata[12] + ",");
						writer.Write(stringofdata[13] + ",");
						writer.Write(stringofdata[14] + ",");
						writer.Write(stringofdata[15] + ",");
						writer.Write(stringofdata[16] + ",");
						writer.Write(stringofdata[17] + ",");

						writer.Write(stringofdata[18] + ",");
						writer.Write(stringofdata[19] + ",");
						writer.Write(stringofdata[20] + ",");
						writer.Write(stringofdata[21] + ",");
						writer.Write(stringofdata[22] + ",");
						writer.Write(stringofdata[23] + ",");
						writer.Write(stringofdata[24] + ",");
						writer.Write(stringofdata[25] + ",");
						writer.Write(stringofdata[26] + ",");
						writer.Write(stringofdata[27] + ",");
						writer.Write(stringofdata[28] + ",");
						writer.Write(stringofdata[29] + ",");
						writer.Write(stringofdata[30] + ",");
						writer.Write(stringofdata[31] + ",");
						writer.Write(stringofdata[32] + ",");
						writer.Write(stringofdata[33] + ",");
						writer.Write(stringofdata[34] + ",");

						writer.Write(stringofdata[35] + ",");
						writer.Write(stringofdata[36] + ",");
						writer.Write(stringofdata[37] + ",");
						writer.Write(stringofdata[38] + ",");
						writer.Write(stringofdata[39] + ",");
						writer.Write(stringofdata[40] + ",");
						writer.Write(stringofdata[41] + ",");
						writer.Write(stringofdata[42] + ",");
						writer.Write(stringofdata[43] + ",");
						writer.Write(stringofdata[44] + ",");
						writer.Write(stringofdata[45] + ",");
						writer.Write(stringofdata[46] + ",");
						if (rowNumber == 0)
						{
							writer.Write("League Country,");
							writer.Write("League Tier,");
						}
						else
						{
							writer.Write(leagueCountryDict[stringofdata[46]].CountryName + ",");
							writer.Write(leagueTier[stringofdata[46]] + ",");
						}
						writer.Write(stringofdata[47] + ",");
						writer.Write(stringofdata[48] + ",");
						writer.Write(stringofdata[49] + ",");
						writer.Write(stringofdata[50] + ",");
						writer.Write(stringofdata[51] + ",");
						writer.Write(stringofdata[52] + ",");
						writer.Write(stringofdata[53] + ",");

						if (rowNumber == 0)
						{
							writer.Write("Transfer Fee,");
							writer.WriteLine("Yearly Salary");
						}
						else
						{
							Player player = new Player(-1, "Temporary");
							player.overall = Int32.Parse(stringofdata[2]);
							writer.Write(((double)CalculateSalary(player, 190) / (double)100).ToString() + ",");
							writer.WriteLine(((double)CalculateSalary(player, 35) / (double)100).ToString());
						}
						
						rowNumber++;
						readText = readtext.ReadLine();
					}
				}
			}
		}

		public void LoadTeams()
		{
			using (StreamReader readtext = new StreamReader(@"Data\\default_players.dat"))
			{
				//Dictionary<Player, int> defenders = new Dictionary<Player, int>();
				//Dictionary<Player, int> forwards = new Dictionary<Player, int>();
				//Dictionary<Player, int> midfielders = new Dictionary<Player, int>();
				//Dictionary<Player, int> goalkeepers = new Dictionary<Player, int>();
				//Dictionary<Player, int> benchAndReserves = new Dictionary<Player, int>();
				string readText = readtext.ReadLine();
				List<Player> roster = new List<Player>();
				int rowNumber = 0;
				League UEFAChampionsLeague = new League("UEFA Champions League");
				LeagueCountry UEFALC = new LeagueCountry("UEFA Champions League");
				countries.Add(UEFALC);

				League UEFAEuropaLeague = new League("UEFA Europa League");
				LeagueCountry UEFAEL = new LeagueCountry("UEFA Europa League");
				countries.Add(UEFAEL);

				//leagueCountryDict.Add("UEFA Champions League", lc);

				while (readText != null)
				{
					if (rowNumber > 0)
					{
						string[] stringofdata = readText.Split(',');
						Player p = new Player(rowNumber, stringofdata[1]);
						Int32.TryParse(stringofdata[2], out p.overall);
						Int32.TryParse(stringofdata[3], out p.pace);
						Int32.TryParse(stringofdata[4], out p.shooting);
						Int32.TryParse(stringofdata[5], out p.passing);
						Int32.TryParse(stringofdata[6], out p.defending);
						Int32.TryParse(stringofdata[7], out p.physicality);
						Int32.TryParse(stringofdata[8], out p.acceleration);
						Int32.TryParse(stringofdata[9], out p.sprint);
						Int32.TryParse(stringofdata[10], out p.positioning);
						Int32.TryParse(stringofdata[11], out p.finishing);
						Int32.TryParse(stringofdata[12], out p.shotPower);
						Int32.TryParse(stringofdata[13], out p.longShot);
						Int32.TryParse(stringofdata[14], out p.volleys);
						Int32.TryParse(stringofdata[15], out p.penalties);
						Int32.TryParse(stringofdata[16], out p.vision);
						Int32.TryParse(stringofdata[17], out p.crossing);

						Int32.TryParse(stringofdata[18], out p.freekicks);
						Int32.TryParse(stringofdata[19], out p.shortPass);
						Int32.TryParse(stringofdata[20], out p.longPass);
						Int32.TryParse(stringofdata[21], out p.curve);
						Int32.TryParse(stringofdata[22], out p.dribbling);
						Int32.TryParse(stringofdata[23], out p.agility);
						Int32.TryParse(stringofdata[24], out p.balance);
						Int32.TryParse(stringofdata[25], out p.reactionTime);
						Int32.TryParse(stringofdata[26], out p.ballControl);
						Int32.TryParse(stringofdata[27], out p.composure);
						Int32.TryParse(stringofdata[28], out p.intercept);
						Int32.TryParse(stringofdata[29], out p.header);
						Int32.TryParse(stringofdata[30], out p.defenseAwareness);
						Int32.TryParse(stringofdata[31], out p.standTackle);
						Int32.TryParse(stringofdata[32], out p.slideTackle);
						Int32.TryParse(stringofdata[33], out p.jumping);
						Int32.TryParse(stringofdata[34], out p.stamina);

						Int32.TryParse(stringofdata[35], out p.strength);
						Int32.TryParse(stringofdata[36], out p.aggression);
						if (stringofdata[37] == "CAM")
							p.Position = Enums.Positions.CentralAttackingMidfielder;
						if (stringofdata[37] == "CB")
							p.Position = Enums.Positions.CenterBack;
						if (stringofdata[37] == "CDM")
							p.Position = Enums.Positions.CentralDefendingMidfielder;
						if (stringofdata[37] == "CM")
							p.Position = Enums.Positions.CentralMidfielder;
						if (stringofdata[37] == "GK")
							p.Position = Enums.Positions.Goalkeeper;
						if (stringofdata[37] == "LB")
							p.Position = Enums.Positions.LeftBack;
						if (stringofdata[37] == "LM")
							p.Position = Enums.Positions.LeftMidfielder;
						if (stringofdata[37] == "LW")
							p.Position = Enums.Positions.LeftWingForward;
						if (stringofdata[37] == "RB")
							p.Position = Enums.Positions.RightBack;
						if (stringofdata[37] == "RM")
							p.Position = Enums.Positions.RightMidfielder;
						if (stringofdata[37] == "RW")
							p.Position = Enums.Positions.RightWingForward;
						if (stringofdata[37] == "ST")
							p.Position = Enums.Positions.Striker;
						Int32.TryParse(stringofdata[38], out p.weakFoot);
						Int32.TryParse(stringofdata[39], out p.skillMoves);
						p.preferredFoot = stringofdata[40];
						p.height = stringofdata[41];
						p.weight = stringofdata[42];
						p.secondPos = stringofdata[43];
						Int32.TryParse(stringofdata[44], out p.age);
						p.nation = stringofdata[45];
						League league;
						if (leagueNameDict.ContainsKey(stringofdata[46]))
						{
							league = leagueNameDict[stringofdata[46]];
						}
						else
						{
							league = new League(stringofdata[46]);
							leagueNameDict.Add(stringofdata[46], league);
							leagues.Add(league);
						}
						LeagueCountry country;
						if (stringofdata[47] == "Bucharest")
							stringofdata[47] = "Romania";
						if (leagueCountryDict.ContainsKey(stringofdata[47]))
						{
							country = leagueCountryDict[stringofdata[47]];
						}
						else
						{
							country = new LeagueCountry(stringofdata[47]);
							leagueCountryDict.Add(stringofdata[47], country);
							countries.Add(country);
						}
						if (!country.leagues.Contains(league))
						{
							country.leagues.Add(league);
						}
						Int32.TryParse(stringofdata[48], out league.tier);
						Team theTeam;
						if (teamNameDict.ContainsKey(stringofdata[49]))
						{
							theTeam = teamNameDict[stringofdata[49]];
						}
						else
						{
							theTeam = new Team(stringofdata[49], league.LeagueName);
							teamNameDict.Add(stringofdata[49], theTeam);
						}
						if (!league.teams.Contains(theTeam))
							league.teams.Add(theTeam);
						p.playStyle = stringofdata[50];
						Int32.TryParse(stringofdata[51], out p.goalkeepingDiving);
						Int32.TryParse(stringofdata[52], out p.goalKeepingHandling);
						Int32.TryParse(stringofdata[53], out p.goalKeepingKicking);
						Int32.TryParse(stringofdata[54], out p.goalKeepingPositioning);
						Int32.TryParse(stringofdata[55], out p.goalKeepingReflexes);
						Double.TryParse(stringofdata[56], out p.transferFee);
						Double.TryParse(stringofdata[57], out p.salary);

						theTeam.completeRoster.Add(p);
						playerIDDict.Add(p.ID, p);
						p.teamName = theTeam.Name;
						if (this.UEFAChampionsLeagueTeams.Contains(theTeam.Name))
						{
							if (!UEFAChampionsLeague.teams.Contains(theTeam))
							{
								UEFAChampionsLeague.teams.Add(theTeam);
							}
						}

						if (this.UEFAEuropaLeagueTeams.Contains(theTeam.Name))
						{
							if (!UEFAEuropaLeague.teams.Contains(theTeam))
							{
								UEFAEuropaLeague.teams.Add(theTeam);
							}
						}

						if (formations.ContainsKey(theTeam.Name))
							theTeam.formation = formations[theTeam.Name];
						else
							theTeam.formation = "4231";
					}
					readText = readtext.ReadLine();
					rowNumber++;
				}
				UEFALC.leagues.Add(UEFAChampionsLeague);
				UEFAEL.leagues.Add(UEFAEuropaLeague);
				SortCountries(countries);
				for (int i = 0; i < countries.Count; i++)
				{
					countryIndex.Add(countries[i].CountryName, i);
					SortLeagues(countries[i].leagues);
					foreach (League l in countries[i].leagues)
					{
						SortTeams(l.teams);
						foreach (Team t in l.teams)
						{
							if (t.completeRoster.Count < 20)
							{
								Player p = new Player(++rowNumber, "Some Goalkeeper");
								p.Position = Enums.Positions.Goalkeeper;
								p.teamName = t.Name;
								t.completeRoster.Add(p);
								playerIDDict.Add(p.ID, p);

								while (t.completeRoster.Count < 20)
								{
									FillWithGenericPlayers(t, ref rowNumber);
								}
							}
							t.ConfigureRoster();
						}
					}
				}
			}
		}

		private void FillWithGenericPlayers(Team t, ref int id)
		{
			Player p = new Player(++id, "Some Defender");
			p.Position = Enums.Positions.CenterBack;
			p.teamName = t.Name;
			t.completeRoster.Add(p);
			playerIDDict.Add(p.ID, p);

			p = new Player(++id, "Some Midfielder");
			p.Position = Enums.Positions.CentralMidfielder;
			p.teamName = t.Name;
			t.completeRoster.Add(p);
			playerIDDict.Add(p.ID, p);

			p = new Player(++id, "Some Forward");
			p.Position = Enums.Positions.Striker;
			p.teamName = t.Name;
			t.completeRoster.Add(p);
			playerIDDict.Add(p.ID, p);
		}

		public void DetermineEuropeanTeams()
		{
			Team uefaChampion = null;
			if (Season.Instance.cupGames.ContainsKey("UEFA Champions League"))
			{
				List<List<Game>> UEFAGames = Season.Instance.cupGames["UEFA Champions League"];
				if (UEFAGames[UEFAGames.Count - 1].Count == 1)
				{
					if (UEFAGames[UEFAGames.Count - 1][0].GamePlayed)
					{
						uefaChampion = UEFAGames[UEFAGames.Count - 1][0].Winner;
					}
				}
			}

			Team europaChampion = null;
			if (Season.Instance.cupGames.ContainsKey("UEFA Europa League"))
			{
				List<List<Game>> EuropaGames = Season.Instance.cupGames["UEFA Europa League"];
				if (EuropaGames[EuropaGames.Count - 1].Count == 1)
				{
					if (EuropaGames[EuropaGames.Count - 1][0].GamePlayed)
					{
						europaChampion = EuropaGames[EuropaGames.Count - 1][0].Winner;
					}
				}
			}

			if (uefaChampion == null || europaChampion == null)
				return;

			int[] UEFAChampionsLeagueTeams = { 4, 4, 4, 3, 3, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
			int[] UEFAEuropaLeagueTeams = { 4, 4, 4, 4, 3, 3, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
			List<Queue<Team>> premierQueues = new List<Queue<Team>>();
			for (int i = 0; i < TeamRepository.Instance.EuropeCountries.Length; i++)
			{
				if (TeamRepository.Instance.EuropeCountries[i] == "UEFA Champions League" ||
					TeamRepository.Instance.EuropeCountries[i] == "UEFA Europa League")
					break;
				string country = TeamRepository.Instance.EuropeCountries[i];
				List<List<Game>> cupGames = Season.Instance.cupGames[country];
				Team cupChampion = null;
				if (cupGames[cupGames.Count - 1].Count == 1)
				{
					if (cupGames[cupGames.Count - 1][0].GamePlayed)
					{
						cupChampion = cupGames[cupGames.Count - 1][0].Winner;
					}
				}

				List<League> l = TeamRepository.Instance.countries[TeamRepository.Instance.countryIndex[country]].leagues;
				for (int j = 0; j < l.Count; j++)
				{
					Season.Instance.SortTeamBySeasonStandingsDESC(l[j].teams, l[j].LeagueName);
					Queue<Team> queue = new Queue<Team>();
					foreach (Team t in l[j].teams)
					{
						if (uefaChampion == t)
						{
							UEFAChampionsLeagueNextSeason.Add(t);
						}
						else if (europaChampion == t)
						{
							UEFAChampionsLeagueNextSeason.Add(t);
						}
						else
						{
							if (j == 0)
							{
								queue.Enqueue(t);
							}
						}
					}
					if (j == 0)
					{
						int uefaChampionsCount = 0;
						while (uefaChampionsCount < UEFAChampionsLeagueTeams[i])
						{
							Team t = queue.Dequeue();
							if (!UEFAChampionsLeagueNextSeason.Contains(t))
							{
								UEFAChampionsLeagueNextSeason.Add(t);
								uefaChampionsCount++;
							}
							if (t == cupChampion)
							{
								if (!UEFAChampionsLeagueNextSeason.Contains(t))
								{
									UEFAEuropaLeagueNextSeason.Add(cupChampion);
									UEFAEuropaLeagueTeams[i]--;
								}
							}
						}
						premierQueues.Add(queue);
					}
				}
				for (int j = 0; j < l.Count; j++)
				{
					Season.Instance.SortTeamBySeasonStandingsDESC(l[j].teams, l[j].LeagueName);
					Queue<Team> queue = new Queue<Team>();
					foreach (Team t in l[j].teams)
					{
						if (t == cupChampion)
						{
							if (!UEFAChampionsLeagueNextSeason.Contains(t))
							{
								UEFAEuropaLeagueNextSeason.Add(cupChampion);
								UEFAEuropaLeagueTeams[i]--;
							}
						}
					}
				}
			}
			int moreTeamsToAdd = 6;
			while (UEFAChampionsLeagueNextSeason.Count != 36)
			{
				UEFAChampionsLeagueNextSeason.Add(premierQueues[moreTeamsToAdd].Dequeue());
				if (moreTeamsToAdd == 6)
					moreTeamsToAdd = 0;
				else
					moreTeamsToAdd++;
			}
			//Console.WriteLine("UEFA Champions League:");
			//for (int i = 0; i < UEFAChampionsLeagueNextSeason.Count; i++)
			//{
			//    Console.WriteLine(UEFAChampionsLeagueNextSeason[i].Name);
			//}
			for (int i = 0; i < UEFAEuropaLeagueTeams.Length; i++)
			{
				int uefaChampionsCount = 0;
				while (uefaChampionsCount < UEFAEuropaLeagueTeams[i])
				{
					Team t = premierQueues[i].Dequeue();
					if (!UEFAEuropaLeagueNextSeason.Contains(t))
					{
						UEFAEuropaLeagueNextSeason.Add(t);
						uefaChampionsCount++;
					}
				}
			}
			//Console.WriteLine("UEFA Europa League:");
			//for (int i = 0; i < UEFAChampionsLeagueNextSeason.Count; i++)
			//{
			//    Console.WriteLine(UEFAEuropaLeagueNextSeason[i].Name);
			//}
		}

		private void SortTeams(List<Team> teams)
		{
			teams.Sort((x, y) =>
			{
				return x.Name.CompareTo(y.Name);
			});
		}

		private void SortLeagues(List<League> leagues)
		{
			leagues.Sort((x, y) =>
			{
				int ret = 0;
				ret = x.tier.CompareTo(y.tier);
				if (ret == 0)
				{
					ret = x.LeagueName.CompareTo(y.LeagueName);
				}
				return ret;
			});
		}

		private void SortCountries(List<LeagueCountry> countries)
		{
			countries.Sort((x, y) =>
			{
				return x.CountryName.CompareTo(y.CountryName);
			});
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

		private int CalculateSalary(Player p, int baseSalary)
		{
			int score = p.overall;
			int salary = 0;

			while (score > 0)
			{
				salary += baseSalary + Dice.Instance.d100.Roll() + Dice.Instance.d100.Roll()
					+ Dice.Instance.d100.Roll();
				score--;
			}

			return salary;
		}

		private void PopulateLeagueCountry()
		{
			leagueCountryDict.Add("1A Pro League", new LeagueCountry("Belgium"));
			leagueCountryDict.Add("3. Liga", new LeagueCountry("Germany"));
			leagueCountryDict.Add("3F Superliga", new LeagueCountry("Denmark"));
			leagueCountryDict.Add("Ö. Bundesliga", new LeagueCountry("Austria"));
			leagueCountryDict.Add("A-League", new LeagueCountry("Australia"));
			leagueCountryDict.Add("Allsvenskan", new LeagueCountry("Sweden"));
			leagueCountryDict.Add("Česká Liga", new LeagueCountry("Czech Republic"));
			leagueCountryDict.Add("Bundesliga", new LeagueCountry("Germany"));
			leagueCountryDict.Add("Bundesliga 2", new LeagueCountry("Germany"));
			leagueCountryDict.Add("CSL", new LeagueCountry("China"));
			leagueCountryDict.Add("CSSL", new LeagueCountry("Switzerland"));
			leagueCountryDict.Add("EFL Championship", new LeagueCountry("England"));
			leagueCountryDict.Add("EFL League One", new LeagueCountry("England"));
			leagueCountryDict.Add("EFL League Two", new LeagueCountry("England"));
			leagueCountryDict.Add("Eliteserien", new LeagueCountry("Norway"));
			leagueCountryDict.Add("Eredivisie", new LeagueCountry("Netherlands"));
			leagueCountryDict.Add("Finnliiga", new LeagueCountry("Finland"));
			leagueCountryDict.Add("Hellas Liga", new LeagueCountry("Greece"));
			leagueCountryDict.Add("ISL", new LeagueCountry("India"));
			leagueCountryDict.Add("K League 1", new LeagueCountry("Korea"));
			leagueCountryDict.Add("La Liga", new LeagueCountry("Spain"));
			leagueCountryDict.Add("LALIGA HYPERMOTION", new LeagueCountry("Spain"));
			leagueCountryDict.Add("Libertadores", new LeagueCountry("Argentina"));
			leagueCountryDict.Add("Liga Azerbaijan", new LeagueCountry("Azerbaijan"));
			leagueCountryDict.Add("Liga Colombia", new LeagueCountry("Colombia"));
			leagueCountryDict.Add("Liga Cyprus", new LeagueCountry("Cyprus"));
			leagueCountryDict.Add("Liga Hrvatska", new LeagueCountry("Croatia"));
			leagueCountryDict.Add("Liga Portugal", new LeagueCountry("Portugal"));
			leagueCountryDict.Add("Ligue 1 McDonald's", new LeagueCountry("France"));
			leagueCountryDict.Add("Ligue 2 BKT", new LeagueCountry("France"));
			leagueCountryDict.Add("Magyar Liga", new LeagueCountry("Hungary"));
			leagueCountryDict.Add("MLS", new LeagueCountry("USA"));
			leagueCountryDict.Add("PKO BP Ekstraklasa", new LeagueCountry("Poland"));
			leagueCountryDict.Add("Premier League", new LeagueCountry("England"));
			leagueCountryDict.Add("Primera División", new LeagueCountry("Argentina"));
			leagueCountryDict.Add("ROSHN Saudi League", new LeagueCountry("Saudia Arabia"));
			leagueCountryDict.Add("Scottish Prem", new LeagueCountry("Scotland"));
			leagueCountryDict.Add("Serie A Enilive", new LeagueCountry("Italy"));
			leagueCountryDict.Add("Serie BKT", new LeagueCountry("Italy"));
			leagueCountryDict.Add("SSE Airtricity PD", new LeagueCountry("Ireland"));
			leagueCountryDict.Add("Sudamericana", new LeagueCountry("Argentina"));
			leagueCountryDict.Add("SUPERLIGA", new LeagueCountry("Romania"));
			leagueCountryDict.Add("Trendyol Süper Lig", new LeagueCountry("Turkey"));
			leagueCountryDict.Add("Ukrayina Liha", new LeagueCountry("Ukraine"));
			leagueCountryDict.Add("United Emirates League", new LeagueCountry("United Emirates"));
			leagueCountryDict.Add("UEFA Europa League", new LeagueCountry("UEFA Europa League"));
			leagueCountryDict.Add("UEFA Champions League", new LeagueCountry("UEFA Champions League"));
		}

		private void PopulateLeagueTier()
		{
			leagueTier.Add("1A Pro League", 2);
			leagueTier.Add("3. Liga", 4);
			leagueTier.Add("3F Superliga", 3);
			leagueTier.Add("Ö. Bundesliga", 4);
			leagueTier.Add("A-League", 5);
			leagueTier.Add("Allsvenskan", 3);
			leagueTier.Add("Česká Liga", 3);
			leagueTier.Add("Bundesliga", 2);
			leagueTier.Add("Bundesliga 2", 3);
			leagueTier.Add("CSL", 5);
			leagueTier.Add("CSSL", 4);
			leagueTier.Add("EFL Championship", 2);
			leagueTier.Add("EFL League One", 3);
			leagueTier.Add("EFL League Two", 4);
			leagueTier.Add("Eliteserien", 4);
			leagueTier.Add("Eredivisie", 1);
			leagueTier.Add("Finnliiga", 4);
			leagueTier.Add("Hellas Liga", 4);
			leagueTier.Add("ISL", 5);
			leagueTier.Add("K League 1", 3);
			leagueTier.Add("La Liga", 1);
			leagueTier.Add("LALIGA HYPERMOTION", 2);
			leagueTier.Add("Libertadores", 2);
			leagueTier.Add("Liga Azerbaijan", 5);
			leagueTier.Add("Liga Colombia", 2);
			leagueTier.Add("Liga Cyprus", 5);
			leagueTier.Add("Liga Hrvatska", 5);
			leagueTier.Add("Liga Portugal", 1);
			leagueTier.Add("Ligue 1 McDonald's", 1);
			leagueTier.Add("Ligue 2 BKT", 2);
			leagueTier.Add("Magyar Liga", 5);
			leagueTier.Add("MLS", 3);
			leagueTier.Add("PKO BP Ekstraklasa", 4);
			leagueTier.Add("Premier League", 1);
			leagueTier.Add("Primera División", 2);
			leagueTier.Add("ROSHN Saudi League", 5);
			leagueTier.Add("Scottish Prem", 3);
			leagueTier.Add("Serie A Enilive", 1);
			leagueTier.Add("Serie BKT", 2);
			leagueTier.Add("SSE Airtricity PD", 3);
			leagueTier.Add("Sudamericana", 2);
			leagueTier.Add("SUPERLIGA", 5);
			leagueTier.Add("Trendyol Süper Lig", 5);
			leagueTier.Add("Ukrayina Liha", 4);
			leagueTier.Add("United Emirates League", 5);
		}

		private void PopulateFormations()
		{
			// Premier League
			formations.Add("AFC Bournemouth", "4231");
			formations.Add("Arsenal", "433");
			formations.Add("Aston Villa", "4231");
			formations.Add("Brentford", "352");
			formations.Add("Brighton", "4231");
			formations.Add("Chelsea", "4231");
			formations.Add("Crystal Palace", "433");
			formations.Add("Everton", "4411");
			formations.Add("Fulham", "4231");
			formations.Add("Ipswich", "4231");
			formations.Add("Leicester City", "433");
			formations.Add("Liverpool", "433");
			formations.Add("Manchester City", "4231");
			formations.Add("Manchester United", "4231");
			formations.Add("Newcastle Utd", "433");
			formations.Add("Nott'm Forest", "4231");
			formations.Add("Southampton", "352");
			formations.Add("West Ham", "442");
			formations.Add("Wolves", "3421");

			// EFL Championship
			formations.Add("Luton Town", "3421");
			formations.Add("Leeds United", "352");
			formations.Add("Plymouth Argyle", "352");
			formations.Add("Bristol City", "352");
			formations.Add("Cardiff City", "352");
			formations.Add("Burnley", "442");
			formations.Add("Derby County", "352");
			formations.Add("Norwich", "352");
			formations.Add("Middlesbrough", "352");
			formations.Add("Sheffield Utd", "352");
			formations.Add("Sunderland", "352");
			formations.Add("Millwall", "352");
			formations.Add("Swansea City", "352");
			formations.Add("Blackburn Rovers", "4231");
			formations.Add("West Brom", "4231");
			formations.Add("Hull City", "4231");
			formations.Add("Sheffield Wed", "4231");
			formations.Add("QPR", "433");
			formations.Add("Portsmouth", "433");
			formations.Add("Stoke City", "433");
			formations.Add("Oxford United", "3421");
			formations.Add("Watford", "3421");
			formations.Add("Coventry City", "343");
			formations.Add("Preston", "4141");

			// EFL League One
			formations.Add("Birmingham City", "352");
			formations.Add("Leyton Orient", "352");
			formations.Add("Rotherham Utd", "352");
			formations.Add("Mansfield Town", "352");
			formations.Add("Bolton", "352");
			formations.Add("Bristol Rovers", "352");
			formations.Add("Charlton Ath", "352");
			formations.Add("Wycombe", "4231");
			formations.Add("Huddersfield", "4231");
			formations.Add("Stockport", "4231");
			formations.Add("Stevenage", "4231");
			formations.Add("Shrewsbury", "4231");
			formations.Add("Reading", "4231");
			formations.Add("Crawley Town", "433");
			formations.Add("Northampton", "433");
			formations.Add("Wrexham", "433");
			formations.Add("Blackpool", "433");
			formations.Add("Peterborough", "433");
			formations.Add("Barnsley", "3421");
			formations.Add("Burton Albion", "3421");
			formations.Add("Wigan Athletic", "343");
			formations.Add("Cambridge Utd", "442");
			formations.Add("Exeter City", "442");

			// EFL League Two
			formations.Add("Harrogate Town", "352");
			formations.Add("Colchester", "352");
			formations.Add("MK Dons", "352");
			formations.Add("Tranmere Rovers", "352");
			formations.Add("Salford City", "352");
			formations.Add("Notts County", "352");
			formations.Add("Walsall", "352");
			formations.Add("AFC Wimbledon", "352");
			formations.Add("Doncaster", "352");
			formations.Add("Newport County", "352");
			formations.Add("Crewe Alexandra", "4231");
			formations.Add("Grimsby Town", "4231");
			formations.Add("Barrow", "4231");
			formations.Add("Bradford City", "433");
			formations.Add("Port Vale", "433");
			formations.Add("Carlisle United", "433");
			formations.Add("Swindon Town", "433");
			formations.Add("Gillingham", "433");
			formations.Add("Fleetwood Town", "3421");
			formations.Add("Chesterfield", "343");
			formations.Add("Morecambe", "343");
			formations.Add("Cheltenham Town", "442");
			formations.Add("Bromley FC", "442");
			formations.Add("Accrington", "41212");
		}
}
