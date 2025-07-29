using FunnyOldGameRedux;
using FunnyOldGameRedux.NonGuiCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static FunnyOldGame.Enums;

namespace FunnyOldGame
{
    public class Player
    {
        //public string firstName;
        //public string lastName;
        public int ID;
        public string fullName;
        public PlayerRating trueRating;
        public PlayerRating potentialRating;
        public Enums.Positions position;
        public Enums.Positions Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
                if (this.trueRating == null)
                    this.CreatePlayerRatings();
                this.trueRating.currentPos = value;
            }
        }
        public int weakFoot;
        public int skillMoves;
        public Enums.Foot preferredFoot;
        public string height;
        public string weight;
        public List<Enums.Positions> secondPos;
        public int age;
        public string nation;
        public string playStyle;
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
        public Team Team;
        public Enums.Positions PreferredPosition;
        public int PotentialRating {get; set; }
        public bool IsWallPlayerForCurrentSetPiece { get; set; }

        /// <summary>
        /// Represents the player's current fitness level (0-100%).
        /// 100 means fully fit, 0 means completely fatigued/exhausted.
        /// </summary>
        public int CurrentFitness { get; set; }

        /// <summary>
        /// Represents a player's inherent tendency to get injured (1-20 scale).
        /// Lower means more durable, higher means more injury prone.
        /// </summary>
        public int InjuryResistance { get; set; }

        // --- UPDATED: Reference to the Contract Class ---
        /// <summary>
        /// The player's current contract details. Can be null if the player is a free agent without a contract.
        /// </summary>
        public Contract CurrentContract { get; set; }
        public int Morale { get; set; }

        /// <summary>
        /// A list of active injuries the player currently has.
        /// A player can potentially have multiple minor injuries.
        /// </summary>
        public List<Injury> ActiveInjuries { get; set; }

        public double CurrentCondition { get; private set; } // Current condition (0.0 to 100.0) // RENAMED
        private const double MAX_CONDITION = 100.0;          // RENAMED

        public bool IsSubstituted { get; set; } // Flag to indicate if player has been subbed off
        //public int InjuryDurationMinutes { get; private set; } // NEW: How long player is out (e.g., for season mode)

        /// <summary>
        /// Convenience property to quickly check if the player is currently injured.
        /// </summary>
        public bool IsInjured
        {
            get {
                foreach (Injury i in ActiveInjuries)
                {
                    if (i.InjurySeverity > InjurySeverity.Moderate)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        // You might also want a history of past injuries for statistics/player narrative
        public List<Injury> InjuryHistory { get; set; }

        public int yellowCards = 0; // For current match
        public bool isSentOff = false; // For current match

        public int totalSeasonYellowCards = 0; // Total for season
        public int totalSeasonRedCards = 0; // Total for season
        public int redCardSuspensionMatches = 0; // Red card ban countdown
        public bool isSuspendedForNextMatch = false; // Yellow card accumulation ban

        private Enums.PitchZone m_CurrentZone = PitchZone._ZoneMax;

        public Enums.PitchZone CurrentZone { get {
                return m_CurrentZone;
        } set {
            if (value == Enums.PitchZone._ZoneMax)
            {
                int x = 0;
            }
            m_CurrentZone = value;
        }
        }

        public Player MarkedOpponent { get; set; } // The opponent this player is assigned to mark. Can be null.

        public Enums.PlayerRole Role { get; set; }

        /// <summary>
        /// The PitchZone that the player is currently aiming to move towards,
        /// especially relevant during set-pieces or when evaluating potential runs.
        /// This is a 'target' zone, and the player's actual CurrentZone will update
        /// to this target once the movement is executed.
        /// </summary>
        public Enums.PitchZone targetSetPieceZone { get; set; }

        public Enums.SetPieceRole OffensiveSetPieceRole { get; set; }
        public Enums.SetPieceRole DefensiveSetPieceRole { get; set; }
        public Enums.PitchZone SimulatedSetPieceZone { get; set; } // = Enums.PitchZone.None;

        public bool _newInjury = true;

        public bool _hasMinorInjury = false;
        public bool _hasMajorInjury = false;
        public bool _hasModerateInjury = false;
        public bool _hasSeriousInjury = false;
        public bool _hasCareerThreateningInjury = false;

        public bool IsAvailable
        {
            get
            {
                return !this.isSentOff && this.redCardSuspensionMatches == 0 && !this.isSuspendedForNextMatch
                    && !this.IsSubstituted && !this.IsInjured;
            }
        }

        private void SetUpInjuries()
        {
            if (_newInjury)
            {
                foreach (Injury injury in this.ActiveInjuries)
                {
                    switch (injury.InjurySeverity)
                    {
                        case InjurySeverity.Minor:
                            _hasMinorInjury = true;
                            break;
                        case InjurySeverity.Moderate:
                            _hasModerateInjury = true;
                            break;
                        case InjurySeverity.Major:
                            _hasMajorInjury = true;
                            break;
                        case InjurySeverity.Serious:
                            _hasSeriousInjury = true;
                            break;
                        case InjurySeverity.CareerThreatening:
                            _hasCareerThreateningInjury = true;
                            break;
                    }
                }
                _newInjury = false;
            }
        }

        public void ResetInjuryStatus()
        {
            _newInjury = true;
            // so that the ratings my update
            this.trueRating.SetInjuryModifiers(this.ActiveInjuries);
        }

        public bool HasMinorInjury
        {
            get
            {
                if (_newInjury)
                    SetUpInjuries();
                return _hasMinorInjury;
            }
        }
        
        public bool HasModerateInjury
        {
            get
            {
                if (_newInjury)
                    SetUpInjuries();
                return _hasModerateInjury;
            }
        }

        public bool HasMajorInjury
        {
            get
            {
                if (_newInjury)
                    SetUpInjuries();
                return _hasMajorInjury;
            }
        }

        public bool HasSeriousInjury
        {
            get
            {
                if (_newInjury)
                    SetUpInjuries();
                return _hasSeriousInjury;
            }
        }

        public bool HasCareerThreateningInjury
        {
            get
            {
                if (_newInjury)
                    SetUpInjuries();
                return _hasCareerThreateningInjury;
            }
        }

        public bool StretcheredOffDueToInjury
        {
            get
            {
                return HasSeriousInjury || HasCareerThreateningInjury;
            }
        }

        public bool AvailableOption
        {
            get
            {
                return StretcheredOffDueToInjury || (!this.isSentOff && this.redCardSuspensionMatches == 0 && !this.isSuspendedForNextMatch && !this.IsSubstituted);
            }
        }

        public string teamName 
        {
            get
            {
                return this.Team.Name;
            }
        }

        public Player(int id, string fullName)
        {
            //this.firstName = firstName;
            //this.lastName = lastName;
            this.ID = id;
            this.fullName = fullName;
            this.weakFoot = 0;
            this.skillMoves = 0;
            this.preferredFoot = Enums.Foot.Right;
            this.height = "176cm / 5'9";
            this.weight = "73kg / 161lb";
            this.age = 25;
            this.nation = "Parts Unknown";
            this.playStyle = "";
            this.number = 0;
            this.gamesOutDueToInjury = 0;
            this.gamesOutDueToSuspension = 0;

            isInjured = false;
            gameStats = new GameStats();
            this.LeagueTeamSeasonStats = new Dictionary<string, Dictionary<string, GameStats>>();

            PotentialRating = -1;

            CurrentContract = new Contract(3, 1500, 0);
            Morale = 70;
            CurrentFitness = 100;
            //InjuryResistance = Math.Max(1, Math.Min(10, 20)); // Clamp injury proneness
            InjuryResistance = 7; // Make Everybody tough.

            // Initialize new lists
            ActiveInjuries = new List<Injury>();
            InjuryHistory = new List<Injury>();

            CurrentCondition = MAX_CONDITION; // Players start with full condition // RENAMED
            IsSubstituted = false;
            //IsInjured = false;
        }

        // --- Update Player Constructor ---
        // Make sure to initialize the new lists in your constructor:
        public Player(string name, int overall, int age, int potential,
                      double contractYears, double weeklyWage, double signingBonus = 0,
                      int initialMorale = 70, int initialFitness = 100, int injuryProneness = 4) // Added initialFitness, injuryProneness
        {
            this.fullName = name;
            this.weakFoot = 0;
            this.skillMoves = 0;
            this.preferredFoot = Enums.Foot.Right;
            this.height = "176cm / 5'9";
            this.weight = "73kg / 161lb";
            this.age = 25;
            this.nation = "Parts Unknown";
            this.playStyle = "";
            this.number = 0;
            this.gamesOutDueToInjury = 0;
            this.gamesOutDueToSuspension = 0;

            isInjured = false;
            gameStats = new GameStats();
            this.LeagueTeamSeasonStats = new Dictionary<string, Dictionary<string, GameStats>>();

            this.fullName = name;
            //OverallRating = overall;
            this.age = age;
            PotentialRating = potential;

            CurrentContract = new Contract(contractYears, weeklyWage, signingBonus);
            Morale = initialMorale;
            CurrentFitness = initialFitness;
            InjuryResistance = Math.Max(1, Math.Min(injuryProneness, 20)); // Clamp injury proneness

            // Initialize new lists
            ActiveInjuries = new List<Injury>();
            InjuryHistory = new List<Injury>();

            // ... (rest of your constructor logic for attributes etc.) ...
        }

         /// <summary>
        /// Decreases a player's condition based on the intensity of the action.
        /// </summary>
        /// <param name="intensity">A value indicating how much condition to drain (e.g., 0.1 for light, 1.0 for heavy).</param>
        public void DecreaseCondition(double intensity) // RENAMED
        {
            // Condition drains faster for players with lower natural condition rating
            double conditionDrain = intensity * (MAX_CONDITION - trueRating.Stamina) / 50.0; // Adjust divisor for desired impact // RENAMED
            CurrentCondition = Math.Max(0, CurrentCondition - conditionDrain); // Condition cannot go below 0 // RENAMED
        }
        
        /// <summary>
        /// Slightly restores a player's condition (e.g., during a brief lull in play or half-time).
        /// </summary>
        /// <param name="amount">The amount of condition to restore.</param>
        public void RestoreCondition(double amount) // RENAMED
        {
            CurrentCondition = Math.Min(MAX_CONDITION, CurrentCondition + amount); // Condition cannot exceed max // RENAMED
        }

        /// <summary>
        /// Returns a modifier based on current condition, affecting effective ratings.
        /// </summary>
        public double GetConditionModifier() // RENAMED
        {
            // Condition affects performance. For example, 100 condition = 1.0 modifier, 0 condition = 0.5 modifier
            return 0.5 + (CurrentCondition / (MAX_CONDITION * 2.0)); // Ranges from 0.5 (at 0 condition) to 1.0 (at 100 condition) // RENAMED
        }

        public void CreatePlayerRatings()
        {
            if (this.potentialRating == null)
            {
                this.potentialRating = new PlayerRating(this.Position, this.PreferredPosition, this.secondPos, this.Morale);
                this.trueRating = new PlayerRating(this.Position, this.Position, this.secondPos, this.Morale);
            }
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
            player.Team = this.Team;
            player.trueRating = this.trueRating.Clone();
            player.potentialRating = this.potentialRating.Clone();
            player.Position = this.Position;
            player.secondPos = this.secondPos;
            player.PreferredPosition = this.PreferredPosition;
            player.weakFoot = this.weakFoot;
            player.skillMoves = this.skillMoves;
            player.preferredFoot = this.preferredFoot;
            player.height = this.height;
            player.weight = this.weight;
            player.secondPos = this.secondPos;
            player.age = this.age;
            player.nation = this.nation;
            player.playStyle = this.playStyle;
            player.number = this.number;
            player.gamesOutDueToInjury = 0;

            player.PotentialRating = this.PotentialRating;

            player.CurrentContract = this.CurrentContract; ;
            player.Morale = this.Morale;
            player.CurrentFitness = this.CurrentFitness;
            player.InjuryResistance = this.InjuryResistance; // Clamp injury proneness

            // Initialize new lists
            player.ActiveInjuries = this.ActiveInjuries;
            player.InjuryHistory = this.InjuryHistory;

            return player;
        }

         /// <summary>
    /// Awards a yellow card to the player. If it's their second yellow, they are sent off.
    /// </summary>
    public string GiveYellowCard()
    {
        if (isSentOff) return "how did this happen?"; // Cannot get another card if already sent off

        yellowCards++;
        if (yellowCards >= 2)
        {
            // If this is the second yellow, it becomes a red card
            SendOff();
            return this.fullName + " receives a second yellow card and is sent off!";
        }
        else
        {
            return this.fullName + " receives a yellow card. Total: " + yellowCards;
        }
    }

    /// <summary>
    /// Sends the player off (direct red card or second yellow).
    /// </summary>
    public string SendOff()
    {
        if (isSentOff) return "Again, how are we here."; // Already sent off

        isSentOff = true;
        // In a more complex simulation, you'd also remove them from the active lineup
        // for their team, affect team's active player count, and potentially trigger
        // a substitution if the rules allow. For now, just marking them as sent off is enough.
        return this.fullName + " has been sent off!";
    }

    public double ApplyPersonalityModifiersToMoraleChange(
        double baseMoraleChange,
        Enums.PersonalityType playerPersonalityType, // Or individual traits as booleans
        bool teamWon, bool teamLost) // Context like team result might matter for some traits
    {
        double modifiedMoraleChange = baseMoraleChange;

        // A common approach is to use 'if' statements for specific traits.
        // Each trait might have different impact values (e.g., a modifier between 0.5 and 1.5).

        switch (playerPersonalityType) // Assuming player has one dominant personality type
        {
            case Enums.PersonalityType.Resilient:
                // Resilient players are less affected by negative morale changes.
                if (modifiedMoraleChange < 0) // Only modify if morale is decreasing
                {
                    modifiedMoraleChange *= 0.75; // Reduce the morale loss by 25%
                }
                // They might also gain slightly less from positive changes, but usually
                // Resilient focuses on mitigating negatives.
                break;

            case Enums.PersonalityType.Temperamental:
                // Temperamental players experience more extreme swings.
                // Amplify both gains and losses.
                if (modifiedMoraleChange > 0)
                {
                    modifiedMoraleChange *= 1.25; // Gain 25% more morale
                }
                else if (modifiedMoraleChange < 0)
                {
                    modifiedMoraleChange *= 1.50; // Lose 50% more morale
                }
                break;

            case Enums.PersonalityType.Ambitious:
                // Ambitious players are highly motivated by success and demotivated by failure.
                if (teamWon)
                {
                    modifiedMoraleChange *= 1.15; // Gain more from wins
                }
                else if (teamLost)
                {
                    modifiedMoraleChange *= 1.30; // Lose more from losses
                }
                else // Draw
                {
                    // Draws might feel more like a loss to an ambitious player,
                    // potentially applying a small penalty even if others don't get one.
                    modifiedMoraleChange -= 1.0; // Small fixed penalty for a draw
                }
                break;

            case Enums.PersonalityType.Leader: // Example of a personality trait you might add
                // Leaders might get a small morale boost from team wins,
                // and possibly suffer less from individual errors if the team wins.
                if (teamWon)
                {
                    modifiedMoraleChange += 0.5; // Small fixed bonus for team success
                }
                break;

            // Add more cases for other personality types as needed...

            default:
                // No specific modification for default or unknown personality types
                break;
        }

        // You can also have independent traits (e.g., bool IsHotHeaded, bool IsConfident)
        // if a player can have multiple traits that are not mutually exclusive.
        /*
        if (player.HasTrait(Enums.PlayerTrait.HotHeaded) && baseMoraleChange < 0)
        {
            modifiedMoraleChange *= 1.2; // Hot-headed players get angrier faster
        }
        if (player.HasTrait(Enums.PlayerTrait.Confident) && baseMoraleChange > 0)
        {
            modifiedMoraleChange *= 1.1; // Confident players get an extra boost from good form
        }
        */

        return modifiedMoraleChange;
    }

        //private const int NumAttributes = 29;
        //private const int OverallRatingMin = 40; // Assuming youth players start at 40
        //private const int OverallRatingMax = 99;

        //private const int TargetAvgAttrMin = 40; // Average attribute for a 40 OR player
        //private const int TargetAvgAttrMax = 99; // Average attribute for a 99 OR player

        //public void GenerateInitialAttributes(int initialOverallRating)
        //{
        //    int tempInitialOveralRating = initialOverallRating;
        //    if (Position == Enums.Positions.Goalkeeper)
        //        initialOverallRating -= 30;
        //    // Initialize all attributes to a baseline
        //    //Dictionary<Enums.Positions, int> tempAttributes = new Dictionary<Enums.Positions, int>();
        //    int baselineAttributeValue = 10 + Dice.Instance.d6.Roll(); // Small random baseline (e.g., 10-16)

        //    int[] attributeWeights = GenerateWeightedAttributeScores(this.Position, this.PreferredPosition, this.secondPos);
        //    if (PreferredPosition == Enums.Positions.Goalkeeper)
        //        // we are merely calculating how he'd be as an outfielder as all stats are used for GKs
        //        attributeWeights = GenerateWeightedAttributeScores(Enums.Positions.CenterBack, Enums.Positions.CenterBack, this.secondPos);
        //    int[] posScores = new int[attributeWeights.Length];

        //    // Calculate initial total points used by baselines
        //    int currentTotalPoints = baselineAttributeValue * NumAttributes;

        //    // Calculate target total points based on initialOverallRating (from Step 2)
        //    double targetAvgAttribute = TargetAvgAttrMin +
        //                                (TargetAvgAttrMax - TargetAvgAttrMin) * ((double)initialOverallRating - OverallRatingMin) / (OverallRatingMax - OverallRatingMin);
        //    int targetTotalPoints = (int)(targetAvgAttribute * NumAttributes);
        //    int minScores = 40;

        //    for (int i = 0; i < posScores.Length; i++)
        //    {
        //        posScores[i] += minScores;
        //        currentTotalPoints += minScores;
        //    }

        //    // Points remaining to distribute
        //    int pointsToDistribute = targetTotalPoints - currentTotalPoints;

        //    // --- Iterative Distribution ---
        //    int numPasses = 1; // Number of passes for distribution. More passes = smoother distribution.

        //    // Calculate total weights for this position to determine proportion
        //    int totalWeights = 0;

        //    for (int i = 0; i < attributeWeights.Length; i++)
        //    {
        //        totalWeights += attributeWeights[i];
        //    }

        //    if (totalWeights == 0) totalWeights = 1; // Avoid division by zero if no weights defined

        //    for (int pass = 0; pass < numPasses; pass++)
        //    {
        //        // Calculate points for this pass (e.g., distribute 10% of remaining points each pass)
        //        int pointsForThisPass = (int)Math.Ceiling((double)pointsToDistribute / (numPasses - pass));
        //        if (pointsForThisPass <= 0) break; // No more points to distribute

        //        // Distribute points in this pass
        //        for (int i = 0; i < attributeWeights.Length; i++)
        //        {
        //            double proportion = (double)attributeWeights[i] / totalWeights;
        //            int pointsToAdd = (int)(pointsForThisPass * proportion);

        //            int dieThree = Dice.Instance.d3.Roll();
        //            if (dieThree == 3)
        //                dieThree = -1;

        //            // Add small random variance to each attribute in each pass
        //            pointsToAdd += --dieThree; // +/- 1 point variance

        //            // Apply age-specific biases (adjust pointsToAdd before adding to attribute)
        //            if (age <= 18) // Young player biases
        //            {
        //                //21 = composure, 14 = vision, 24 == defensive awareness
        //                if (i == 21 || i == 14 || i == 24)
        //                {
        //                    pointsToAdd = Math.Max(0, pointsToAdd - Dice.Instance.d4.Roll()); // Slightly lower mental stats
        //                }
        //                // 6 = acceleration, 7 = sprint, 19 = agility
        //                else if (i == 6 || i == 7 || i == 19)
        //                {
        //                    pointsToAdd = Math.Max(0, pointsToAdd + Dice.Instance.d3.Roll()); // Slightly higher raw physicals
        //                }
        //            }

        //            posScores[i] += pointsToAdd;
        //            currentTotalPoints += pointsToAdd; // Update current total

        //            // Clamp attribute values
        //            posScores[i] = Math.Max(1, Math.Min(posScores[i], 99));
        //        }
        //        pointsToDistribute = targetTotalPoints - currentTotalPoints; // Update remaining points
        //    }

        //    // --- Final Adjustment Pass (Optional but Recommended) ---
        //    // After distribution, it's possible the calculated overall rating (using the weighted formula)
        //    // might not be *exactly* the initialOverallRating you targeted. This pass nudges attributes.
        //    // This requires your OverallRating calculation method to be available.
        //    int start = 0;
        //    while (pointsToDistribute > 0)
        //    {
        //        int pointsToAdd = 1;
        //        if (attributeWeights[start] <= 3)
        //            pointsToAdd = 1;
        //        else
        //        {
        //            if (attributeWeights[start] <= 7)
        //            {
        //                pointsToAdd = 2;
        //            }
        //            else if (attributeWeights[start] <= 9)
        //            {
        //                pointsToAdd = 3;
        //            }
        //            else
        //            {
        //                pointsToAdd = 4;
        //            }
        //            int rollValue = Dice.Instance.d4.Roll();
        //            if (rollValue == 4)
        //                pointsToAdd++;
        //        }


        //        posScores[start] += pointsToAdd;
        //        currentTotalPoints += pointsToAdd; // Update current total

        //        // Clamp attribute values
        //        posScores[start] = Math.Max(1, Math.Min(posScores[start], 99));

        //        pointsToDistribute = targetTotalPoints - currentTotalPoints;
        //        start++;
        //        if (start >= attributeWeights.Length)
        //            start = 0;
        //    }

        //    // Assign generated attributes to the Player object
        //    // (Assuming Player class has properties for each attribute)
        //    trueRating.pace = posScores[0];
        //    trueRating.shooting = posScores[1];
        //    trueRating.passing = posScores[2];
        //    trueRating.dribbling = posScores[3];
        //    trueRating.defending = posScores[4];
        //    trueRating.physicality = posScores[5];
        //    trueRating.acceleration = posScores[6];
        //    trueRating.sprint = posScores[7];
        //    trueRating.positioning = posScores[8];
        //    trueRating.finishing = posScores[9];
        //    trueRating.shotPower = posScores[10];
        //    trueRating.longShot = posScores[11];
        //    trueRating.volleys = posScores[12];
        //    trueRating.penalties = posScores[13];
        //    trueRating.vision = posScores[14];
        //    trueRating.crossing = posScores[15];
        //    trueRating.shortPass = posScores[16];
        //    trueRating.longPass = posScores[17];
        //    trueRating.curve = posScores[18];
        //    trueRating.agility = posScores[19];
        //    trueRating.ballControl = posScores[20];
        //    trueRating.composure = posScores[21];
        //    trueRating.intercept = posScores[22];
        //    trueRating.header = posScores[23];
        //    trueRating.defenseAwareness = posScores[24];
        //    trueRating.standTackle = posScores[25];
        //    trueRating.slideTackle = posScores[26];
        //    trueRating.jumping = posScores[27];
        //    trueRating.stamina = posScores[28];
        //    trueRating.strength = posScores[29];

        //    int overall = this.trueRating.overall;

        //    if (PreferredPosition == Enums.Positions.Goalkeeper)
        //    {
        //        initialOverallRating = tempInitialOveralRating;
        //        attributeWeights = GenerateWeightedAttributeScores(Enums.Positions.Goalkeeper, Enums.Positions.Goalkeeper, this.secondPos);
        //        baselineAttributeValue = 10 + Dice.Instance.d10.Roll();
        //        posScores = new int[attributeWeights.Length];

        //        // Calculate initial total points used by baselines
        //        currentTotalPoints = baselineAttributeValue * 11;

        //        // Calculate target total points based on initialOverallRating (from Step 2)
        //        targetAvgAttribute = TargetAvgAttrMin +
        //                                    (TargetAvgAttrMax - TargetAvgAttrMin) * ((double)initialOverallRating - OverallRatingMin) / (OverallRatingMax - OverallRatingMin);
        //        targetTotalPoints = (int)(targetAvgAttribute * 11);

        //        // Points remaining to distribute
        //        pointsToDistribute = targetTotalPoints - currentTotalPoints;

        //        numPasses = 10; // Number of passes for distribution. More passes = smoother distribution.

        //        for (int pass = 0; pass < numPasses; pass++)
        //        {
        //            // Calculate points for this pass (e.g., distribute 10% of remaining points each pass)
        //            int pointsForThisPass = (int)Math.Ceiling((double)pointsToDistribute / (numPasses - pass));
        //            if (pointsForThisPass <= 0) break; // No more points to distribute

        //            // Distribute points in this pass
        //            for (int i = 0; i < attributeWeights.Length; i++)
        //            {
        //                double proportion = (double)attributeWeights[i] / totalWeights;
        //                int pointsToAdd = (int)(pointsForThisPass * proportion);

        //                int baseValue = initialOverallRating; // A starting point
        //                int randomDeviation = Dice.Instance.d20.Roll() - 10; // +/- 10 points
        //                pointsToAdd += Math.Max(1, Math.Min(baseValue + randomDeviation, 99));

        //                posScores[i] += pointsToAdd;
        //                currentTotalPoints += pointsToAdd; // Update current total

        //                // --- Step 3: Apply age bias for youth GKs (similar to outfield, but GK specific) ---
        //                if (age <= 18)
        //                {
        //                    // Young GKs might have lower GKComposures, better GKReflexes/GKDiving
        //                    if (i == 7)
        //                    {
        //                        // composure
        //                        pointsToAdd = Math.Max(1, pointsToAdd - (Dice.Instance.d5.Roll() + 2));
        //                    }
        //                    else if (i == 1)
        //                    {
        //                        // reflexes
        //                        pointsToAdd = Math.Min(99, pointsToAdd + Dice.Instance.d4.Roll());
        //                    }
        //                    else if (i == 3)
        //                    {
        //                        // diving
        //                        pointsToAdd = Math.Min(99, pointsToAdd + Dice.Instance.d4.Roll());
        //                    }
        //                }

        //                // Clamp attribute values
        //                posScores[i] = Math.Max(1, Math.Min(posScores[i], 99));
        //            }
        //            pointsToDistribute = targetTotalPoints - currentTotalPoints; // Update remaining points
        //        }

        //        trueRating.goalKeepingPositioning = posScores[0];
        //        trueRating.goalKeepingReflexes = posScores[1];
        //        trueRating.goalKeepingHandling = posScores[2];
        //        trueRating.goalkeepingDiving = posScores[3];
        //        trueRating.goalKeepingKicking = posScores[4];
        //        //physicality, agility, composure, jumping, stamina strength
        //        trueRating.physicality = posScores[5];
        //        trueRating.agility = posScores[6];
        //        trueRating.composure = posScores[7];
        //        trueRating.jumping = posScores[8];
        //        trueRating.stamina = posScores[9];
        //        trueRating.strength = posScores[10];
        //    }
        //}

        //private int[] GenerateWeightedAttributeScores(Enums.Positions pos, Enums.Positions preferredPos,
        //    List<Enums.Positions> secondaryPos)
        //{
        //    double[] stats = new double[TeamRepository.Instance.strikerWeightedScoreArray.Length - 1];
        //    int[] weights = new int[TeamRepository.Instance.strikerWeightedScoreArray.Length - 1];

        //    switch (preferredPos)
        //    {
        //        case Enums.Positions.Striker:
        //            stats = TeamRepository.Instance.strikerWeightedScoreArray;
        //            break;
        //        case Enums.Positions.LeftBack:
        //            stats = TeamRepository.Instance.lbWeightedScoreArray;
        //            break;
        //        case Enums.Positions.RightBack:
        //            stats = TeamRepository.Instance.rbWeightedScoreArray;
        //            break;
        //        case Enums.Positions.LeftWingForward:
        //            stats = TeamRepository.Instance.leftwingerWeightedScoreArray;
        //            break;
        //        case Enums.Positions.RightWingForward:
        //            stats = TeamRepository.Instance.rightwigerWeightedScoreArray;
        //            break;
        //        case Enums.Positions.CenterBack:
        //            stats = TeamRepository.Instance.cbWeightedScoreArray;
        //            break;
        //        case Enums.Positions.CentralAttackingMidfielder:
        //            stats = TeamRepository.Instance.camWeightedScoreArray;
        //            break;
        //        case Enums.Positions.CentralDefendingMidfielder:
        //            stats = TeamRepository.Instance.cdmWeightedScoreArray;
        //            break;
        //        case Enums.Positions.CentralMidfielder:
        //            stats = TeamRepository.Instance.cmWeightedScoreArray;
        //            break;
        //        case Enums.Positions.LeftMidfielder:
        //            stats = TeamRepository.Instance.lmWeightedScoreArray;
        //            break;
        //        case Enums.Positions.RightMidfielder:
        //            stats = TeamRepository.Instance.rmWeightedScoreArray;
        //            break;
        //        case Enums.Positions.Goalkeeper:
        //            stats = new double[TeamRepository.Instance.gkWeightedScoreArray.Length - 1];
        //            weights = new int[TeamRepository.Instance.gkWeightedScoreArray.Length - 1];
        //            stats = TeamRepository.Instance.gkWeightedScoreArray;
        //            break;
        //        default:
        //            stats = TeamRepository.Instance.strikerWeightedScoreArray;
        //            break;
        //    }

        //    Dictionary<PlayerRating, int> weightedPosRatings = new Dictionary<PlayerRating, int>();
        //    PlayerRating pr = new PlayerRating(pos, preferredPos, secondaryPos);
        //    for (int i = 0; i < weights.Length; i++)
        //    {
        //        weights[i] = 1;

        //        if (stats[i] > 21)
        //            weights[i] = 10;
        //        else if (stats[i] > 0.18)
        //            weights[i] = 9;
        //        else if (stats[i] > 0.15)
        //            weights[i] = 7;
        //        else if (stats[i] > 0.13)
        //            weights[i] = 6;
        //        else if (stats[i] > 0.10)
        //            weights[i] = 5;
        //        else if (stats[i] > 0.07)
        //            weights[i] = 4;
        //        else if (stats[i] > 0.04)
        //            weights[i] = 3;
        //        else if (stats[i] > 0.02)
        //            weights[i] = 2;
        //        //else if (stats[i] > 0.03)
        //        //    weights[i] = 2;
        //    }
        //    //for (int i = 0; i < weights.Length; i++)
        //    //{
        //    //    weights[i] = (int)Math.Round(stats[i] * 100, 0);
        //    //}
        //    //if (this.PreferredPosition == Enums.Positions.Striker)
        //    //{
        //    //    weights = TeamRepository.Instance.strikerGeneratePlayerWeightArray;
        //    //}
        //    //else
        //    //{
        //    //    for (int i = 0; i < weights.Length; i++)
        //    //    {
        //    //        weights[i] = 1;

        //    //        if (stats[i] > 0.14)
        //    //            weights[i] = 5;
        //    //        //else if (stats[i] > 0.21)
        //    //        //    weights[i] = 9;
        //    //        else if (stats[i] > 0.11)
        //    //            weights[i] = 4;
        //    //        //else if (stats[i] > 0.15)
        //    //        //    weights[i] = 7;
        //    //        else if (stats[i] > 0.08)
        //    //            weights[i] = 3;
        //    //        //else if (stats[i] > 0.09)
        //    //        //    weights[i] = 5;
        //    //        else if (stats[i] > 0.04)
        //    //            weights[i] = 2;
        //    //        //else if (stats[i] > 0.05)
        //    //        //    weights[i] = 3;
        //    //        //else if (stats[i] > 0.03)
        //    //        //    weights[i] = 2;
        //    //    }
        //    //}

        //    return weights;
        //}

        public void AgeOneYear()
        {
            age++;
            CurrentContract.DecrementYear(); // Decrement contract if player has one
            // ... then call PlayerDevelopmentManager.UpdateYearlyAttributes etc.
        }
    }
}
