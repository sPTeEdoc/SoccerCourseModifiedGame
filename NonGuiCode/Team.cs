using FunnyOldGameRedux;
using FunnyOldGameRedux.NonGuiCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunnyOldGame
{
    public class Team
    {
        // to do - make a bench and delineate between the bench and starting rosters

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
        public List<Player> bench;
        public List<Player> reserves;
        public List<Player> completeRoster;
        private Player goalKeeper;
        public Player GoalKeeper
        {
            get
            {
                return this.AvailableOnFieldPlayers // Assumes starting11 is the list of active players
                       .Where(p => p.Position == Enums.Positions.Goalkeeper) // Check availability
                       .FirstOrDefault(); // Get the best one
            }
        }
        public GameStats gameStats;
        public Dictionary<string, GameStats> seasonStats;
        public string imageFile = "";
        public string formation = "";
        public string LeagueName = "";
        public int tier; // 1 = premier, 2 = 2nd or 3rd division, 3 = 4th or 5th, etc.
        Dictionary<Team, List<int>> opponentsFixtures;
        public List<Player> StartingEleven;
        //public List<Player> CurrentEleven { get; set; }
        public Dictionary<string, Dictionary<int, GameStats>> playerStats = new Dictionary<string, Dictionary<int, GameStats>>();
        public YouthAcademy youthAcademy;

        public int Reputation { get; set; } // e.g., 1-100, affects player interest, sponsor deals
        public decimal ClubBalance { get; set; } // Current cash on hand
        public decimal WageBudgetRemaining { get; set; } // How much can be spent on wages

        // --- Staff Management ---
        /// <summary>
        /// The comprehensive list of all coaches employed by the club.
        /// </summary>
        public List<Coach> Staff { get; set; }

        /// <summary>
        /// A mapping of training categories to the coaches specifically assigned to them.
        /// </summary>
        public Dictionary<Enums.TrainingCategory, List<Coach>> AssignedCoaches { get; set; }

        // --- Facilities ---
        /// <summary>
        /// The overall level of the club's training facilities (e.g., 1-5 or 1-10 scale).
        /// </summary>
        public double TrainingFacilitiesLevel { get; set; }

        /// <summary>
        /// List of all medical staff employed by the club.
        /// </summary>
        public List<MedicalStaff> MedicalStaff { get; set; }

        /// <summary>
        /// The overall level of the club's medical facilities (e.g., 1-5 scale).
        /// </summary>
        public double MedicalFacilitiesLevel { get; set; }

        public Enums.Formation CurrentFormation = Enums.Formation.FourThreeThree;

        public Enums.TeamTactic CurrentTactic { get; set; } // Default to Balance

        public Enums.PressingStyle PressingStyle { get; set; } // Default

        // New properties for the defensive instructions:
        public Enums.DefensiveLineDepth CurrentDefensiveLineDepth { get; set; }
        public Enums.MarkingStyle CurrentMarkingStyle { get; set; }
        public Enums.TacklingAggression CurrentTacklingAggression { get; set; }
        public Enums.PressingTrigger CurrentPressingTrigger { get; set; }
        public Enums.SweeperKeeperStyle SweeperKeeperStyle { get; set; }

        public bool justWonPossessionFlag { get; set; }
        public int possessionCounter { get; set; }

        public Enums.SetPieceRoutine AttackingFreeKickRoutine { get; set; } // = Enums.SetPieceRoutine.None;
        public Enums.SetPieceRoutine AttackingCornerRoutine { get; set; } // = Enums.SetPieceRoutine.CrowdTheBox; // Example for corners too

        public bool HasMadeRedCardAdjustment = false;

        // 1. Private backing field for the cached list
        private List<Player> _cachedOnFieldOutfielders;

        // 2. A flag to indicate if the cache is stale (invalid)
        private bool _isOutfieldersCacheStale = true;

        private List<Player> _cachedAvailableOnFieldPlayers;

        private List<Player> _cachedAvailableOutfielders;

        private void SetOnFieldProperties()
        {
            if (_isOutfieldersCacheStale)
            {
                // Note: Consider using something like List<Player>.Capacity = StartingEleven.Count - 1
                // if you know the typical number of outfielders to avoid reallocations,
                // or just let it grow. For 10-11 players, it's not a huge deal.
                _cachedOnFieldOutfielders = new List<Player>();
                _cachedAvailableOnFieldPlayers = new List<Player>();
                _cachedAvailableOutfielders = new List<Player>();
                foreach (var player in CurrentEleven)
                {
                    // no need to call IsAvailable. Upon reflection, I over call this. Injured players should stay until subbed out.
                    // Sent off players should stay on so they're slot isn't accidentally used.
                    if (player.Position != Enums.Positions.Goalkeeper)
                    {
                        _cachedOnFieldOutfielders.Add(player);
                        if (player.AvailableOption)
                            _cachedAvailableOutfielders.Add(player);
                    }
                    if (player.AvailableOption)
                        _cachedAvailableOnFieldPlayers.Add(player);
                }
                _isOutfieldersCacheStale = false; // Mark cache as fresh
            }
        }

        public List<Player> AvailableOnFieldOutfielders
        {
            get
            {
                if (_isOutfieldersCacheStale)
                    SetOnFieldProperties();
                return _cachedAvailableOutfielders;
            }
        }

        public List<Player> AvailableOnFieldPlayers
        {
            get
            {
                if (_isOutfieldersCacheStale)
                    SetOnFieldProperties();
                return _cachedAvailableOnFieldPlayers;
            }
        }

        // Public property to access the outfielders
        public List<Player> CurrentOutfielders
        {
            get 
            {
                if (_isOutfieldersCacheStale)
                    SetOnFieldProperties();
                return _cachedOnFieldOutfielders;
            }
        }

        public List<Player> CurrentEleven { get; set; }

        public List<Player> CurrentBench { get; set; }

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
            this.StartingEleven = new List<Player>();
            youthAcademy = new YouthAcademy();
            CurrentTactic = Enums.TeamTactic.Balanced;

            // Initialize lists and dictionaries in the constructor (for older C# compatibility)
            Staff = new List<Coach>();
            AssignedCoaches = new Dictionary<Enums.TrainingCategory, List<Coach>>();
            MedicalStaff = new List<MedicalStaff>(); // Initialize new list

            // Populate AssignedCoaches with empty lists for each category initially
            foreach (Enums.TrainingCategory category in Enum.GetValues(typeof(Enums.TrainingCategory)))
            {
                AssignedCoaches.Add(category, new List<Coach>());
            }
            CurrentTactic = Enums.TeamTactic.Balanced;

            justWonPossessionFlag = false;
            possessionCounter = 0;
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
            this.StartingEleven = new List<Player>();
            youthAcademy = new YouthAcademy();

            // Initialize lists and dictionaries in the constructor (for older C# compatibility)
            Staff = new List<Coach>();
            AssignedCoaches = new Dictionary<Enums.TrainingCategory, List<Coach>>();
            MedicalStaff = new List<MedicalStaff>(); // Initialize new list

            // Populate AssignedCoaches with empty lists for each category initially
            foreach (Enums.TrainingCategory category in Enum.GetValues(typeof(Enums.TrainingCategory)))
            {
                AssignedCoaches.Add(category, new List<Coach>());
            }
            CurrentTactic = Enums.TeamTactic.Balanced;

            justWonPossessionFlag = false;
            possessionCounter = 0;
        }

        public Team(string name, string leagueName, int reputation, decimal initialBalance, decimal initialWageBudget, double trainingFacilitiesLevel)
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
            this.StartingEleven = new List<Player>();

            Reputation = reputation;
            ClubBalance = initialBalance;
            WageBudgetRemaining = initialWageBudget;
            TrainingFacilitiesLevel = trainingFacilitiesLevel;

            // Initialize lists and dictionaries in the constructor (for older C# compatibility)
            Staff = new List<Coach>();
            AssignedCoaches = new Dictionary<Enums.TrainingCategory, List<Coach>>();
            MedicalStaff = new List<MedicalStaff>(); // Initialize new list

            // Populate AssignedCoaches with empty lists for each category initially
            foreach (Enums.TrainingCategory category in Enum.GetValues(typeof(Enums.TrainingCategory)))
            {
                AssignedCoaches.Add(category, new List<Coach>());
            }
            
            youthAcademy = new YouthAcademy();

            // Initialize lists and dictionaries in the constructor (for older C# compatibility)
            Staff = new List<Coach>();
            AssignedCoaches = new Dictionary<Enums.TrainingCategory, List<Coach>>();
            MedicalStaff = new List<MedicalStaff>(); // Initialize new list

            // Populate AssignedCoaches with empty lists for each category initially
            foreach (Enums.TrainingCategory category in Enum.GetValues(typeof(Enums.TrainingCategory)))
            {
                AssignedCoaches.Add(category, new List<Coach>());
            }
            CurrentTactic = Enums.TeamTactic.Balanced;

            justWonPossessionFlag = false;
            possessionCounter = 0;
        }

        public Team Clone()
        {
            Team team = new Team(this.Name, this.NickName);
            foreach (Player p in this.completeRoster)
            {
                team.completeRoster.Add(p.Clone());
            }
            team.ConfigureRoster();

            team.Reputation = this.Reputation;
            team.ClubBalance = this.ClubBalance;
            team.WageBudgetRemaining = this.WageBudgetRemaining;
            team.TrainingFacilitiesLevel = this.TrainingFacilitiesLevel;

            team.Staff = new List<Coach>();
            team.AssignedCoaches = new Dictionary<Enums.TrainingCategory, List<Coach>>();
            team.MedicalStaff = new List<MedicalStaff>(); // Initialize new list

            // Initialize lists and dictionaries in the constructor (for older C# compatibility)
            foreach (Coach c in this.Staff)
                team.Staff.Add(c.Clone());

            foreach (MedicalStaff physio in this.MedicalStaff)
                team.MedicalStaff.Add(physio.Clone());

            foreach (KeyValuePair<Enums.TrainingCategory, List<Coach>> entry in this.AssignedCoaches)
            {
                List<Coach> coaches = new List<Coach>();
                foreach (Coach c in entry.Value)
                {
                    coaches.Add(c.Clone());
                }
                team.AssignedCoaches.Add(entry.Key, coaches);
            }

            return team;
        }

        public void SetPlayerSentOff(Player player)
        {
            player.isSentOff = true;
            _isOutfieldersCacheStale = true; // Invalidate the cache
        }

        public void SetPlayerSubstituted(Player player)
        {
            player.IsSubstituted = true;
            _isOutfieldersCacheStale = true; // Invalidate the cache
        }

        // You might also add a generic InvalidateOutfieldersCache() method
        // to call whenever a change occurs that affects player availability (injuries, suspensions etc.)
        public void InvalidateOutfieldersCache()
        {
            _isOutfieldersCacheStale = true;
        }

        /// <summary>
        /// Assigns a coach to a specific training category.
        /// </summary>
        public void AssignCoach(Coach coach, Enums.TrainingCategory category)
        {
            if (!Staff.Contains(coach))
            {
                //Console.WriteLine($"Error: {coach.Name} is not currently on staff.");
                return;
            }

            // Optional: Remove coach from any other categories they might be assigned to if single assignment
            // For simplicity, let's allow multiple assignments for now, but in a real game you'd control this.

            if (!AssignedCoaches[category].Contains(coach))
            {
                AssignedCoaches[category].Add(coach);
                //Console.WriteLine($"{coach.Name} assigned to {category} training.");
            }
            else
            {
                //Console.WriteLine($"{coach.Name} is already assigned to {category} training.");
            }
        }

        /// <summary>
        /// Gets the aggregated effective coaching skill for a given training category.
        /// </summary>
        public int GetEffectiveCoachingSkill(Enums.TrainingCategory category)
        {
            // Declare the list variable *before* using it in TryGetValue
            List<Coach> coaches = null; // Initialize to null

            // Now use TryGetValue, passing the pre-declared variable
            if (!AssignedCoaches.TryGetValue(category, out coaches) || coaches.Count == 0)
            {
                return 1; // Default minimal skill if no coaches assigned or list is empty
            }

            int maxSkill = 0;
            Coach bestCoachForCategory = null;

            foreach (Coach c in coaches)
            {
                if (bestCoachForCategory == null)
                {
                    bestCoachForCategory = c;
                }
                int coachSkill = 0;
                if (c.SkillRatings.ContainsKey(category))
                {
                    coachSkill = c.SkillRatings[category];
                }
                if (coachSkill > maxSkill)
                {
                    bestCoachForCategory = c;
                    maxSkill = coachSkill;
                }
            }

            // For now, let's take the skill of the *best* coach assigned to that category.
            return maxSkill;
        }

        // --- NEW: Method to get Effective Medical Skill ---
        /// <summary>
        /// Gets the aggregated effective medical skill from all employed medical staff.
        /// </summary>
        public int GetEffectiveMedicalSkill()
        {
            if (MedicalStaff == null || MedicalStaff.Count == 0)
            {
                return 1; // Minimum effective skill if no medical staff
            }

            // For simplicity, let's take the skill of the *best* medical staff member.
            // A more advanced system might average the top 2-3 or sum them up to a cap.
            return MedicalStaff.Max(ms => ms.MedicalSkill);
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
            ResetStats(this.completeRoster);
        }

        private void ResetStats(List<Player> players)
        {
            foreach (Player p in players)
                p.gameStats = new GameStats();
        }

        private void StorePlayerSeasonStats(string leagueName)
        {
            StoreSeasonStats(this.completeRoster, leagueName);
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

        private void StoreSeasonStats(List<Player> players, string leagueName)
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
            TotalTeamGameStats(this.completeRoster);
        }

        private void TotalTeamGameStats(List<Player> players)
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
            gameStats.PassAttempts += p.gameStats.PassAttempts;
            gameStats.SuccessfulPasses += p.gameStats.SuccessfulPasses;
        }

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

        public void SetUpMatchLineups()
        {
            _isOutfieldersCacheStale = true;
            CurrentEleven = new List<Player>();
            foreach (Player p in this.StartingEleven)
            {
                CurrentEleven.Add(p);
                p.ResetInjuryStatus(); // shouldn't be necessary, but just in case
            }

            CurrentBench = new List<Player>();
            foreach (Player p in this.bench)
            {
                CurrentBench.Add(p);
                p.ResetInjuryStatus(); // shouldn't be necessary, but just in case
            }
        }

        public void ConfigureRoster()
        {
            this.formation = FormationOptimizer.Instance.DetermineBestFormationAllPositionsFactored(this.completeRoster);
            for (int i = 0; i < this.completeRoster.Count; i++)
            {
                Player p = completeRoster[i];
                TeamRepository.Instance.DetermineRolesForPlayer(ref p);
            }
            //Dictionary<Player, int> CenterBacks = new Dictionary<Player, int>();
            //Dictionary<Player, int> LeftBacks = new Dictionary<Player, int>();
            //Dictionary<Player, int> RightBacks = new Dictionary<Player, int>();

            //Dictionary<Player, int> RightWingForwards = new Dictionary<Player, int>();
            //Dictionary<Player, int> LeftWingForwards = new Dictionary<Player, int>();
            //Dictionary<Player, int> Strikers = new Dictionary<Player, int>();

            //Dictionary<Player, int> CentralAttackingMidfielders = new Dictionary<Player, int>();
            //Dictionary<Player, int> CentralDefendingMidfielders = new Dictionary<Player, int>();
            //Dictionary<Player, int> CentralMidfielders = new Dictionary<Player, int>();
            //Dictionary<Player, int> LeftMidfielders = new Dictionary<Player, int>();
            //Dictionary<Player, int> RightMidfielders = new Dictionary<Player, int>();

            Dictionary<Player, int> goalkeepers = new Dictionary<Player, int>();

            bench = new List<Player>();
            reserves = new List<Player>();

            List<Player> availableOutfielders = new List<Player>();
            List<Player> availableGoalKeepers = new List<Player>();

            foreach (Player p in completeRoster)
            {
                if (!p.IsAvailable)
                    reserves.Add(p);
                else
                {
                    if (TeamRepository.Instance.IsGoalKeeper(p.Position))
                    {
                        if (p.IsAvailable)
                            availableGoalKeepers.Add(p);
                    }
                    else
                    {
                        if (p.IsAvailable)
                            availableOutfielders.Add(p);
                    }
                    //if (p.Position == Enums.Positions.Goalkeeper)
                    //    goalkeepers.Add(p, playerScore);
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

                    //if (p.Position == Enums.Positions.CentralAttackingMidfielder)
                    //    CentralAttackingMidfielders.Add(p, playerScore);
                    //if (p.Position == Enums.Positions.CentralDefendingMidfielder)
                    //    CentralDefendingMidfielders.Add(p, playerScore);
                    //if (p.Position == Enums.Positions.CentralMidfielder)
                    //    CentralMidfielders.Add(p, playerScore);
                    //if (p.Position == Enums.Positions.LeftMidfielder)
                    //    LeftMidfielders.Add(p, playerScore);
                    //if (p.Position == Enums.Positions.RightMidfielder)
                    //    RightMidfielders.Add(p, playerScore);

                    //if (p.Position == Enums.Positions.Striker)
                    //    Strikers.Add(p, playerScore);
                    //if (p.Position == Enums.Positions.LeftWingForward)
                    //    LeftWingForwards.Add(p, playerScore);
                    //if (p.Position == Enums.Positions.RightWingForward)
                    //    RightWingForwards.Add(p, playerScore);

                    //if (p.Position == Enums.Positions.CenterBack)
                    //    CenterBacks.Add(p, playerScore);
                    //if (p.Position == Enums.Positions.RightBack)
                    //    RightBacks.Add(p, playerScore);
                    //if (p.Position == Enums.Positions.LeftBack)
                    //    LeftBacks.Add(p, playerScore);
                }
            }

            foreach (Player p in availableGoalKeepers)
            {
                goalkeepers.Add(p, p.trueRating.OverallForGameCalculations);
            }

            SortListByPlayerScore(goalkeepers);

            //List<Player> gks = SortListByPlayerScore(goalkeepers);

            //List<Player> cams = SortListByPlayerScore(CentralAttackingMidfielders);
            //List<Player> cdms = SortListByPlayerScore(CentralDefendingMidfielders);
            //List<Player> cmfs = SortListByPlayerScore(CentralMidfielders);
            //List<Player> lmfs = SortListByPlayerScore(LeftMidfielders);
            //List<Player> rmfs = SortListByPlayerScore(RightMidfielders);

            //List<Player> rwfs = SortListByPlayerScore(RightWingForwards);
            //List<Player> lwfs = SortListByPlayerScore(LeftWingForwards);
            //List<Player> sts = SortListByPlayerScore(Strikers);

            //List<Player> cbs = SortListByPlayerScore(CenterBacks);
            //List<Player> lbs = SortListByPlayerScore(LeftBacks);
            //List<Player> rbs = SortListByPlayerScore(RightBacks);
            StartingEleven = new List<Player>();
            StartingEleven.Add(availableGoalKeepers[0]);
            availableGoalKeepers.Remove(availableGoalKeepers[0]);

            if (formation == "4-3-3")
            {
                //[9][8][7][6][5][4][3][2][1][0]
                //LB,CB,CB,RB,CDM,RCM,LCM,LWF,RWF,ST
                //4-3-3: Balanced, often with wingers.
                CurrentFormation = Enums.Formation.FourThreeThree;
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.LeftBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.RightBack));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralDefendingMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.LeftMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.RightMidfielder));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.LeftWingForward));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.RightWingForward));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.Striker));

                //StartingEleven.Add(GetNextPlayerAvailable(lbs, rbs, cbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, rbs, lbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, lbs, rbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(rbs, lbs, cbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(lmfs, cmfs, cdms, rmfs, cams, sts, lbs, rbs, cbs, rwfs, lwfs));
                //StartingEleven.Add(GetNextPlayerAvailable(cmfs, cams, cdms, rmfs, lmfs, sts, lbs, rbs, cbs, rwfs, lwfs));
                //StartingEleven.Add(GetNextPlayerAvailable(rmfs, cmfs, cams, lmfs, cdms, sts, lbs, rbs, cbs, rwfs, lwfs));

                //StartingEleven.Add(GetNextPlayerAvailable(lwfs, rwfs, sts, cams, cmfs, lmfs, rmfs, cdms, lbs, rbs, cbs));
                //StartingEleven.Add(GetNextPlayerAvailable(rwfs, lwfs, sts, cams, cmfs, lmfs, rmfs, cdms, lbs, rbs, cbs));
                //StartingEleven.Add(GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, rmfs, lmfs, cdms, lbs, rbs, cbs));
            }
            else if (formation == "3-4-2-1")
            {
                //[9][8][7][6][5][4][3][2][1][0]
                //CB,CB,CB,CM,LM,CM,RM,AMF,AMF,ST
                // 3-4-2-1: Three at the back, strong midfield with attacking mids behind a lone striker.
                CurrentFormation = Enums.Formation.ThreeFourTwoOne;
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.LeftMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.RightMidfielder));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralAttackingMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralAttackingMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.Striker));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, rbs, lbs, cmfs, cdms, lmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, rbs, lbs, cmfs, cdms, lmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, lbs, rbs, cmfs, cdms, lmfs, rmfs, cams, lwfs, rwfs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(cdms, cmfs, cams, lmfs, rmfs, lwfs, rwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cmfs, lmfs, cdms, rmfs, cams, lwfs, rwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cmfs, rmfs, cdms, lmfs, cams, lwfs, rwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cams, cmfs, cdms, rmfs, lmfs, lwfs, rwfs, lbs, rbs, cbs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(lmfs, sts, lwfs, cams, cmfs, lmfs, rmfs, cdms, rbs, lbs, cbs));
                //StartingEleven.Add(GetNextPlayerAvailable(rmfs, sts, rwfs, cams, cmfs, lmfs, rmfs, cdms, rbs, lbs, cbs));
                //StartingEleven.Add(GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, lmfs, rmfs, cdms, rbs, lbs, cbs));
            }
            else if (formation == "3-5-2")
            {
                //[9][8][7][6][5][4][3][2][1][0]
                //CB,CB,CB,LCM,CM,RM,CM,AM,ST,ST
                //3-5-2: Three at the back, dominant midfield, two strikers.
                CurrentFormation = Enums.Formation.ThreeFiveTwo;
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.LeftMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.RightMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralAttackingMidfielder));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.Striker));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.Striker));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, rbs, lbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, rbs, lbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, lbs, rbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(lmfs, cmfs, cams, rmfs, cdms, lwfs, rwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(rmfs, cmfs, cdms, lmfs, cams, lwfs, rwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cmfs, rmfs, cams, lmfs, cdms, lwfs, rwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cams, cmfs, cdms, lmfs, rmfs, lwfs, rwfs, lbs, rbs, cbs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, lmfs, rmfs, cdms, lbs, rbs, cbs));
                //StartingEleven.Add(GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, lmfs, rmfs, cdms, lbs, rbs, cbs));
            }
            else if (formation == "4-4-1-1")
            {
                //[9][8][7][6][5][4][3][2][1][0]
                //LB,CB,CB,LB,LCM,CM,RCM,CM,AM,ST
                // 4-4-1-1: Four at the back, flat midfield, attacking midfielder supporting a striker.
                CurrentFormation = Enums.Formation.FourFourOneOne;
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.LeftBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.RightBack));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.LeftMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.RightMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralMidfielder));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralAttackingMidfielder));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.Striker));
                //StartingEleven.Add(GetNextPlayerAvailable(lbs, rbs, cbs, cdms, lmfs, cmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, rbs, lbs, cdms, lmfs, cmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, lbs, rbs, cdms, lmfs, cmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(rbs, lbs, cbs, cdms, lmfs, cmfs, rmfs, cams, lwfs, rwfs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(lmfs, cdms, cmfs, rmfs, cams, lwfs, rwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cmfs, cdms, lmfs, rmfs, cams, rwfs, lwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cmfs, cdms, rmfs, lmfs, cams, lwfs, rwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(rmfs, cmfs, cams, lmfs, cdms, rwfs, lwfs, lbs, rbs, cbs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(cams, sts, lwfs, rwfs, cmfs, rmfs, lmfs, cdms, lbs, cbs, rbs));
                //StartingEleven.Add(GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, lmfs, rmfs, cdms, lbs, cbs, rbs));
            }
            else if (formation == "4-4-2")
            {
                //[9][8][7][6][5][4][3][2][1][0]
                //LB,CB,CB,RB,CM,CM,CM,CM,ST,ST
                //4-4-2: Classic formation with two strikers.
                CurrentFormation = Enums.Formation.FourFourTwo;
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.LeftBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.RightBack));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.LeftMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.RightMidfielder));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.Striker));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.Striker));
                //StartingEleven.Add(GetNextPlayerAvailable(lbs, rbs, cbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, rbs, lbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, lbs, rbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(rbs, lbs, cbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(lmfs, cmfs, cdms, cmfs, cams, lwfs, rwfs, lbs, cbs, rbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cmfs, cdms, lmfs, rmfs, cams, rwfs, lwfs, lbs, cbs, rbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cmfs, cams, rmfs, lmfs, cams, lwfs, rwfs, lbs, cbs, rbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(rmfs, cmfs, cams, lmfs, cdms, rwfs, lwfs, lbs, cbs, rbs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, lmfs, rmfs, cdms, lbs, cbs, rbs));
                //StartingEleven.Add(GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, lmfs, rmfs, cdms, lbs, cbs, rbs));
            }
            else if (formation == "4-2-3-1")
            {
                //[9][8][7][6][5][4][3][2][1][0]
                //LB,CB,CB,RB,CDM,CDM,RM,RM,CAM,ST
                // 4-2-3-1: Four at the back, two holding midfielders, three attacking midfielders/wingers, one striker.
                CurrentFormation = Enums.Formation.FourTwoOneThree;
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.LeftBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.RightBack));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralDefendingMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralDefendingMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.LeftMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.RightMidfielder));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralAttackingMidfielder));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.Striker));
                //StartingEleven.Add(GetNextPlayerAvailable(lbs, rbs, cbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, rbs, lbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, lbs, rbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(rbs, lbs, cbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cdms, cmfs, rmfs, lmfs, cams, lwfs, rwfs, lbs, rbs, cbs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(lmfs, cmfs, rmfs, cams, cdms, lwfs, rwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cams, cmfs, lmfs, rmfs, cdms, lwfs, rwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(rmfs, cams, cmfs, lmfs, cdms, lwfs, rwfs, lbs, rbs, cbs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, lmfs, rmfs, cdms, lbs, rbs, cbs));
            }
            else if (formation == "4-1-2-1-2")
            {
                //[9][8][7][6][5][4][3][2][1][0]
                //LB,CB,CB,RB,CDM,CM,CM,CAM,ST,ST
                // 4-1-2-1-2 (Diamond): Four at the back, a defensive midfielder, two central, one attacking, two strikers.
                CurrentFormation = Enums.Formation.FourFourTwo;
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.LeftBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.RightBack));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralDefendingMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralAttackingMidfielder));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.Striker));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.Striker));
                //StartingEleven.Add(GetNextPlayerAvailable(lbs, rbs, cbs, cdms, lmfs, rmfs, cams, cmfs, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, rbs, lbs, cdms, lmfs, rmfs, cams, cmfs, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, lbs, rbs, cdms, lmfs, rmfs, cams, cmfs, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(rbs, lbs, cbs, cdms, lmfs, rmfs, cams, cmfs, lwfs, rwfs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(cdms, cmfs, cams, lmfs, rmfs, rwfs, lwfs, lbs, rbs, cbs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(lmfs, cmfs, rmfs, cams, cdms, lwfs, rwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(rmfs, cmfs, lmfs, cams, cdms, rwfs, lwfs, lbs, rbs, cbs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(cams, cmfs, sts, lmfs, rmfs, lwfs, rwfs, lbs, rbs, cbs, cdms));

                //StartingEleven.Add(GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, rmfs, lmfs, lbs, rbs, cbs, cdms));
                //StartingEleven.Add(GetNextPlayerAvailable(sts, lwfs, rwfs, cams, cmfs, lmfs, rmfs, lbs, rbs, cbs, cdms));
            }
            else if (formation == "3-4-3")
            {
                //[9][8][7][6][5][4][3][2][1][0]
                //CB,CB,CB,RB,LM,CM,RM,LWF,RWF,ST
                //3-4-3: Three at the back, four in midfield (often wide), three attacking forwards.

                CurrentFormation = Enums.Formation.ThreeFourthree;
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.LeftMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.RightMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralMidfielder));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.LeftWingForward));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.RightWingForward));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.Striker));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, rbs, lbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, rbs, lbs, cdms, lmfs, cmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, lbs, rbs, cdms, lmfs, rmfs, cmfs, cams, lwfs, rwfs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cmfs, lmfs, rmfs, cams, cdms, rwfs, lwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cmfs, rmfs, lmfs, cams, cdms, lwfs, rwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cams, cmfs, rmfs, lmfs, cdms, rwfs, lwfs, lbs, rbs, cbs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(lmfs, sts, lwfs, cams, cmfs, rmfs, lmfs, lbs, rbs, cbs, cdms));
                //StartingEleven.Add(GetNextPlayerAvailable(rmfs, sts, rwfs, cams, cmfs, lmfs, rmfs, lbs, rbs, cbs, cdms));
                //StartingEleven.Add(GetNextPlayerAvailable(sts, lwfs, rwfs, cams, rmfs, cmfs, lmfs, lbs, rbs, cbs, cdms));
            }
            else
            {
                //4-2-1-3
                //[9][8][7][6][5][4][3][2][1][0]
                //LB,CB,CB,RB,CDM,CDM,CAM,LWF,RWF,ST
                // 4-2-1-3: Four at the back, two holding midfielders, one attacking midfielder, three true forwards.
                CurrentFormation = Enums.Formation.FourTwoOneThree;
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.LeftBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CenterBack));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.RightBack));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralDefendingMidfielder));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralDefendingMidfielder));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.CentralAttackingMidfielder));

                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.LeftWingForward));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.RightWingForward));
                StartingEleven.Add(GetNextPlayerAvailable(ref availableOutfielders, Enums.Positions.Striker));
                //StartingEleven.Add(GetNextPlayerAvailable(lbs, rbs, cbs, cdms, cmfs, lmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, rbs, lbs, cdms, lmfs, cmfs, rmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cbs, lbs, rbs, cdms, lmfs, rmfs, cmfs, cams, lwfs, rwfs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(rbs, lbs, cbs, cdms, lmfs, rmfs, cmfs, cams, lwfs, rwfs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(cdms, cmfs, lmfs, rmfs, cdms, lwfs, rwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cams, cmfs, cdms, lmfs, rmfs, rwfs, lwfs, lbs, rbs, cbs, sts));
                //StartingEleven.Add(GetNextPlayerAvailable(cams, rmfs, lmfs, cmfs, cdms, lwfs, rwfs, lbs, rbs, cbs, sts));

                //StartingEleven.Add(GetNextPlayerAvailable(lwfs, rwfs, sts, cams, cmfs, lmfs, rmfs, lbs, rbs, cbs, cdms));
                //StartingEleven.Add(GetNextPlayerAvailable(rwfs, lwfs, sts, cams, cmfs, rmfs, lmfs, lbs, rbs, cbs, cdms));
                //StartingEleven.Add(GetNextPlayerAvailable(sts, lwfs, rwfs, cams, rmfs, cmfs, lmfs, lbs, rbs, cbs, cdms));
            }

            //OffScreen[4]=Starting[9], OnScreen[4]=Starting[8]
            //OffScreen[3]=Starting[7], OnScreen[3]=Starting[6]
            //OffScreen[2]=Starting[5], OnScreen[2]=Starting[4]
            //OffScreen[1]=Starting[3], OnScreen[1]=Starting[2]
            //OffScreen[0]=Starting[1], OnScreen[0]=Starting[0]

            ////slots 4-5, fill with best dfs
            //onCamera.Add(GetNextPlayerAvailable(dfs, mfs, fs);
            //offCamera.Add(GetNextPlayerAvailable(dfs, mfs, fs);
            //onCamera.Add(GetNextPlayerAvailable(dfs, mfs, fs);
            //offCamera.Add(GetNextPlayerAvailable(dfs, mfs, fs);

            //onCamera.Add(GetNextPlayerAvailable(fs, mfs, dfs);
            //offCamera.Add(GetNextPlayerAvailable(fs, mfs, dfs); ;
            ////slot 2, fill with best two mfs or best mf and next best f
            //bool midfielderaddedInOneSlot = false;

            //// because rearlier removal, fs[0] is now the next forward
            //if (fs.Count == 0)
            //{
            //    onCamera.Add(GetNextPlayerAvailable(mfs, fs, dfs); ;
            //    //mfs.Remove(onCamera[1]);
            //    midfielderaddedInOneSlot = true;
            //}
            //else
            //{
            //    if (midfielders[mfs[0]] >= forwards[fs[0]])
            //    {
            //        onCamera.Add(GetNextPlayerAvailable(fs, mfs, dfs);
            //        //mfs.Remove(onCamera[1]);
            //        midfielderaddedInOneSlot = true;
            //    }
            //    else
            //    {
            //        onCamera.Add(GetNextPlayerAvailable(fs, mfs, dfs);
            //        //fs.Remove(onCamera[1]);
            //    }
            //}

            //if (midfielderaddedInOneSlot)
            //{
            //    if (fs.Count == 0)
            //    {
            //        offCamera.Add(GetNextPlayerAvailable(mfs, fs, dfs); ;
            //        //mfs.Remove(offCamera[1]);
            //    }
            //    else
            //    {
            //        // slot 0 still contains the best remaining midfielder
            //        if (midfielders[mfs[0]] >= forwards[fs[0]] && formation != "FourThreeThree")
            //        {
            //            offCamera.Add(GetNextPlayerAvailable(fs, mfs, dfs);
            //            //mfs.Remove(offCamera[1]);
            //        }
            //        else
            //        {
            //            offCamera.Add(GetNextPlayerAvailable(fs, mfs, dfs);
            //            //fs.Remove(offCamera[1]);
            //        }
            //    }
            //}
            //else
            //{
            //    offCamera.Add(GetNextPlayerAvailable(mfs, fs, dfs);
            //    //mfs.Remove(offCamera[1]);
            //}

            ////slot 3, fill with remaining best mfs
            //onCamera.Add(GetNextPlayerAvailable(mfs, fs, dfs);
            //offCamera.Add(GetNextPlayerAvailable(mfs, fs, dfs);

            //foreach (Player p in cbs)
            //{
            //    benchAndReserves[p] = CenterBacks[p];
            //}
            //foreach (Player p in lbs)
            //{
            //    benchAndReserves[p] = LeftBacks[p];
            //}
            //foreach (Player p in rbs)
            //{
            //    benchAndReserves[p] = RightBacks[p];
            //}

            //foreach (Player p in cmfs)
            //{
            //    benchAndReserves[p] = CentralMidfielders[p];
            //}
            //foreach (Player p in cdms)
            //{
            //    benchAndReserves[p] = CentralDefendingMidfielders[p];
            //}
            //foreach (Player p in lmfs)
            //{
            //    benchAndReserves[p] = LeftMidfielders[p];
            //}
            //foreach (Player p in rmfs)
            //{
            //    benchAndReserves[p] = RightMidfielders[p];
            //}
            //foreach (Player p in cams)
            //{
            //    benchAndReserves[p] = CentralAttackingMidfielders[p];
            //}

            //foreach (Player p in sts)
            //{
            //    benchAndReserves[p] = Strikers[p];
            //}
            //foreach (Player p in lwfs)
            //{
            //    benchAndReserves[p] = LeftWingForwards[p];
            //}
            //foreach (Player p in rwfs)
            //{
            //    benchAndReserves[p] = RightWingForwards[p];
            //}

            int benchIndex = 0;

            //fill bench with goalkeeper, top remaining forward, midfielder, defender, if any are available, then fill the bench with the top players until all players are exhausted or no players remain
            for (int i = 0; i < availableGoalKeepers.Count; i++)
            {
                if (i == 0)
                    bench.Add(availableGoalKeepers[benchIndex++]);
                else
                    reserves.Add(availableGoalKeepers[i]);
            }

            Dictionary<Player, int> benchAndReserves = new Dictionary<Player, int>();
            foreach (Player p in availableOutfielders)
            {
                benchAndReserves.Add(p, p.trueRating.OverallWithPositionModifier);
            }

            List<Player> benchPlayersAndReserves = SortListByPlayerScore(benchAndReserves);

            while (benchPlayersAndReserves.Count > 0)
            {
                // modern teams get 9 subs.
                if (benchIndex > 9)
                    reserves.Add(benchPlayersAndReserves[0]);
                else
                    bench.Add(benchPlayersAndReserves[0]);
                benchPlayersAndReserves.Remove(benchPlayersAndReserves[0]);
                benchIndex++;
            }

            if (!TeamIsValid())
                throw new ArgumentException("It's not a valid roster!");
        }

        private Player GetNextPlayerAvailable(ref List<Player> outfielders, Enums.Positions pos)
        {
            Dictionary<Player, int> outfieldersScoreBasedOnPosition = new Dictionary<Player, int>();
            foreach (Player p in outfielders)
            {
                p.Position = pos;
                outfieldersScoreBasedOnPosition.Add(p, p.trueRating.OverallWithPositionModifier);
            }
            outfielders = SortListByPlayerScore(outfieldersScoreBasedOnPosition);
            Player outfielder = outfielders[0];
            outfielders.Remove(outfielder);

            foreach (Player p in outfielders)
            {
                // player is still available so we assume he's a bench player or reserve and therefore should have his
                // preferred position listed
                p.Position = p.PreferredPosition;
            }

            return outfielder;

        }

        //private Player GetNextPlayerAvailable(List<Player> priority, List<Player> secondary, List<Player> tertiary,
        //    List<Player> fourthTier, List<Player> fifthTier, List<Player> sixTier, List<Player> sevenTier,
        //    List<Player> eightTier, List<Player> ninthTier, List<Player> tenthTier, List<Player> elevenTier)
        //{
        //    Player p = null;
        //    if (priority.Count > 0)
        //    {
        //        p = priority[0];
        //        priority.Remove(p);
        //    }
        //    else if (secondary.Count > 0)
        //    {
        //        p = secondary[0];
        //        secondary.Remove(p);
        //    }
        //    else if (tertiary.Count > 0)
        //    {
        //        p = tertiary[0];
        //        tertiary.Remove(p);
        //    }
        //    else if (fourthTier.Count > 0)
        //    {
        //        p = fourthTier[0];
        //        fourthTier.Remove(p);
        //    }
        //    else if (fifthTier.Count > 0)
        //    {
        //        p = fifthTier[0];
        //        fifthTier.Remove(p);
        //    }
        //    else if (sixTier.Count > 0)
        //    {
        //        p = sixTier[0];
        //        sixTier.Remove(p);
        //    }
        //    else if (sevenTier.Count > 0)
        //    {
        //        p = sevenTier[0];
        //        sevenTier.Remove(p);
        //    }
        //    else if (eightTier.Count > 0)
        //    {
        //        p = eightTier[0];
        //        eightTier.Remove(p);
        //    }
        //    else if (ninthTier.Count > 0)
        //    {
        //        p = ninthTier[0];
        //        ninthTier.Remove(p);
        //    }
        //    else if (tenthTier.Count > 0)
        //    {
        //        p = tenthTier[0];
        //        tenthTier.Remove(p);
        //    }
        //    else if (elevenTier.Count > 0)
        //    {
        //        p = elevenTier[0];
        //        elevenTier.Remove(p);
        //    }
        //    else
        //    {
        //        p = new Player(-1, "Generic Reserve");
        //        p.InitializePlayerStats(LeagueName, Name);
        //        p.trueRating.shooting = 0;
        //        p.trueRating.passing = 0;
        //    }
        //    return p;
        //}

        public bool TeamIsValid()
        {
            //if (!DetermineIfAPlayerInArrayIsInvalid(onCamera))
            //    return false;
            //if (!DetermineIfAPlayerInArrayIsInvalid(offCamera))
            //    return false;
            //if (!PlayerIsValid(this.goalKeeper))
            //    return false;
            //foreach (Player p in completeRoster)
            //{
            //    int countNumberOfApperances = 0;
            //    if (onCamera.Contains(p))
            //        countNumberOfApperances++;
            //    if (offCamera.Contains(p))
            //        countNumberOfApperances++;
            //    if (bench.Contains(p))
            //        countNumberOfApperances++;
            //    if (reserves.Contains(p))
            //        countNumberOfApperances++;
            //    if (this.goalKeeper == p)
            //        countNumberOfApperances++;
            //    if (countNumberOfApperances != 1)
            //        return false;
            //}
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
}
