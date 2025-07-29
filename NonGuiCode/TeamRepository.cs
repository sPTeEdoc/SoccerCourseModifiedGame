using FunnyOldGame;
using FunnyOldGameRedux.NonGuiCode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunnyOldGameRedux
{
    class TeamRepository
    {
        public double[] strikerWeightedScoreArray = { 0.04, 0.24, 0.02, 0.06, 0.01, 0.04, 0.05, 0.1, 0.08, 0.18, 0.12, 0.02, 0.06, 0.03, 0.05, 0.01, 0.005, 0.01, 0.02, 0.07, 0.08, 0.1, 0.01, 0.03, 0.01, 0.005, 0.005, 0.02, 0.04, 0.04, 1.537 };
        public double[] leftwingerWeightedScoreArray = { 0.06, 0.22, 0.08, 0.14, 0.02, 0.06, 0.08, 0.12, 0.1, 0.18, 0.14, 0.04, 0.06, 0.05, 0.12, 0.03, 0.02, 0.02, 0.04, 0.14, 0.16, 0.1, 0.01, 0.06, 0.03, 0.01, 0.01, 0.01, 0.05, 0.03, 2.19 };
        public double[] rightwigerWeightedScoreArray = { 0.05, 0.2, 0.04, 0.1, 0.02, 0.05, 0.06, 0.08, 0.09, 0.16, 0.12, 0.03, 0.05, 0.04, 0.1, 0.06, 0.02, 0.03, 0.04, 0.1, 0.12, 0.09, 0.01, 0.05, 0.03, 0.01, 0.01, 0.02, 0.05, 0.03, 1.86 };
        public double[] camWeightedScoreArray = { 0.03, 0.15, 0.1, 0.12, 0.05, 0.04, 0.04, 0.06, 0.08, 0.14, 0.1, 0.02, 0.06, 0.05, 0.12, 0.08, 0.04, 0.06, 0.05, 0.1, 0.14, 0.09, 0.03, 0.05, 0.04, 0.02, 0.02, 0.02, 0.06, 0.04, 2 };
        public double[] cdmWeightedScoreArray = { 0.02, 0.03, 0.12, 0.06, 0.17, 0.07, 0.03, 0.04, 0.1, 0.05, 0.06, 0.01, 0.03, 0.04, 0.1, 0.08, 0.06, 0.07, 0.04, 0.08, 0.1, 0.09, 0.12, 0.08, 0.2, 0.08, 0.06, 0.02, 0.06, 0.04, 2.11 };
        public double[] lmWeightedScoreArray = { 0.05, 0.12, 0.08, 0.14, 0.04, 0.06, 0.05, 0.07, 0.09, 0.1, 0.08, 0.03, 0.06, 0.05, 0.12, 0.09, 0.07, 0.08, 0.06, 0.12, 0.14, 0.19, 0.05, 0.06, 0.07, 0.04, 0.03, 0.02, 0.06, 0.03, 2.25 };
        public double[] rmWeightedScoreArray = { 0.04, 0.11, 0.09, 0.13, 0.05, 0.07, 0.04, 0.06, 0.08, 0.09, 0.07, 0.02, 0.05, 0.04, 0.11, 0.08, 0.06, 0.07, 0.05, 0.1, 0.12, 0.09, 0.06, 0.05, 0.06, 0.03, 0.02, 0.02, 0.06, 0.03, 1.95 };
        public double[] cmWeightedScoreArray = { 0.03, 0.08, 0.15, 0.1, 0.1, 0.08, 0.03, 0.05, 0.11, 0.07, 0.06, 0.02, 0.04, 0.05, 0.12, 0.1, 0.07, 0.08, 0.05, 0.09, 0.11, 0.1, 0.12, 0.06, 0.09, 0.05, 0.04, 0.02, 0.06, 0.03, 2.16 };
        public double[] cbWeightedScoreArray = { 0.02, 0.05, 0.04, 0.02, 0.25, 0.12, 0.02, 0.03, 0.14, 0.03, 0.04, 0.01, 0.05, 0.02, 0.06, 0.05, 0.03, 0.04, 0.02, 0.05, 0.06, 0.09, 0.2, 0.1, 0.25, 0.08, 0.06, 0.02, 0.05, 0.04, 2.04 };
        public double[] lbWeightedScoreArray = { 0.03, 0.04, 0.06, 0.02, 0.18, 0.08, 0.04, 0.05, 0.12, 0.03, 0.04, 0.01, 0.02, 0.02, 0.06, 0.05, 0.03, 0.04, 0.02, 0.05, 0.06, 0.08, 0.16, 0.07, 0.23, 0.06, 0.04, 0.02, 0.05, 0.04, 1.8 };
        public double[] rbWeightedScoreArray = { 0.05, 0.06, 0.08, 0.04, 0.2, 0.1, 0.05, 0.07, 0.13, 0.04, 0.05, 0.01, 0.03, 0.07, 0.01, 0.04, 0.06, 0.03, 0.06, 0.07, 0.09, 0.08, 0.18, 0.14, 0.23, 0.05, 0.06, 0.02, 0.05, 0.04, 2.19 };
        //GKs are different. Here's how we weight them: positioning, reflexes, handling, diving, kicking, physicality, agility, composure, jumping, stamina strength }
        public double[] gkWeightedScoreArray = { 0.3, 0.28, 0.27, 0.09, 0.01, 0.02, 0.08, 0.08, 0.02, 0.04, 0.06, 1.25 };

        public int[] strikerGeneratePlayerWeightArray = { 7, 10, 6, 7, 2, 5, 7, 8, 6, 10, 10, 3, 8, 8, 5, 5, 5, 6, 8, 8, 6, 8, 4, 7, 4, 2, 2, 7, 8, 4 };

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

        public Double MarketValue = 0.8;
        public int LatestID = 0;

        //List<Division> divisions;
        public TeamRepository()
        {
            //divisions = new List<Division>();
            //SaveData();
            //LoadTeams();
            //MarketValue = MarketDemand();
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

        // TO DO: Save data based on current information in the system, even if it is a player dat, rather than immediately
        // from the csv.
        public void SaveData()
        {
            PopulateLeagueCountry();
            PopulateLeagueTier();
            //PopulateFormations();

            MarketValue = MarketDemand();

            using (StreamReader readtext = new StreamReader(@"..\\..\\Data\\male_players.csv"))
            {
                string readText = readtext.ReadLine();
                using (StreamWriter writer = new StreamWriter(@"..\\..\\Data\\default_players.dat"))
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
                            player.CreatePlayerRatings();
                            //player.trueRating.overallWithPositionModifier = Int32.Parse(stringofdata[2]);
                            //TO DO: store contract information
                            writer.Write(((double)CalculateSalary(player) / (double)100).ToString() + ",");
                            writer.WriteLine(((double)CalculateSalary(player) / (double)100).ToString());
                        }
                        
                        rowNumber++;
                        readText = readtext.ReadLine();
                    }
                }
            }
        }

        public void DetermineRolesForPlayer(ref Player p)
        {
            p.Role = DetermineRoleFromPosition(p.Position);
            p.OffensiveSetPieceRole = DetermineOffensiveSetPieceRoleFromPosition(p.Position);
            p.DefensiveSetPieceRole = DetermineDefensiveSetPieceRoleFromPosition(p.Position);
        }

        // TO DO: Expand roles based on, well, string of roles given in Excel. Someday create random roles
        // for players, too.
        private Enums.SetPieceRole DetermineDefensiveSetPieceRoleFromPosition(Enums.Positions pos)
        {
            switch (pos)
            {
                case Enums.Positions.CenterBack:
                    return Enums.SetPieceRole.ManMarker;
                case Enums.Positions.CentralAttackingMidfielder:
                    return Enums.SetPieceRole.EdgeOfBoxSweeper;
                case Enums.Positions.CentralDefendingMidfielder:
                    return Enums.SetPieceRole.PostBlocker;
                case Enums.Positions.CentralMidfielder:
                    return Enums.SetPieceRole.DeepDefender;
                case Enums.Positions.Goalkeeper:
                    return Enums.SetPieceRole.GoalkeeperCover;
                case Enums.Positions.LeftBack:
                    return Enums.SetPieceRole.ClearingDefender;
                case Enums.Positions.RightBack:
                    return Enums.SetPieceRole.ZonalMarker;
                case Enums.Positions.RightMidfielder:
                    return Enums.SetPieceRole.WallPlayer;
                case Enums.Positions.LeftMidfielder:
                    return Enums.SetPieceRole.WallPlayer;
                case Enums.Positions.LeftWingForward:
                    return Enums.SetPieceRole.EdgeOfBoxSweeper;
                case Enums.Positions.RightWingForward:
                    return Enums.SetPieceRole.EdgeOfBoxSweeper;
                case Enums.Positions.Striker:
                    return Enums.SetPieceRole.CounterAttackOutlet;
                default:
                    return Enums.SetPieceRole.BoxAttacker;
            }
        }

        // TO DO: Expand roles based on, well, string of roles given in Excel. Someday create random roles
        // for players, too.
        private Enums.SetPieceRole DetermineOffensiveSetPieceRoleFromPosition(Enums.Positions pos)
        {
            switch (pos)
            {
                case Enums.Positions.CenterBack:
                    return Enums.SetPieceRole.MidfieldSupport;
                case Enums.Positions.CentralAttackingMidfielder:
                    return Enums.SetPieceRole.ReboundAttacker;
                case Enums.Positions.CentralDefendingMidfielder:
                    return Enums.SetPieceRole.OffensiveBlocker;
                case Enums.Positions.CentralMidfielder:
                    return Enums.SetPieceRole.SetPieceTaker;
                case Enums.Positions.Goalkeeper:
                    return Enums.SetPieceRole.None;
                case Enums.Positions.LeftBack:
                case Enums.Positions.RightBack:
                    return Enums.SetPieceRole.BoxAttacker;
                case Enums.Positions.RightMidfielder:
                    return Enums.SetPieceRole.ShortOptionReceiver;
                case Enums.Positions.LeftMidfielder:
                    return Enums.SetPieceRole.ShortOptionReceiver;
                case Enums.Positions.LeftWingForward:
                    return Enums.SetPieceRole.NearPostRunner;
                case Enums.Positions.RightWingForward:
                    return Enums.SetPieceRole.FarPostThreat;
                case Enums.Positions.Striker:
                    return Enums.SetPieceRole.TargetMan;
                default:
                    return Enums.SetPieceRole.BoxAttacker;
            }
        }

        // TO DO: Expand roles based on, well, string of roles given in Excel. Someday create random roles
        // for players, too.
        private Enums.PlayerRole DetermineRoleFromPosition(Enums.Positions pos)
        {
            switch (pos)
            {
                case Enums.Positions.CenterBack:
                    return Enums.PlayerRole.CenterBack;
                case Enums.Positions.CentralAttackingMidfielder:
                    return Enums.PlayerRole.AttackingMidfielder;
                case Enums.Positions.CentralDefendingMidfielder:
                    return Enums.PlayerRole.DefensiveMidfielder;
                case Enums.Positions.CentralMidfielder:
                    return Enums.PlayerRole.BoxToBoxMidfielder;
                case Enums.Positions.Goalkeeper:
                    return Enums.PlayerRole.Goalkeeper;
                case Enums.Positions.LeftBack:
                case Enums.Positions.RightBack:
                    return Enums.PlayerRole.FullBack;
                case Enums.Positions.RightMidfielder:
                    return Enums.PlayerRole.CentralMidfielder;
                case Enums.Positions.LeftMidfielder:
                    return Enums.PlayerRole.CentralMidfielder;
                case Enums.Positions.LeftWingForward:
                case Enums.Positions.RightWingForward:
                    return Enums.PlayerRole.Winger;
                case Enums.Positions.Striker:
                    return Enums.PlayerRole.Striker;
                default:
                    return Enums.PlayerRole.CentralMidfielder;
            }
        }

        public void LoadTeams()
        {
            using (StreamReader readtext = new StreamReader(@"..\\..\\Data\\default_players.dat"))
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
                        LatestID = rowNumber;
                        string[] stringofdata = readText.Split(',');
                        Player p = new Player(rowNumber, stringofdata[1]);

                        p.Position = GetPositionBasedOnstring(stringofdata[37]);
                        p.PreferredPosition = p.Position;

                        DetermineRolesForPlayer(ref p);
                        
                        string[] secondaryPositions = stringofdata[43].Split(new string[] { ";" }, StringSplitOptions.None);
                        List<Enums.Positions> secPosList = new List<Enums.Positions>();
                        foreach (string s in secondaryPositions)
                        {
                            secPosList.Add(GetPositionBasedOnstring(s));
                        }
                        p.secondPos = secPosList;

                        p.CreatePlayerRatings();

                        //Int32.TryParse(stringofdata[2], out p.trueRating.overall);
                        Int32.TryParse(stringofdata[3], out p.trueRating.pace);
                        Int32.TryParse(stringofdata[4], out p.trueRating.shooting);
                        Int32.TryParse(stringofdata[5], out p.trueRating.passing);
                        Int32.TryParse(stringofdata[6], out p.trueRating.defending);
                        Int32.TryParse(stringofdata[7], out p.trueRating.physicality);
                        Int32.TryParse(stringofdata[8], out p.trueRating.acceleration);
                        Int32.TryParse(stringofdata[9], out p.trueRating.sprint);
                        Int32.TryParse(stringofdata[10], out p.trueRating.positioning);
                        Int32.TryParse(stringofdata[11], out p.trueRating.finishing);
                        Int32.TryParse(stringofdata[12], out p.trueRating.shotPower);
                        Int32.TryParse(stringofdata[13], out p.trueRating.longShot);
                        Int32.TryParse(stringofdata[14], out p.trueRating.volleys);
                        Int32.TryParse(stringofdata[15], out p.trueRating.penalties);
                        Int32.TryParse(stringofdata[16], out p.trueRating.vision);
                        Int32.TryParse(stringofdata[17], out p.trueRating.crossing);

                        Int32.TryParse(stringofdata[18], out p.trueRating.freekicks);
                        Int32.TryParse(stringofdata[19], out p.trueRating.shortPass);
                        Int32.TryParse(stringofdata[20], out p.trueRating.longPass);
                        Int32.TryParse(stringofdata[21], out p.trueRating.curve);
                        Int32.TryParse(stringofdata[22], out p.trueRating.dribbling);
                        Int32.TryParse(stringofdata[23], out p.trueRating.agility);
                        Int32.TryParse(stringofdata[24], out p.trueRating.balance);
                        Int32.TryParse(stringofdata[25], out p.trueRating.reactionTime);
                        Int32.TryParse(stringofdata[26], out p.trueRating.ballControl);
                        Int32.TryParse(stringofdata[27], out p.trueRating.composure);
                        Int32.TryParse(stringofdata[28], out p.trueRating.intercept);
                        Int32.TryParse(stringofdata[29], out p.trueRating.header);
                        Int32.TryParse(stringofdata[30], out p.trueRating.defenseAwareness);
                        Int32.TryParse(stringofdata[31], out p.trueRating.standTackle);
                        Int32.TryParse(stringofdata[32], out p.trueRating.slideTackle);
                        Int32.TryParse(stringofdata[33], out p.trueRating.jumping);
                        Int32.TryParse(stringofdata[34], out p.trueRating.stamina);

                        Int32.TryParse(stringofdata[35], out p.trueRating.strength);
                        Int32.TryParse(stringofdata[36], out p.trueRating.aggression);
                        Int32.TryParse(stringofdata[38], out p.weakFoot);
                        Int32.TryParse(stringofdata[39], out p.skillMoves);
                        if (stringofdata[40] == "Right")
                            p.preferredFoot = Enums.Foot.Right;
                        else if (stringofdata[40] == "Left")
                            p.preferredFoot = Enums.Foot.Left;
                        else
                            p.preferredFoot = Enums.Foot.Both;
                        p.height = stringofdata[41];
                        p.weight = stringofdata[42];
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
                        Int32.TryParse(stringofdata[51], out p.trueRating.goalkeepingDiving);
                        Int32.TryParse(stringofdata[52], out p.trueRating.goalKeepingHandling);
                        Int32.TryParse(stringofdata[53], out p.trueRating.goalKeepingKicking);
                        Int32.TryParse(stringofdata[54], out p.trueRating.goalKeepingPositioning);
                        Int32.TryParse(stringofdata[55], out p.trueRating.goalKeepingReflexes);
                        Double.TryParse(stringofdata[56], out p.transferFee);
                        Double.TryParse(stringofdata[57], out p.salary);
                        //p.salary = CalculateSalary(p);
                        p.CurrentContract = ContractGenerator.GenerateContract(p, p.trueRating.overall, p.PotentialRating, p.age, p.trueRating.overall);
                        p.transferFee = CalculateTransferFee(p, p.salary);
                        p.PotentialRating = p.trueRating.overall;

                        theTeam.completeRoster.Add(p);
                        playerIDDict.Add(p.ID, p);
                        p.Team = theTeam;
                        
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

                        //if (formations.ContainsKey(theTeam.Name))
                        //    theTeam.formation = formations[theTeam.Name];
                        //else
                        //    theTeam.formation = "4-3-3";
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
                            if (t.completeRoster.Count < 30)
                            {
                                while (t.completeRoster.Count < 30)
                                {
                                    double sumOfRatings = 0.0;
                                    foreach (Player p in t.completeRoster)
                                    {
                                        sumOfRatings += p.trueRating.overall;
                                    }
                                    sumOfRatings /= (double)t.completeRoster.Count;
                                    int avgOveralls = (int)Math.Round(sumOfRatings, 0);

                                    FillWithGenericPlayers(t, ref rowNumber, avgOveralls);
                                }
                            }
                            t.ConfigureRoster();
                        }
                    }
                }
                LatestID = rowNumber;
            }
        }

        public Enums.Positions GetPositionBasedOnstring(string pos)
        {
            if (pos == "CAM")
                return Enums.Positions.CentralAttackingMidfielder;
            if (pos == "CB")
                return Enums.Positions.CenterBack;
            if (pos == "CDM")
                return Enums.Positions.CentralDefendingMidfielder;
            if (pos == "CM")
                return Enums.Positions.CentralMidfielder;
            if (pos == "GK")
                return Enums.Positions.Goalkeeper;
            if (pos == "LB")
                return Enums.Positions.LeftBack;
            if (pos == "LM")
                return Enums.Positions.LeftMidfielder;
            if (pos == "LW")
                return Enums.Positions.LeftWingForward;
            if (pos == "RB")
                return Enums.Positions.RightBack;
            if (pos == "RM")
                return Enums.Positions.RightMidfielder;
            if (pos == "RW")
                return Enums.Positions.RightWingForward;
            if (pos == "ST")
                return Enums.Positions.Striker;
            else
                return Enums.Positions.CentralMidfielder;
        }

        public PlayerRating DeterminePlayerPotentialStats(Player p)
        {
            if (p.age < 21)
            {
            }
            else if (p.age < 25)
            {
            }
            else if (p.age < 30)
            {
            }
            //else
            //{
                return p.trueRating.Clone();
            //}
        }

        public void PlayerTester()
        {
            Player p = new Player(-1, PlayerNameGenerator.Instance.GenerateRandomName());
            p.nation = PlayerNationGenerator.Instance.GenerateRandomNation();
            p.Position = Enums.Positions.Striker;
            p.PreferredPosition = Enums.Positions.Striker;
            p.CreatePlayerRatings();
            p.Team = new Team("X", "Y");
            //t.completeRoster.Add(p);
            //p.GenerateInitialAttributes(65);
            PlayerGenerator.GeneratePlayerWithQuality(p, 65);
            playerIDDict.Add(p.ID, p);
        }

        public Player GeneratePlayer(Team t, Enums.Positions pos, int id, int overallRating)
        {
            Player p = new Player(id, PlayerNameGenerator.Instance.GenerateRandomName());
            p.nation = PlayerNationGenerator.Instance.GenerateRandomNation();
            p.Position = pos;
            p.PreferredPosition = pos;
            DetermineRolesForPlayer(ref p);
            p.CreatePlayerRatings(); 
            p.Team = t;
            t.completeRoster.Add(p);
            p.preferredFoot = Dice.d100() < 20 ? Enums.Foot.Left : Enums.Foot.Right;
            PlayerGenerator.GeneratePlayerWithQuality(p, overallRating);
            playerIDDict.Add(p.ID, p);
            return p;
        }

        private void FillWithGenericPlayers(Team t, ref int id, int overallRating)
        {
            GeneratePlayer(t, Enums.Positions.CenterBack, ++id, overallRating);
            GeneratePlayer(t, Enums.Positions.LeftBack, ++id, overallRating);
            GeneratePlayer(t, Enums.Positions.RightBack, ++id, overallRating);
            GeneratePlayer(t, Enums.Positions.Striker, ++id, overallRating);
            GeneratePlayer(t, Enums.Positions.CentralAttackingMidfielder, ++id, overallRating);
            GeneratePlayer(t, Enums.Positions.CentralDefendingMidfielder, ++id, overallRating);
            GeneratePlayer(t, Enums.Positions.CentralMidfielder, ++id, overallRating);
            GeneratePlayer(t, Enums.Positions.LeftMidfielder, ++id, overallRating);
            GeneratePlayer(t, Enums.Positions.RightMidfielder, ++id, overallRating);
            GeneratePlayer(t, Enums.Positions.LeftWingForward, ++id, overallRating);
            GeneratePlayer(t, Enums.Positions.RightWingForward, ++id, overallRating);
            GeneratePlayer(t, Enums.Positions.Goalkeeper, ++id, overallRating);
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

        public double CalculateTransferFee(Player p, double salary)
        {
            double modifier = PositionMultiplier(p) + 0.1;
            double addition = 0.0;

            int d10Roll = Dice.d10();
            double d10Val = (double)d10Roll / 10.0;
            int overall = p.trueRating.overall;
            if (p.age < 22)
                overall = p.potentialRating.overall;

            if (overall > 90)
                addition = 1 + d10Roll;
            else if (overall > 80)
                addition = 0.9 + d10Roll;
            else if (overall > 70)
                addition = 0.5 + d10Roll;
            if (overall > 60)
            {
                addition = 0.3;
            }

            return (modifier + addition) * salary;
        }

        public double CalculateSalary(Player p)
        {
            //int score = p.trueRating.overall;
            double weeklySalary = 0;
            weeklySalary = (FindBaseSalary(p) + (p.trueRating.overall * WageMultiplier(p))) *
                (1 - ((p.age - PeakAge(p)) / AgeRange(p)) * PositionMultiplier(p) * MarketValue);

            double modifier = 2500;

            int totalDiceRolls = (Dice.d10() - 1) * 10000 + (Dice.d10() - 1) * 1000 +
                (Dice.d10() - 1) * 100 + (Dice.d10() - 1) * 10 + Dice.d10();

            modifier += totalDiceRolls;
            modifier /= 9100;

            weeklySalary *= modifier;

            //while (score > 0)
            //{
            //    salary += baseSalary + Dice.d100() + Dice.d100()
            //        + Dice.d100();
            //    score--;
            //}

            return weeklySalary;
        }

        public double MarketDemand()
        {
            double val = Dice.d6();
            return 0.7 + (val / 10.0);
        }

        public double WageMultiplier(Player p)
        {
            return 2000 * PositionMultiplier(p);
        }

        public int AgeRange(Player p)
        {
            if (p.Position == Enums.Positions.Striker)
            {
                return 9;
            }
            else if (IsForward(p.Position))
            {
                return 9;
            }
            else if (IsMidfielder(p.Position))
            {
                return 10;
            }
            else if (IsGoalKeeper(p.Position))
            {
                return 12;
            }
            else
            {
                return 11;
            }
        }

        public int PeakAge(Player p)
        {
            if (p.Position == Enums.Positions.Striker)
            {
                return 26;
            }
            else if (IsForward(p.Position))
            {
                return 27;
            }
            else if (IsMidfielder(p.Position))
            {
                return 27;
            }
            else if (IsGoalKeeper(p.Position))
            {
                return 26;
            }
            else
            {
                return 26;
            }
        }

        public double PositionMultiplier(Player p)
        {
            if (p.Position == Enums.Positions.Striker)
            {
                return 1.5;
            }
            else if (IsForward(p.Position))
            {
                return 1.3;
            }
            else if (IsMidfielder(p.Position))
            {
                return 1.2;
            }
            else if (IsGoalKeeper(p.Position))
            {
                return 1.1;
            }
            else
            {
                return 1.0;
            }
        }

        public double FindBaseSalary(Player p)
        {
            double positionBase = 10000;
            if (IsForward(p.Position))
                positionBase = 15000;
            else if (IsMidfielder(p.Position))
                positionBase = 12000;
            else if (IsGoalKeeper(p.Position))
                positionBase = 11000;

            if (p.trueRating.overall > 90)
                return positionBase += 100000;
            else if (p.trueRating.overall > 80)
                positionBase += 70000;
            else if (p.trueRating.overall > 70)
                positionBase += 50000;
            else if (p.trueRating.overall > 60)
                positionBase += 23000;
            else if (p.trueRating.overall > 50)
                positionBase += 15000;
            else
                positionBase += 3000;

            return positionBase;
        }

        public Boolean IsForward(Enums.Positions pos)
        {
            return pos == Enums.Positions.Striker || pos == Enums.Positions.RightWingForward || pos == Enums.Positions.LeftWingForward;
        }

        public Boolean IsMidfielder(Enums.Positions pos)
        {
            return pos == Enums.Positions.CentralAttackingMidfielder || pos == Enums.Positions.CentralDefendingMidfielder || pos == Enums.Positions.CentralMidfielder
                || pos == Enums.Positions.LeftMidfielder || pos == Enums.Positions.RightMidfielder;
        }

        public Boolean IsDefender(Enums.Positions pos)
        {
            return pos == Enums.Positions.LeftBack || pos == Enums.Positions.RightBack || pos == Enums.Positions.CenterBack;
        }

        public Boolean IsGoalKeeper(Enums.Positions pos)
        {
            return pos == Enums.Positions.Goalkeeper;
        }

        public void PopulateNewEuroTeams()
        {
            
            LeagueCountry uefacl = TeamRepository.Instance.countries[TeamRepository.Instance.countryIndex["UEFA Champions League"]];
            uefacl.leagues[0].teams = new List<Team>();
            foreach (Team t in UEFAChampionsLeagueNextSeason)
                uefacl.leagues[0].teams.Add(t);
            
            LeagueCountry uefael = TeamRepository.Instance.countries[TeamRepository.Instance.countryIndex["UEFA Europa League"]];
            uefael.leagues[0].teams = new List<Team>();
            foreach (Team t in UEFAChampionsLeagueNextSeason)
                uefael.leagues[0].teams.Add(t);
            
            UEFAChampionsLeagueNextSeason = new List<Team>();
            UEFAEuropaLeagueNextSeason = new List<Team>();
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
            formations.Add("AFC Bournemouth", "4-3-3");
            formations.Add("Arsenal", "4-3-3");
            formations.Add("Aston Villa", "4-3-3");
            formations.Add("Brentford", "3-5-2");
            formations.Add("Brighton", "4-3-3");
            formations.Add("Chelsea", "4-3-3");
            formations.Add("Crystal Palace", "4-3-3");
            formations.Add("Everton", "4-4-1-1");
            formations.Add("Fulham", "4-3-3");
            formations.Add("Ipswich", "4-3-3");
            formations.Add("Leicester City", "4-3-3");
            formations.Add("Liverpool", "4-3-3");
            formations.Add("Manchester City", "4-3-3");
            formations.Add("Manchester United", "4-3-3");
            formations.Add("Newcastle Utd", "4-3-3");
            formations.Add("Nott'm Forest", "4-3-3");
            formations.Add("Southampton", "3-5-2");
            formations.Add("West Ham", "4-4-2");
            formations.Add("Wolves", "3-4-2-1");

            // EFL Championship
            formations.Add("Luton Town", "3-4-2-1");
            formations.Add("Leeds United", "3-5-2");
            formations.Add("Plymouth Argyle", "3-5-2");
            formations.Add("Bristol City", "3-5-2");
            formations.Add("Cardiff City", "3-5-2");
            formations.Add("Burnley", "4-4-2");
            formations.Add("Derby County", "3-5-2");
            formations.Add("Norwich", "3-5-2");
            formations.Add("Middlesbrough", "3-5-2");
            formations.Add("Sheffield Utd", "3-5-2");
            formations.Add("Sunderland", "3-5-2");
            formations.Add("Millwall", "3-5-2");
            formations.Add("Swansea City", "3-5-2");
            formations.Add("Blackburn Rovers", "4-3-3");
            formations.Add("West Brom", "4-3-3");
            formations.Add("Hull City", "4-3-3");
            formations.Add("Sheffield Wed", "4-3-3");
            formations.Add("QPR", "4-3-3");
            formations.Add("Portsmouth", "4-3-3");
            formations.Add("Stoke City", "4-3-3");
            formations.Add("Oxford United", "3-4-2-1");
            formations.Add("Watford", "3-4-2-1");
            formations.Add("Coventry City", "3-4-3");
            formations.Add("Preston", "4141");

            // EFL League One
            formations.Add("Birmingham City", "3-5-2");
            formations.Add("Leyton Orient", "3-5-2");
            formations.Add("Rotherham Utd", "3-5-2");
            formations.Add("Mansfield Town", "3-5-2");
            formations.Add("Bolton", "3-5-2");
            formations.Add("Bristol Rovers", "3-5-2");
            formations.Add("Charlton Ath", "3-5-2");
            formations.Add("Wycombe", "4-3-3");
            formations.Add("Huddersfield", "4-3-3");
            formations.Add("Stockport", "4-3-3");
            formations.Add("Stevenage", "4-3-3");
            formations.Add("Shrewsbury", "4-3-3");
            formations.Add("Reading", "4-3-3");
            formations.Add("Crawley Town", "4-3-3");
            formations.Add("Northampton", "4-3-3");
            formations.Add("Wrexham", "4-3-3");
            formations.Add("Blackpool", "4-3-3");
            formations.Add("Peterborough", "4-3-3");
            formations.Add("Barnsley", "3-4-2-1");
            formations.Add("Burton Albion", "3-4-2-1");
            formations.Add("Wigan Athletic", "3-4-3");
            formations.Add("Cambridge Utd", "4-4-2");
            formations.Add("Exeter City", "4-4-2");

            // EFL League Two
            formations.Add("Harrogate Town", "3-5-2");
            formations.Add("Colchester", "3-5-2");
            formations.Add("MK Dons", "3-5-2");
            formations.Add("Tranmere Rovers", "3-5-2");
            formations.Add("Salford City", "3-5-2");
            formations.Add("Notts County", "3-5-2");
            formations.Add("Walsall", "3-5-2");
            formations.Add("AFC Wimbledon", "3-5-2");
            formations.Add("Doncaster", "3-5-2");
            formations.Add("Newport County", "3-5-2");
            formations.Add("Crewe Alexandra", "4-3-3");
            formations.Add("Grimsby Town", "4-3-3");
            formations.Add("Barrow", "4-3-3");
            formations.Add("Bradford City", "4-3-3");
            formations.Add("Port Vale", "4-3-3");
            formations.Add("Carlisle United", "4-3-3");
            formations.Add("Swindon Town", "4-3-3");
            formations.Add("Gillingham", "4-3-3");
            formations.Add("Fleetwood Town", "3-4-2-1");
            formations.Add("Chesterfield", "3-4-3");
            formations.Add("Morecambe", "3-4-3");
            formations.Add("Cheltenham Town", "4-4-2");
            formations.Add("Bromley FC", "4-4-2");
            formations.Add("Accrington", "4-1-2-1-2");
        }
    }


}
