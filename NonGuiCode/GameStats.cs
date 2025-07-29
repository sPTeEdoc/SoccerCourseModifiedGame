using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunnyOldGame
{
    public class GameStats
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
        public double MinutesWithBall
        {
            get
            {
                return (double)SecondsWithBall / 60;
            }
        }
        public int penaltyKickAttempts;
        public int penaltyKickGoals;
        public int cornerKicks;
        public int wins;
        public int losses;
        public int draws;
        public int matchesPlayed;

        // --- New Properties for Match Rating Calculation (Behind the scenes performance) ---
        // These are not necessarily displayed but are crucial for objective rating.
        public int SuccessfulTackles { get; set; }
        public int Interceptions { get; set; }
        public int Clearances { get; set; }
        public int Blocks { get; set; } // Shot blocks, pass blocks
        public int ChancesCreated { get; set; } // Key passes
        public int SuccessfulDribbles { get; set; }
        public int DuelsWon { get; set; } // Headers won, ground duels won
        public int PassAttempts { get; set; }
        public int SuccessfulPasses { get; set; }
        public int ErrorsLeadingToGoal { get; set; }
        public int Dispossessed { get; set; }  // Losing possession under pressure
        public double MinutesPlayed
        {
            get
            {
                return (double)SecondsPlayed / 60;
            }
        }

        public double SecondsPlayed { get; set; }
        public double SecondsWithBall { get; set; }
        // Add a reference to the player and their position for contextual scoring
        public Player PlayerReference { get; set; } // Reference back to the player object
        public int OwnGoals { get; set; }

        public void IncreaseSaves()
        {
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

        public void IncreasePassCount(bool successfulPass)
        {
            this.PassAttempts++;
            if (successfulPass)
                this.SuccessfulPasses++;
        }

        /// <summary>
        /// Calculates the final match rating for this player based on accumulated stats.
        /// Outputs a rating between 1 and 100.
        /// </summary>
        public void CalculateFinalMatchRating(Enums.Positions playerMatchPosition) // Pass player's position for contextual scoring
        {
            double ratingScore = 50.0; // Starting base rating out of 100

            // --- Positive Contributions ---
            ratingScore += goals * 25.0; // High impact
            ratingScore += assists * 15.0;
            ratingScore += penaltyKickGoals * 10.0; // Bonus for converting penalties
            ratingScore += m_saves * 5.0; // Base for saves

            ratingScore += SuccessfulTackles * 3.0;
            ratingScore += Interceptions * 3.0;
            ratingScore += Clearances * 1.5;
            ratingScore += Blocks * 2.0;
            ratingScore += ChancesCreated * 5.0;
            ratingScore += SuccessfulDribbles * 1.0;
            ratingScore += DuelsWon * 1.0;

            // Pass accuracy contribution
            if (PassAttempts > 0)
            {
                double passCompletionRate = (double)SuccessfulPasses / PassAttempts;
                ratingScore += passCompletionRate * 10.0; // Up to 10 points for good passing
            }

            // --- Negative Contributions ---
            ratingScore -= yellowCards * 5.0;
            ratingScore -= redCards * 20.0; // Significant penalty
            ratingScore -= fouls * 1.0;
            ratingScore -= offsides * 0.5;
            ratingScore -= ErrorsLeadingToGoal * 25.0; // Huge penalty
            ratingScore -= Dispossessed * 1.5;
            ratingScore -= (shotsTotal - shotsOnGoal) * 0.5; // Penalty for off-target/blocked shots
            ratingScore -= (penaltyKickAttempts - penaltyKickGoals) * 10.0; // Penalty for missed penalties

            // Contextual adjustments based on position (very important!)
            // These multipliers adjust the *impact* of a stat based on player's role.
            switch (playerMatchPosition) // e.g., "ST", "CB", "CM", "GK"
            {
                case Enums.Positions.Goalkeeper: // Goalkeeper
                    ratingScore += m_saves * 5.0; // Emphasize saves for GK
                    ratingScore += (cleansheets > 0 ? 10.0 : 0.0); // Big bonus for clean sheet
                    ratingScore -= goalsConceded * 7.0; // Big penalty for conceding
                    ratingScore -= (ErrorsLeadingToGoal * 15.0); // Even bigger for keeper errors
                    ratingScore -= fouls * 0.5; // Less penalty for GK fouls
                    break;
                case Enums.Positions.CenterBack: // Center Back
                case Enums.Positions.LeftBack: // Full Back
                case Enums.Positions.RightBack:
                    ratingScore += SuccessfulTackles * 5.0; // More for tackles
                    ratingScore += Interceptions * 5.0;
                    ratingScore += Clearances * 3.0;
                    ratingScore += Blocks * 3.0;
                    ratingScore += (cleansheets > 0 ? 5.0 : 0.0); // Clean sheet bonus
                    ratingScore -= goalsConceded * 3.0; // Penalty for team goals conceded
                    ratingScore -= ErrorsLeadingToGoal * 10.0;
                    break;
                case Enums.Positions.CentralMidfielder: // Central Midfielder (all-rounder)
                case Enums.Positions.CentralDefendingMidfielder: // Defensive Midfielder
                case Enums.Positions.CentralAttackingMidfielder: // Attacking Midfielder
                    ratingScore += ChancesCreated * 7.0; // Higher for creative actions
                    ratingScore += SuccessfulTackles * 2.0; // Still relevant
                    ratingScore += Interceptions * 2.0;
                    ratingScore += (SuccessfulPasses / (double)PassAttempts) * 15.0; // High value on passing accuracy
                    ratingScore += DuelsWon * 1.5;
                    ratingScore -= Dispossessed * 2.0; // Losing ball in midfield can be bad
                    break;
                case Enums.Positions.Striker: // Striker / Forward
                case Enums.Positions.RightWingForward: // Winger
                case Enums.Positions.LeftWingForward:
                    ratingScore += goals * 15.0; // Even more emphasis on goals
                    ratingScore += assists * 10.0;
                    ratingScore += ChancesCreated * 5.0;
                    ratingScore += SuccessfulDribbles * 3.0;
                    ratingScore -= (shotsTotal - shotsOnGoal) * 1.0; // More penalty for wasted chances
                    ratingScore -= fouls * 0.5;
                    ratingScore -= offsides * 1.0;
                    break;
                // Add more cases for specific roles if needed
                default:
                    // Default weighting if position isn't explicitly handled
                    break;
            }

            // Normalization by Minutes Played
            // This is key to ensure subs get fair ratings.
            // A simple approach: if played less than a full match, scale score relative to a baseline.
            // If they played < 10 mins, cap their rating to avoid extreme values from one action.
            if (MinutesPlayed > 15)
            {
                // This formula is an example and might need tuning:
                // It means a player playing half the minutes needs double the impact to achieve the same rating as a full-game player.
                // It prevents a 5-minute cameo from getting a 99 rating from one goal, but still rewards it.
                ratingScore = ratingScore * ((double)MinutesPlayed / 90.0) + (50.0 * (1.0 - ((double)MinutesPlayed / 90.0)));
                // This formula interpolates between the calculated score and the base 50, based on minutes played.
                // So, a player with great stats in 10 mins will still get a good rating, but not as high as if they did it for 90.
                // And a player with bad stats in 10 mins won't drop as low as if they played 90.

                //// Minimum rating for very low minutes
                //if (MinutesPlayed < 15 && ratingScore < 60.0) // If less than 15 minutes, minimum 60 (unless extremely negative actions)
                //{
                //    ratingScore = Math.Max(ratingScore, 60.0);
                //}
            }


            // Final Clamp: Ensure rating is between 1 and 100
            //this.playerRating = Math.Max(1.0, Math.Min(100.0, ratingScore));
            this.playerRating = GetDisplayMatchRating(ratingScore);
        }

        public double GetDisplayMatchRating(double playerRawRating) // Takes your 1-100 playerRating
        {
            // Step 1: Scale the 1-100 rating to a 1.0-10.0 range.
            // This ensures that a playerRating of 1 (your minimum) maps to 1.0,
            // and 100 maps to 10.0.
            double scaledRating = ((playerRawRating - 1.0) / 99.0) * 9.0 + 1.0;

            // Step 2: Round to the nearest 0.5 interval.
            // The trick here is to:
            // 1. Multiply by 2 (e.g., 7.3 * 2 = 14.6)
            // 2. Round to the nearest whole number (e.g., Round(14.6) = 15)
            // 3. Divide by 2 (e.g., 15 / 2 = 7.5)
            double roundedRating = Math.Round(scaledRating * 2.0) / 2.0;

            // Step 3 (Optional but Recommended): Ensure the result is still within 1.0 and 10.0
            // In most cases, the scaling and rounding will keep it here, but a final clamp
            // adds robustness against floating-point quirks or unexpected inputs.
            return Math.Max(1.0, Math.Min(10.0, roundedRating));
        }

        // This function would be called for each player after their CalculateFinalMatchRating has run.
        public double CalculateMoraleChangeFromMatchRating(double playerMatchRating) // Renamed parameter for clarity
        {
            double moraleChangeFromRating = 0.0;

            // Assuming playerMatchRating is 1-100
            // Let's use 65 as the neutral threshold:
            // Ratings >= 65 increase morale
            // Ratings < 65 decrease morale
            const double RATING_NEUTRAL_THRESHOLD = 65.0;

            // Tunable parameters for the impact strength:
            // These define the max gain/loss if the rating hits 100 or 1 respectively
            const double MAX_RATING_GAIN = 8.0; // Max morale points gained for a 100 rating
            const double MAX_RATING_LOSS = -12.0; // Max morale points lost for a 1 rating

            if (playerMatchRating >= RATING_NEUTRAL_THRESHOLD)
            {
                // Scale the bonus: 0 at threshold, 1 at 100 rating
                double bonusRatio = (playerMatchRating - RATING_NEUTRAL_THRESHOLD) / (100.0 - RATING_NEUTRAL_THRESHOLD);
                moraleChangeFromRating = bonusRatio * MAX_RATING_GAIN;
            }
            else // playerMatchRating < RATING_NEUTRAL_THRESHOLD
            {
                // Scale the penalty: 0 at threshold, 1 at 1 rating
                double penaltyRatio = (RATING_NEUTRAL_THRESHOLD - playerMatchRating) / (RATING_NEUTRAL_THRESHOLD - 1.0); // -1.0 to ensure 1.0 rating gives full penalty
                moraleChangeFromRating = penaltyRatio * MAX_RATING_LOSS;
            }

            return moraleChangeFromRating;
        }

        public double CalculateTotalMoraleChange(
            double playerMatchRating,
            bool teamWon, bool teamLost, bool teamDrew, // Team result
            int playerGoals, int playerAssists, int playerRedCards, int playerErrorsLeadingToGoal,
            bool playerKeptCleanSheet, int goalsConceded, // Defensive stats (useful for GK/Defenders)
            Enums.Positions playerMatchPosition) // To check for defensive roles
        {
            double totalMoraleChange = 0.0;

            // 1. Morale change from Match Rating (primary individual factor)
            totalMoraleChange += CalculateMoraleChangeFromMatchRating(playerMatchRating);

            // 2. Morale change from Team Result (shared factor, can be adjusted by personality later)
            if (teamWon)
            {
                totalMoraleChange += 5.0; // Base morale boost for winning
            }
            else if (teamLost)
            {
                totalMoraleChange -= 7.0; // Base morale hit for losing
            }
            else if (teamDrew)
            {
                totalMoraleChange += 1.0; // Small morale boost for a draw (prevents stagnation if team is mediocre)
            }

            // 3. Morale change from Key Individual Events
            totalMoraleChange += playerGoals * 4.0; // Each goal is a significant morale boost
            totalMoraleChange += playerAssists * 2.5; // Each assist is also good

            totalMoraleChange -= playerRedCards * 15.0; // Major morale hit for a red card
            totalMoraleChange -= playerErrorsLeadingToGoal * 10.0; // Major hit for critical errors

            // Defensive contributions
            if (playerKeptCleanSheet && (playerMatchPosition == Enums.Positions.Goalkeeper ||
                                         playerMatchPosition == Enums.Positions.CenterBack ||
                                         playerMatchPosition == Enums.Positions.LeftBack ||
                                         playerMatchPosition == Enums.Positions.RightBack))
            {
                totalMoraleChange += 3.0; // Bonus for defenders/GK if team keeps a clean sheet
            }
            // Consider adding a small penalty for goals conceded IF the player's match rating wasn't already terrible for it.
            // However, the MatchRating should already account for this, so be careful not to double-penalize.
            // For now, let's omit direct "goalsConceded" impact to avoid double-counting if your MatchRating already penalizes it heavily.

            // You can add more specific event impacts here if needed:
            // e.g., Penalty miss: totalMoraleChange -= 5.0;
            // e.g., Penalty save: totalMoraleChange += 5.0;

            return totalMoraleChange;
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
            SecondsWithBall = 0;
            penaltyKickAttempts = 0;
            penaltyKickGoals = 0;
            cornerKicks = 0;
            matchesPlayed = 0;

            // Initialize new properties too
            SuccessfulTackles = 0;
            Interceptions = 0;
            Clearances = 0;
            Blocks = 0;
            ChancesCreated = 0;
            SuccessfulDribbles = 0;
            DuelsWon = 0;
            PassAttempts = 0;
            SuccessfulPasses = 0;
            ErrorsLeadingToGoal = 0;
            Dispossessed = 0;
            SecondsPlayed = 0; // This should be updated in the main simulation loop
            // PlayerReference will be set when the object is created for a specif
        }
    }
}
