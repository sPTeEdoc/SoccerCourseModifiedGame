using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunnyOldGame
{
    public static class Enums
    {
        public enum InjurySeverity
        {
            Minor = 0, Moderate = 1, Major = 2, Serious = 3, CareerThreatening = 4
        }

        public enum Positions
        {
            Goalkeeper,
            CentralAttackingMidfielder,
            CenterBack,
            CentralDefendingMidfielder,
            CentralMidfielder,
            LeftBack,
            LeftMidfielder,
            LeftWingForward,
            RightBack,
            RightMidfielder,
            RightWingForward,
            Striker
        }

        public static string ConvertPositionToString(Enums.Positions pos)
        {
            switch (pos)
            {
                case Positions.Striker:
                    return "ST";
                case Positions.LeftWingForward:
                    return "LWF";
                case Positions.RightWingForward:
                    return "RWF";
                case Positions.CentralAttackingMidfielder:
                    return "CAM";
                case Positions.CentralDefendingMidfielder:
                    return "CDM";
                case Positions.CentralMidfielder:
                    return "CM";
                case Positions.LeftMidfielder:
                    return "LM";
                case Positions.RightMidfielder:
                    return "RM";
                case Positions.CenterBack:
                    return "CB";
                case Positions.RightBack:
                    return "RB";
                case Positions.Goalkeeper:
                    return "GK";
                default:
                    return "CM";
            }
        }

        public enum Ability
        {
            BelowAverage = -1,
            Average = 0,
            AboveAverage = 1,
            Amazing = 2
        }

        public enum Characteristic
        {
            pace = 1,
            dribbling = 2,
            isStrong = 3,
            isHard = 4,
            passing = 5,
            agility = 6
        }

        public enum RefAttitude
        {
            Friendly = 1,
            Neutral = 2,
            Hostile = 3
        }

        public enum SpecialShot
        {
            Header = 0,
            FreeKick = 1
        }

        public enum WeatherIssue
        {
            None
            , Light
            , Moderate
            , Heavy
        }

        public enum TierDifference
        {
            None
            , HomeSuperior
            , AwaySuperior
        }

        public enum YellowCardRegulations
        {
            EPL
        }

        public enum YouthAcademyTier
        {
            Basic,
            Average,
            Good,
            Excellent,
            Elite
        }

        public enum AttributeCategory
        {
            Physical,
            Technical,
            Mental
        }

        public enum TrainingCategory
        {
            // Outfield Categories
            Attacking,       // Focuses on Finishing, Shot Power, Volleys, Penalties
            Defending,       // Focuses on Defensive Awareness, Standing Tackle, Sliding Tackle, Interceptions, Heading Accuracy (defensive aspects)
            Fitness,         // Focuses on Acceleration, Sprint Speed, Stamina, Strength, Jumping, Agility, Balance
            Technical,       // Focuses on Dribbling, Ball Control, Short Pass, Long Pass, Crossing, Curve, Free Kicks
            Mental,          // Focuses on Composure, Vision, Positioning (outfield), Aggression, Reactions

            // Goalkeeper Category
            Goalkeeping,     // Focuses on GKDiving, GKHandling, GKKicking, GKReflexes, GKPositioning (GK), GKAwareness (GK)

            // General/Tactical (could influence overall team cohesion or tactical understanding)
            Tactical,
            YouthDevelopment // Could be a general skill that boosts all youth player gains
        }

        public enum TrainingIntensity
        {
            Light,
            Normal,
            Heavy,
            VeryHeavy
        }

        public enum PlayerDailyActivity
        {
            Training_Light,
            Training_Normal,
            Training_Heavy,
            Training_VeryHeavy,
            Match, // If a player plays a match, their fitness drain and injury risk is handled differently
            Rest,
            Recovery // For injured players who aren't playing or heavily training
        }

        // Define this enum outside your Game class, perhaps in an Enums.cs file
        public enum PitchZone
        {
            None,
            // Defensive Third (for the team whose goal is in this half)
            HomeDefense_LeftFlank,
            HomeDefense_Center,
            HomeDefense_RightFlank,

            // Midfield
            Midfield_LeftFlank,
            Midfield_Center,
            Midfield_RightFlank,

            // Attacking Third (for the team whose goal is in the *other* half)
            AwayAttack_LeftFlank,  // Attacking team's perspective (i.e., this is near opponent's goal)
            AwayAttack_Center,
            AwayAttack_RightFlank,

            // Specific goal-related areas
            PenaltyArea_Home,       // Inside the penalty box near the home team's goal
            PenaltyArea_Away,       // Inside the penalty box near the away team's goal
            GoalkeeperArea_Home,
            GoalkeeperArea_Away,

            // Set-piece specific zones
            Corner_HomeLeft,
            Corner_HomeRight,
            Corner_AwayLeft,
            Corner_AwayRight,

            KickOffCircle, // The very center of the pitch for kick-offs

            DefensivePenaltyArea_LeftNearPost,
            DefensivePenaltyArea_RightNearPost,
            DefensivePenaltyArea_Central,
            DefensivePenaltyArea_LeftFarPost,
            DefensivePenaltyArea_RightFarPost,
            Defensive_EdgeOfBox, // Area just outside the defending team's penalty box
            _ZoneMax
        }

        public enum SetPieceRoutineType
        {
            None,
            Corner_ShortToEdge,         // Short corner, then pass to player at edge of box
            Corner_NearPostRun,         // Cross to near post, player makes run
            Corner_FarPostCross,        // Cross to far post, target player
            Corner_CentralHeader,       // Cross to central penalty area
            FreeKick_DirectShot,        // Attempt shot on goal
            FreeKick_CrossIntoBox,      // Cross into penalty box
            FreeKick_LayOffAndShot,     // Lay off ball for another player to shoot
            FreeKick_ShortPassAndMove,  // Short pass, then movement for open play
            // Add more as needed for variety
        }

        //public String ConvertPitchZoneEnumToString(Enums.PitchZone pz)
        //{
        //    switch (pz)
        //    {
        //        case PitchZone.AwayAttack_Center:
        //            return "Away Center";
        //        case PitchZone.AwayAttack_LeftFlank:
        //            return "Away Left Flank";
        //        case PitchZone.AwayAttack_RightFlank:
        //            return "Away Right Flank";
        //        case PitchZone.Corner_AwayRight:
        //            return "Corner Away Right";
        //        case PitchZone.Corner_AwayLeft:
        //            return "Corner Away Left";
        //        case PitchZone.Corner_HomeLeft:
        //            return "Corner Home Left";
        //        case PitchZone.Corner_HomeRight:
        //            return "Corner Home Left";
        //        case PitchZone.GoalkeeperArea_Away:
        //            return "Goalkeeper Area Away";
        //        case PitchZone.GoalkeeperArea_Home:
        //            return "Goalkeeper Area Away";
        //        case PitchZone.HomeDefense_Center:
        //            return "Home Center";
        //        case PitchZone.HomeDefense_LeftFlank:
        //            return "Home Left Flank";
        //        case PitchZone.HomeDefense_RightFlank:
        //            return "Home Right Flank";
        //        case PitchZone.KickOffCircle:
        //            return "KickOff Circle";
        //        case PitchZone.Midfield_Center:
        //            return "Midfield Center";
        //        case PitchZone.Midfield_LeftFlank:
        //            return "Midfield Left Flank";
        //        case PitchZone.Midfield_RightFlank:
        //            return "Midfield Right Flank";
        //        case PitchZone.PenaltyArea_Away:
        //            return "Penalty Area Away";
        //        case PitchZone.PenaltyArea_Home:
        //            return "Penalty Area Home";
        //        default:
        //            return "None";
        //    }
        //}

        public enum PassOutcome
        {
            Successful,
            Intercepted,
            OutOfBounds,
            Offside,
            LostPossession, // Receiver fails to control
            LooseBall,
            FoulInAerialContest
        }

        public enum DribbleOutcome
        {
            Successful,
            Tackled,
            LostBall,
            FoulCommitted // Add this new outcome
        }

        public enum FoulOutcome
        {
            NoFoul,
            Foul_FreeKick,         // Simple foul, free kick awarded
            Foul_YellowCard,       // Foul with a yellow card
            Foul_RedCard,          // Serious foul, red card (player sent off)
            Foul_Penalty           // Foul inside the penalty area, penalty awarded
        }

        public enum Formation // Your new Formation enum
        {
            FourThreeThree,    // 4-3-3
            ThreeFourTwoOne,   // 3-5-2-1
            ThreeFiveTwo,      // 3-5-2
            FourFourOneOne,    // 4-4-1-1
            FourFourTwo,       // 4-4-2
            FourTwoThreeOne,   // 4-2-3-1
            FourOneTwoOneTwo,  // 4-1-2-1-2
            ThreeFourthree,    // 3-4-3
            FourTwoOneThree,    // 4-2-1-3
            FiveThreeTwo       // 5-3-2
            // Add more formations as needed
        }

        public enum MatchState
        {
            // Pre-game / Setup states
            MatchSetup,      // Initial state before anything starts
            KickOff,         // When the ball is about to be kicked off (start of half, after goal)

            // In-play states
            InPlay_Possession, // A player has clear possession of the ball
            InPlay_LooseBall,  // The ball is loose, no one has clear possession
            InPlay_LooseBallHigh,
            FreeKick_Indirect,

            // Dead ball / Set piece states
            GoalScored,      // Immediately after a goal, before reset for kickoff
            Foul,            // A foul has occurred, determines what happens next (free kick, penalty)
            Offside,         // Offside call
            CornerKick,      // Ball out over goal line by defender
            FreeKick,        // General free kick (direct or indirect)
            PenaltyKick,     // Penalty shot
            ThrowIn,         // Ball out on sideline
            GoalKick,        // Ball out over goal line by attacker
            CrossAttempted,
            GoalkeeperRecycle,

            // End of game states
            Halftime,        // Between halves
            Fulltime,        // End of regulation time
            ExtraTime_FirstHalf, // Start of first half of extra time
            ExtraTime_SecondHalf, // Start of second half of extra time
            PenaltyShootout, // For penalty shootout stage
            MatchEnded,       // Final state after all play is concluded
            SubstitutionOpportunity // NEW: A state to process pending substitutions
        }

        public enum Half
        {
            FirstHalf,
            SecondHalf,
            ExtraTimeFirst,
            ExtraTimeSecond,
            PenaltyShootout
        }

        public enum PassType
        {
            Short,
            Long,
            ThroughBall,
            Cross,
            DirectFreeKick,
            GoalKick,
            GKRecycleKick
        }

        public enum GKCrossAction
        {
            None,       // Goalkeeper chooses not to intervene, stays on line
            Catch,      // Goalkeeper attempts to catch the cross
            Punch       // Goalkeeper attempts to punch the cross away
        }

        public enum CrossOutcome
        {
            CaughtByGK,         // Goalkeeper cleanly catches the ball
            PunchedClearByGK,   // Goalkeeper punches the ball clear and out of danger (e.g., out for a throw-in)
            PunchedToOpponent,  // Goalkeeper punches the ball, but it goes straight to an opponent
            CrossMissed,        // The cross itself was poor (e.g., out of bounds, too weak)
            HeaderAttempted,    // An attacking player successfully attempted a header (leading to a shot attempt)
            ShotAttempted,      // An attacking player successfully attempted a shot (non-header) from the cross
            Defended,           // A defender cleared or intercepted the cross
            GoalFromCross,      // A goal was scored directly or indirectly from the cross (e.g., header into goal)
            FreeKickAwarded,
            LooseBall,
            FoulByGK,
            AttackerReachedBall,
            OutOfPlay,
            IndirectFreeKickAwarded,
            LooseBallHigh
        }

        // In your Enums.cs
        public enum TeamTactic
        {
            Balanced,           // Default, no strong bias
            Defensive,          // Focus on defending, less attacking risk
            Attacking,          // More aggressive attacking, higher risk (can imply DirectPlay/CentralFocus)
            CounterAttack,      // Focus on quick breaks after defending
            PressHigh,          // Aggressive pressing to win ball high up the pitch (often combined with Attacking/CounterAttack)
            Possession,         // Corresponds to 'PossessionBased' in our discussion
            AllOutAttack,       // Even more extreme attacking
            ParkTheBus,         // Even more extreme defensive

            // New specific attacking biases, if you want them distinct from 'Attacking'
            DirectPlay,         // Focus on quick forward passes, runs in behind (could be a flavor of 'Attacking')
            WidePlay,           // Focus on using flanks, crosses (could be a flavor of 'Attacking')
            CentralFocus        // Focus on penetrating through the middle (could be a flavor of 'Attacking')
        }

        public enum ShotOutcome
        {
            Goal,
            Saved,
            Blocked, // If a defender blocks it before it reaches the goal/GK
            Miss,
            Post, // Hit post or crossbar
            Rebound, // New: Saved, but ball is loose in a dangerous area for an attacker
            FoulByGK
        }

        public enum FoulType
        {
            None,            // No foul committed
            Minor,           // Light contact, push, shirt pull (rarely carded, usually a free kick)
            Tactical,        // Intentional foul to break up play (often a yellow card)
            Reckless,        // Foul with disregard for opponent's safety (often a yellow card, could be red)
            SeriousFoulPlay, // Excessive force or brutality (red card)
            Professional,    // Denying a clear goal-scoring opportunity (DOGSO) - (red card unless specific conditions met, e.g., penalty)
            ViolentConduct,  // Off-ball aggression, fighting, spitting (red card)
            Handball,        // Illegally touching the ball with hand/arm
            Other,            // Catch-all for less common fouls
            AerialFoul,        // foul in the air.
            FoulByGK,          // GK commits foul.
            Obstruction,
            Holding,
            FoulByGKCharge
        }

        public enum CardType
        {
            None,
            Yellow,
            Red
        }

        public enum RefereeStrictness
        {
            Lenient, // Less likely to give fouls/cards
            Normal,  // Standard interpretation
            Strict   // More likely to give fouls/cards
        }

        public enum Flank { Left, Center, Right }

        public enum PlayerRole
        {
            None, // Default or unassigned
            // Strikers
            Poacher,          // Focuses on runs in behind, getting into the box
            TargetMan,        // Holds up play, wins aerial balls, links with midfield
            DeepLyingForward, // Drops deep to link play, can also run in behind
            FalseNine,        // Drops very deep, creates space, links midfield & attack

            // Wingers / Wide Forwards
            Winger,           // Stays wide, focuses on crosses, can cut inside
            InsideForward,    // Cuts inside from wide areas to shoot/pass
            WidePlaymaker,    // Operates from wide, links play, creates chances

            // Attacking Midfielders
            AdvancedPlaymaker, // Roams, dictates tempo, killer passes
            AttackingMidfielder, // More direct, focuses on scoring/assisting from central positions
            DefensiveMidfielder,

            // Central Midfielders (could also make offensive runs)
            BoxToBoxMidfielder, // Covers ground, contributes defensively and offensively
            CentralMidfielder, // General purpose, links defense and attack

            FullBack,
            CenterBack,
            Goalkeeper,
            Striker
            // Defensive roles might also have nuances, but let's focus on attacking for now.
            // ... add other roles as needed for your simulation ...
        }

        public enum PressingStyle
        {
            None,
            HighPress,     // Aggressive, press high up the pitch
            MidBlock,      // Press primarily in the midfield, less aggressive high up
            LowBlock,       // Rarely press high, focus on defending deep
            VeryLow,    // Rarely press, deep block
            Standard,   // Balanced press
            VeryHigh    // Constant, all-out press (Gegenpress)

        }

        public enum WeatherType
        {
            Sunny,
            Cloudy,
            LightRain,
            HeavyRain,
            Snow,
            Windy,
            Fog // You can expand this list as desired
        }

        public enum TackleType
        {
            Standing,
            Sliding
        }

        public enum TackleOutcome
        {
            Successful,
            UnsuccessfulBallWon, // Tackle failed, but ball was won through other means
            UnsuccessfulFoul,     // Tackle resulted in a foul
            UnsuccessfulNoFoul,    // Tackle failed, opponent kept ball, no foul
            Failed,
            Foul,
            BallOutOfBounds,
            OwnGoal,
            LooseBall
        }

        public enum InjuryType
        {
            None,
            MinorKnock,         // Very short-term, minimal impact (e.g., 1-3 days)
            Bruise,             // Short-term, slight impact (e.g., 3-7 days)
            MuscleStrain_Minor, // Moderate-term, moderate impact (e.g., 1-2 weeks)
            MuscleStrain_Major, // Longer-term, significant impact (e.g., 3-6 weeks)
            LigamentSprain_Minor, // Moderate-long term, significant impact (e.g., 4-8 weeks)
            LigamentSprain_Major, // Long-term, very significant impact (e.g., 2-4 months)
            Fracture_Minor,     // Long-term, very significant impact (e.g., 3-6 months)
            Fracture_Major,     // Very long-term, potentially career-threatening (e.g., 6-12 months+)
            Concussion,         // Variable, requires careful handling (e.g., 1-4 weeks)
            TornACL,            // Very long-term, often season-ending (e.g., 6-9 months)
            TwistedAnkle,       // Short-term, slight impact (e.g., 3-7 days)
            // Add more as desired (e.g., Tendonitis, TwistedAnkle, etc.)
        }

        // You'll need this enum to simplify the GetZoneForPlayerFlankAndThird method
        public enum PitchThird
        {
            Defensive,
            Midfield,
            Attacking
        }

        public enum ShotType
        {
            Normal,
            Volley,
            Penalty,
            // Add these:
            DirectFreeKick,
            Header
        }

        // You'll need this new enum for the outcome of the aerial contest:
        public enum AerialContestOutcome
        {
            Player1Wins,    // Contester1 (e.g., target of pass) wins
            Player2Wins,    // Contester2 (e.g., defender) wins
            LooseBall,
            FoulOccurred
        }

        public enum PersonalityType
        {
            Temperamental,
            Resilient,
            Leader,
            Ambitious
        }

        public enum DefensiveLineDepth
        {
            VeryDeep,   // Retreats very far, often into own penalty area
            Deep,       // Sits deep, just outside own penalty area
            Standard,   // Balances defending depth
            High,       // Pushes up towards the midfield line
            VeryHigh,    // Pushes very high, close to the half-way line, aggressive offside trap
            OffsideTrap // This implies a high line with specific behavior
        }

        public enum MarkingStyle
        {
            Zonal,      // Players primarily hold their zones, intercepting passes
            Mixed,      // Balances zonal coverage with man-marking of dangerous opponents
            ManToMan    // Players stick tightly to assigned opponents, even if pulled out of zone
        }

        public enum TacklingAggression
        {
            Cautious,
            Standard,
            Aggressive,
            VeryAggressive
        }

        public enum PressingTrigger
        {
            Conservative,   // Pressing is initiated only when the opponent is in a clearly dangerous or vulnerable situation
            Standard,       // Balances waiting for opportunities with proactive pressing
            Intense         // Players proactively close down opponents quickly, even if not immediately threatening
        }

        public enum SweeperKeeperStyle
        {
            Aggressive,
            Standard,
            Conservative
        }

        public enum SetPieceRoutine
        {
            None, // Default, implies standard dynamic movement
            NearPostRun,
            FarPostRun,
            LayOffShort, // For indirect free kicks (e.g., short pass from a wide free kick)
            CrowdTheBox, // For indirect free kicks (e.g., aiming for a congested penalty area)
            ShotOnGoal, // For direct free kicks (default for direct shots)
            ThroughBallRun,
            ShortCornerPlay
        }

        public enum SetPieceRole
        {
            None, // Default or unassigned player

            // --- Offensive Set Piece Roles ---
            SetPieceTaker,          // Takes the kick (corner, free kick, penalty)
            TargetMan,              // Primary aerial target in the box (high header/jumping)
            NearPostRunner,         // Makes a run towards the near post area
            FarPostThreat,          // Positions or runs towards the far post area
            ReboundAttacker,        // Positions for rebounds or loose balls outside/edge of the box (combines ReboundPlayer & EdgeOfBoxShooter)
            ShortOptionReceiver,    // Offers a short pass option near the taker (combines ShortOptionRunner & ShortPassReceiver)
            OffensiveBlocker,       // Blocks defenders to create space for teammates
            MidfieldSupport,        // Stays deeper in midfield to offer support or cover against counters (replaces Offensive ScreeningPlayer)
            BoxAttacker,            // A general role for players flooding the box without a specific assigned target role (can be a fallback)
            PenaltyTaker,

            // --- Defensive Set Piece Roles ---
            WallPlayer,             // Forms part of the defensive wall (for free kicks)
            PostBlocker,            // Stands on the goal line at the posts (Corrected typo: Bloker -> Blocker)
            ZonalMarker,            // Marks a specific zone in the penalty area
            ManMarker,              // Marks a specific opponent player
            ClearingDefender,       // Positions to clear danger within the box (similar to ClearancePlayer)
            CounterAttackOutlet,    // Positions high up the pitch to launch a quick counter-attack
            EdgeOfBoxSweeper,       // Positions just outside the box to clear loose balls or intercept (replaces EdgeOfBoxDefender)
            DeepDefender,           // Stays deep in defense, general cover (replaces GeneralDefender)
            GoalkeeperCover,        // Goalkeeper's specific role (I suggested this earlier, not sure if you had it)
            // You might also consider specific roles like "Presser" for attacking the short option receiver
        }

        public enum CornerDeliveryType
        {
            None,
            InswingerLofted,   // High, arcing, curves towards goal
            InswingerDriven,   // Hard, flat, curves towards goal
            OutswingerLofted,  // High, arcing, curves away from goal
            OutswingerDriven,  // Hard, flat, curves away from goal
            ShortPass           // Pass along the ground to a nearby player
            // 'Lofted' and 'Floated' are covered by 'Lofted' in the specific types.
            // 'Driven' is covered by 'Driven' in the specific types.
            // 'Short' is covered by 'ShortPass'.
        }

        public enum ConceptualSetPieceZone
        {
            None,
            OpponentPenaltyArea_General,    // Main area inside the box
            OpponentPenaltyArea_NearPost,   // Specific sub-area near the near post
            OpponentPenaltyArea_FarPost,    // Specific sub-area near the far post
            OpponentAttackingThird_CentralEdge, // Just outside the box, central
            OpponentAttackingThird_Flank,   // Wide area in the attacking third (for short corners, wide playmakers)
            // Add more as needed, always from the attacking perspective
            CornerFlag, // For the taker's starting position
            ShortCornerArea, // For the player receiving a short pass
            OpponentPenaltyArea_Central,
            EdgeOfBox,
            Midfield_Center
            // For players positioned outside the box for rebounds/shots
            // Add more as needed for different routine strategies
        }

        public enum Foot
        {
            Both, // Default or unknown
            Left,
            Right
        }

        public enum DefensiveStrategy
        {
            None,
            ZonalMarking,
            ManMarking,
            HybridMarking // Combination of zonal and man-marking
        }

        public enum FreeKickDeliveryType
        {
            None,                // Default or unassigned
            DirectShot,          // Attempting to score directly on goal (for direct free kicks)
            LoftedPassIntoBox,   // A high, arcing pass into the penalty area, often for headers
            DrivenPassIntoBox,   // A hard, flat pass into the penalty area, often to run onto or for volleys
            ShortPass,           // A simple, short pass to a nearby teammate for combination play
            ThroughBall,         // A penetrating pass that splits the defense for an attacker to run onto (more common for indirect F.K.s)
            Cross                // A general cross from a wide free kick position (similar to a corner, but from a free kick spot)
            // You could also add 'DummyRun' if you have routines that involve faking a kick
        }

        public enum SetPieceTargetType
        {
            None,                // Default or unassigned
            SpecificPlayer,      // The delivery is aimed at a particular player (e.g., the TargetMan)
            SpecificZone,        // The delivery is aimed at a general area on the pitch (e.g., the "NearPost" zone)
            BlindArea,           // The delivery is aimed at an empty space that a player is expected to run into (e.g., behind a defender)
            Goalkeeper,          // The delivery is aimed directly at the goalkeeper (e.g., to test them, force a spill, or create chaos)
            Goal,                // The delivery is a direct shot on goal (for penalties or direct free kicks)
            // You could consider 'ReboundArea' if you have a distinct strategy for where missed shots should fall
        }

        public enum SetPieceType
        {
            CornerKick,
            FreeKick,
            IndirectFreeKick,
            Penalty,
            ThrowIn,
            GoalKick
        }

        // Ensure you have this enum in your Enums.cs
        public enum ThrowInDeliveryType
        {
            None,
            ShortThrow,
            LongThrowIntoBox
        }

        public enum PitchHalfType
        {
            None,       // For zones that don't clearly belong to a single half (or for default/error)
            Home,       // Zones in the defending team's (Home team's goal) half
            Midfield,   // Central midfield zones
            Away        // Zones in the defending team's (Away team's goal) half
        }

        public enum DialogResult
        {
            Yes,
            No,
            None
        }
    }
}
