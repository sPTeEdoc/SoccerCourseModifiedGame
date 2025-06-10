using Godot;
using System;
using System.Collections.Generic;

public class Player
{
	//public string firstName;
		//public string lastName;
		public int ID;
		public string fullName;
		public int overall;
		public int pace;
		public int shooting;
		public int passing;
		public int defending;
		public int physicality;
		public int acceleration;
		public int sprint;
		public int positioning;
		public int finishing;
		public int shotPower;
		public int longShot;
		public int volleys;
		public int penalties;
		public int vision;
		public int crossing;
		public int freekicks;
		public int shortPass;
		public int longPass;
		public int curve;
		public int dribbling;
		public int agility;
		public int balance;
		public int reactionTime;
		public int ballControl;
		public int composure;
		public int intercept;
		public int header;
		public int defenseAwareness;
		public int standTackle;
		public int slideTackle;
		public int jumping;
		public int stamina;
		public int strength;
		public int aggression;
		public Enums.Positions Position;
		public int weakFoot;
		public int skillMoves;
		public string preferredFoot;
		public string height;
		public string weight;
		public string secondPos;
		public int age;
		public string nation;
		public string playStyle;
		public int goalkeepingDiving;
		public int goalKeepingHandling;
		public int goalKeepingKicking;
		public int goalKeepingPositioning;
		public int goalKeepingReflexes;
		public int number;
		public int gamesOutDueToInjury;
		public int gamesOutDueToSuspension;
		public bool isInjured = false;
		public GameStats gameStats;
		//public GameStats seasonStats;
		//public GameStats fACupStats;
		public Dictionary<string, Dictionary<string, GameStats>> LeagueTeamSeasonStats;
		public double transferFee;
		public double salary;
		public string teamName = "";

		public Player(int id, string fullName)
		{
			//this.firstName = firstName;
			//this.lastName = lastName;
			this.ID = id;
			this.fullName = fullName;
			this.overall = 0;
			this.pace = 0;
			this.shooting = 0;
			this.passing = 0;
			this.defending = 0;
			this.physicality = 0;
			this.acceleration = 0;
			this.sprint = 0;
			this.positioning = 0;
			this.finishing = 0;
			this.shotPower = 0;
			this.longShot = 0;
			this.volleys = 0;
			this.penalties = 0;
			this.vision = 0;
			this.crossing = 0;
			this.freekicks = 0;
			this.shortPass = 0;
			this.longPass = 0;
			this.curve = 0;
			this.dribbling = 0;
			this.agility = 0;
			this.balance = 0;
			this.reactionTime = 0;
			this.ballControl = 0;
			this.composure = 0;
			this.intercept = 0;
			this.header = 0;
			this.defenseAwareness = 0;
			this.standTackle = 0;
			this.slideTackle = 0;
			this.jumping = 0;
			this.stamina = 0;
			this.strength = 0;
			this.aggression = 0;
			this.weakFoot = 0;
			this.skillMoves = 0;
			this.preferredFoot = "Right";
			this.height = "176cm / 5'9";
			this.weight = "73kg / 161lb";
			this.age = 25;
			this.nation = "Parts Unknown";
			this.playStyle = "";
			this.goalkeepingDiving = 0;
			this.goalKeepingHandling = 0;
			this.goalKeepingKicking = 0;
			this.goalKeepingPositioning = 0;
			this.goalKeepingReflexes = 0;
			this.number = 0;
			this.gamesOutDueToInjury = 0;
			this.gamesOutDueToSuspension = 0;

			isInjured = false;
			gameStats = new GameStats();
			this.LeagueTeamSeasonStats = new Dictionary<string, Dictionary<string, GameStats>>();
		}

		public void InitializePlayerStats(string leagueName, string teamName)
		{
			if (!this.LeagueTeamSeasonStats.ContainsKey(leagueName))
			{
				Dictionary<string, GameStats> teamStats = new Dictionary<string, GameStats>();
				GameStats gameStats = new GameStats();
				teamStats.Add(teamName, gameStats);
				this.LeagueTeamSeasonStats.Add(leagueName, teamStats);
			}
		}

		public Player Clone()
		{
			Player player = new Player(this.ID, this.fullName);
			player.teamName = this.teamName;
			player.overall = this.overall;
			player.pace = this.pace;
			player.shooting = this.shooting;
			player.passing = this.passing;
			player.defending = this.defending;
			player.physicality = this.physicality;
			player.acceleration = this.acceleration;
			player.sprint = this.sprint;
			player.positioning = this.positioning;
			player.finishing = this.finishing;
			player.shotPower = this.shotPower;
			player.longShot = this.longShot;
			player.volleys = this.volleys;
			player.penalties = this.penalties;
			player.vision = this.vision;
			player.crossing = this.crossing;
			player.freekicks = this.freekicks;
			player.shortPass = this.shortPass;
			player.longPass = this.longPass;
			player.curve = this.curve;
			player.dribbling = this.dribbling;
			player.agility = this.agility;
			player.balance = this.balance;
			player.reactionTime = this.reactionTime;
			player.ballControl = this.ballControl;
			player.composure = this.composure;
			player.intercept = this.intercept;
			player.header = this.header;
			player.defenseAwareness = this.defenseAwareness;
			player.standTackle = this.standTackle;
			player.slideTackle = this.slideTackle;
			player.jumping = this.jumping;
			player.stamina = this.stamina;
			player.strength = this.strength;
			player.aggression = this.aggression;
			player.Position = this.Position;
			player.weakFoot = this.weakFoot;
			player.skillMoves = this.skillMoves;
			player.preferredFoot = this.preferredFoot;
			player.height = this.height;
			player.weight = this.weight;
			player.secondPos = this.secondPos;
			player.age = this.age;
			player.nation = this.nation;
			player.playStyle = this.playStyle;
			player.goalkeepingDiving = this.goalkeepingDiving;
			player.goalKeepingHandling = this.goalKeepingHandling;
			player.goalKeepingKicking = this.goalKeepingKicking;
			player.goalKeepingPositioning = this.goalKeepingPositioning;
			player.goalKeepingReflexes = this.goalKeepingReflexes;
			player.number = this.number;
			gamesOutDueToInjury = 0;
			return player;
		}
}
