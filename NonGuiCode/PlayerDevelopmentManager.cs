using FunnyOldGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FunnyOldGameRedux.NonGuiCode
{
    // --- 2. PlayerDevelopmentManager (or similar static class/part of Player) ---
    // This static class contains the logic for aging and attribute updates.
    // You could also place these methods directly inside your Player class if preferred,
    // but a static manager often helps organize game logic.
    public static class PlayerDevelopmentManager
    {
        // Utility for random numbers (ensure it's static and used consistently across your game)
        private static readonly Random Rng = new Random();

        // Map all 29 OUTFIELD attributes to their respective categories for aging/decline
        // Note: Goalkeeper attributes would have their own aging logic/categories if desired.
        private static readonly Dictionary<string, Enums.AttributeCategory> AttributeCategories = new Dictionary<string, Enums.AttributeCategory>
        {
            {"Acceleration", Enums.AttributeCategory.Physical}, {"SprintSpeed", Enums.AttributeCategory.Physical},
            {"Jumping", Enums.AttributeCategory.Physical}, {"Stamina", Enums.AttributeCategory.Physical},
            {"Strength", Enums.AttributeCategory.Physical}, {"Agility", Enums.AttributeCategory.Physical},
            {"Balance", Enums.AttributeCategory.Physical}, {"Reactions", Enums.AttributeCategory.Physical}, // Reactions often have a physical component

            {"Finishing", Enums.AttributeCategory.Technical}, {"ShotPower", Enums.AttributeCategory.Technical},
            {"LongShot", Enums.AttributeCategory.Technical}, {"Volleys", Enums.AttributeCategory.Technical},
            {"Penalties", Enums.AttributeCategory.Technical}, {"Dribbling", Enums.AttributeCategory.Technical},
            {"BallControl", Enums.AttributeCategory.Technical}, {"Crossing", Enums.AttributeCategory.Technical},
            {"FreeKicks", Enums.AttributeCategory.Technical}, {"ShortPass", Enums.AttributeCategory.Technical},
            {"LongPass", Enums.AttributeCategory.Technical}, {"Curve", Enums.AttributeCategory.Technical},
            {"HeadingAccuracy", Enums.AttributeCategory.Technical},

            {"Positioning", Enums.AttributeCategory.Mental}, {"Vision", Enums.AttributeCategory.Mental},
            {"Composure", Enums.AttributeCategory.Mental}, {"Interceptions", Enums.AttributeCategory.Mental},
            {"DefensiveAwareness", Enums.AttributeCategory.Mental}, {"StandingTackle", Enums.AttributeCategory.Mental},
            {"SlidingTackle", Enums.AttributeCategory.Mental}, {"Aggression", Enums.AttributeCategory.Mental}
        };

        // Map all OUTFIELD attributes to their respective TrainingCategories
        // This is crucial for the training system to know which coach category applies to which attribute.
        // Ensure all 29 outfield attributes are mapped.
        private static readonly Dictionary<string, Enums.TrainingCategory> AttributeToTrainingCategoryMap = new Dictionary<string, Enums.TrainingCategory>
    {
        // Physical
        {"Acceleration", Enums.TrainingCategory.Fitness},
        {"SprintSpeed", Enums.TrainingCategory.Fitness},
        {"Jumping", Enums.TrainingCategory.Fitness},
        {"Stamina", Enums.TrainingCategory.Fitness},
        {"Strength", Enums.TrainingCategory.Fitness},
        {"Agility", Enums.TrainingCategory.Fitness},
        {"Balance", Enums.TrainingCategory.Fitness},
        {"Reactions", Enums.TrainingCategory.Mental}, // Reactions has mental and physical components, let's say Mental for training
        
        // Technical
        {"Finishing", Enums.TrainingCategory.Attacking},
        {"ShotPower", Enums.TrainingCategory.Attacking},
        {"LongShot", Enums.TrainingCategory.Attacking},
        {"Volleys", Enums.TrainingCategory.Attacking},
        {"Penalties", Enums.TrainingCategory.Attacking},
        {"Dribbling", Enums.TrainingCategory.Technical},
        {"BallControl", Enums.TrainingCategory.Technical},
        {"Crossing", Enums.TrainingCategory.Technical},
        {"FreeKicks", Enums.TrainingCategory.Technical},
        {"ShortPass", Enums.TrainingCategory.Technical},
        {"LongPass", Enums.TrainingCategory.Technical},
        {"Curve", Enums.TrainingCategory.Technical},
        {"HeadingAccuracy", Enums.TrainingCategory.Defending}, // Can be Attacking too, but often defensive for headers

        // Mental
        {"Positioning", Enums.TrainingCategory.Mental},
        {"Vision", Enums.TrainingCategory.Mental},
        {"Composure", Enums.TrainingCategory.Mental},
        {"Interceptions", Enums.TrainingCategory.Defending},
        {"DefensiveAwareness", Enums.TrainingCategory.Defending},
        {"StandingTackle", Enums.TrainingCategory.Defending},
        {"SlidingTackle", Enums.TrainingCategory.Defending},
        {"Aggression", Enums.TrainingCategory.Mental}
    };

        // Map Goalkeeper attributes to their TrainingCategory
        private static readonly Dictionary<string, Enums.TrainingCategory> GkAttributeToTrainingCategoryMap = new Dictionary<string, Enums.TrainingCategory>
    {
        {"GKDiving", Enums.TrainingCategory.Goalkeeping},
        {"GKHandling", Enums.TrainingCategory.Goalkeeping},
        {"GKKicking", Enums.TrainingCategory.Goalkeeping},
        {"GKReflexes", Enums.TrainingCategory.Goalkeeping},
        {"GKPositioning", Enums.TrainingCategory.Goalkeeping},
        // Assuming other GK attributes like GKComposure, GKAwareness map to Goalkeeping
    };

        // List of all OUTFIELD attribute names for consistent iteration (ensure matches Player property names)
        private static readonly List<string> AllOutfieldAttributeNames = new List<string>
    {
        "Acceleration", "SprintSpeed", "Positioning", "Finishing", "ShotPower",
        "LongShot", "Volleys", "Penalties", "Vision", "Crossing",
        "FreeKicks", "ShortPass", "LongPass", "Curve", "Dribbling",
        "Agility", "Balance", "Reactions", "BallControl", "Composure",
        "Interceptions", "HeadingAccuracy", "DefensiveAwareness", "StandingTackle", "SlidingTackle",
        "Jumping", "Stamina", "Strength", "Aggression"
    };

        // --- Helper Method: GetBaseGrowthRate ---
        // Defines the natural, age-dependent growth rate for younger players.
        // Returns a base growth rate (e.g., points per year)
        public static double GetBaseGrowthRate(int age)
        {
            if (age < 18) return 4.0; // Very rapid growth
            if (age < 22) return 3.0; // High growth
            if (age < 26) return 2.0; // Moderate growth
            if (age < 29) return 1.0; // Slow growth
            if (age < 31) return 0.5; // Very slow growth, almost maintenance
            return 0.0; // No natural growth from this factor after peak
        }

        // --- Helper Method: GetPotentialInfluence ---
        // Determines how much an attribute can still grow based on its current value relative to potential.
        // The closer to potential, the harder it is to grow.
        public static double GetPotentialInfluence(int currentAttr, int playerPotentialRating)
        {
            // Define a slight buffer above overall potential for individual stats to excel
            // This means individual attributes can potentially exceed the overall potential by a small margin
            int attributePotentialCap = playerPotentialRating + 5;

            if (currentAttr >= attributePotentialCap) return -2.0; // Strong negative influence if way above individual potential cap
            if (currentAttr >= playerPotentialRating) return -0.5; // Slight negative if at/above overall potential

            // Growth is higher when further from potential
            double differenceToPotential = playerPotentialRating - currentAttr;
            if (differenceToPotential > 20) return 2.0; // Far from potential, good growth opportunity
            if (differenceToPotential > 10) return 1.0; // Moderately far
            if (differenceToPotential > 0) return 0.5;  // Close to potential, slower growth

            return 0.0; // Already at or above potential
        }

        // --- Helper Method: GetAgeDecline ---
        // Calculates the negative impact of aging, with different rates for different attribute categories.
        public static double GetAgeDecline(int age, Enums.AttributeCategory category)
        {
            if (age <= 29) return 0.0; // No significant decline before 30 (adjust this peak age as desired)

            // Base decline increases with age past the peak
            double baseDecline = (age - 29) * 0.2; // Example: 0.2 points decline per year over 29

            if (category == Enums.AttributeCategory.Physical)
            {
                // Physical attributes decline significantly faster
                return baseDecline * 2.5; // Example: 2.5x faster than base decline
            }
            else if (category == Enums.AttributeCategory.Technical)
            {
                // Technical attributes decline at a moderate pace
                return baseDecline * 0.8; // Example: 0.8x faster than base decline
            }
            else if (category == Enums.AttributeCategory.Mental)
            {
                // Mental attributes might hold longer or even slightly increase due to experience
                if (age < 34) return -0.1; // Small positive influence from experience until mid-30s
                return baseDecline * 0.4; // Very slow decline later
            }
            return baseDecline; // Default for uncategorized or base case (shouldn't be hit with proper categories)
        }

        /// <summary>
        /// Updates a player's attributes for one year, incorporating natural aging, potential, and training effects.
        /// This method now orchestrates the calculation of the yearlyTrainingEffect internally.
        /// </summary>
        /// <param name="player">The player whose attributes are being updated.</param>
        /// <param name="club">The club the player belongs to (needed for training facilities/coaches).</param>
        /// <param name="yearlyAvgIntensity">The average training intensity set by the manager for the past year.</param>
        public static void UpdateYearlyAttributes(Player player, Team club, Enums.TrainingIntensity yearlyAvgIntensity)
        {
            player.AgeOneYear(); // Player ages up

            // --- Step 1: Initialize yearlyTrainingEffect ---
            // This dictionary will accumulate all training gains for the year for each attribute.
            Dictionary<string, double> yearlyTrainingEffect = new Dictionary<string, double>();

            // Determine which set of attributes to iterate over (outfield or GK)
            List<string> attributesToConsider;
            if (player.Position == Enums.Positions.Goalkeeper)
            {
                attributesToConsider = GkAttributeToTrainingCategoryMap.Keys.ToList();
            }
            else
            {
                attributesToConsider = AttributeToTrainingCategoryMap.Keys.ToList();
            }

            // --- Step 2: Accumulate yearlyTrainingEffect by calling CalculateAttributeTrainingGainForPeriod ---
            // Assuming CalculateAttributeTrainingGainForPeriod gives you the gain for one 'period' (e.g., week)
            // You'd typically loop this N times (e.g., 52 times for 52 weeks in a year)
            int numberOfTrainingPeriodsPerYear = 52; // Example: Weekly training calculations

            foreach (string attrName in attributesToConsider)
            {
                double totalTrainingGainForAttribute = 0.0;
                for (int i = 0; i < numberOfTrainingPeriodsPerYear; i++)
                {
                    // Only update fitness/injury risk if there's actual training this day
                    // (You'd need a system for daily training schedules)
                    // For now, let's assume training happens every day of the period
                    bool wasInjuredThisDay = PlayerDevelopmentManager.UpdatePlayerFitnessAndInjuryRiskForPeriod(
                                                player, club, yearlyAvgIntensity);

                    // This is where you'd decide if you need to roll for injury for match play too.

                    // Advance recovery for ALL players (injured or not, no harm in calling)
                    PlayerDevelopmentManager.AdvancePlayerRecoveryDaily(player, club);

                    // HERE IS WHERE CLUB AND YEARLYAVGINTENSITY ARE USED:
                    double gainForPeriod = CalculateAttributeTrainingGainForPeriod(
                        player,
                        attrName,
                        club,
                        yearlyAvgIntensity
                    );
                    totalTrainingGainForAttribute += gainForPeriod;
                }
                yearlyTrainingEffect[attrName] = totalTrainingGainForAttribute;
            }

            // --- Step 3: Apply ALL attribute changes ---
            // This part uses reflection to dynamically update each attribute
            Type playerType = player.GetType();
            foreach (string attrName in attributesToConsider)
            {
                PropertyInfo property = playerType.GetProperty(attrName);
                if (property == null || property.PropertyType != typeof(int))
                {
                    continue; // Skip if it's not a valid int attribute
                }

                Enums.AttributeCategory category = Enums.AttributeCategory.Technical;
                if (AttributeCategories.ContainsKey(attrName))
                {
                    category = AttributeCategories[attrName];
                }

                int currentAttrValue = (int)property.GetValue(player);

                // Calculate individual components of the total change:
                double baseGrowth = GetBaseGrowthRate(player.age);
                double potentialInfluence = GetPotentialInfluence(currentAttrValue, player.PotentialRating);
                double ageDecline = GetAgeDecline(player.age, category);

                // --- THIS IS WHERE THE YEARLY TRAINING EFFECT IS APPLIED ---

                double trainingEffect = 0.0; // Get the accumulated training gain
                if (yearlyTrainingEffect.ContainsKey(attrName))
                {
                    trainingEffect = yearlyTrainingEffect[attrName];
                }

                // Add a small random factor for natural variance
                double randomFactor = (Rng.NextDouble() * 2.0) - 1.0; // Random number between -1.0 and 1.0

                // Calculate total change for this attribute
                double totalChange = baseGrowth
                                   + potentialInfluence
                                   + ageDecline
                                   + trainingEffect // This is the sum of training gains from the year
                                   + randomFactor;

                // Apply the change to the attribute
                int newAttrValue = currentAttrValue + (int)Math.Round(totalChange);

                // Clamp the attribute value between reasonable bounds (e.g., 1 to 99)
                newAttrValue = Math.Max(1, Math.Min(newAttrValue, 99));

                property.SetValue(player, newAttrValue);
            }

            // --- Step 4: Recalculate Overall Rating ---
            // After all individual attributes are updated, recalculate the player's overall rating.
            // Assuming CalculateOverallRating method exists (as discussed previously).
            //player.OverallRating = PlayerRatingCalculator.CalculateOverallRating(player);

            // --- Step 5: Decrement Contract Years ---
            player.CurrentContract.DecrementYear();
        }

        // --- You would need a similar method for Goalkeepers if their aging logic is distinct ---
        public static void UpdateYearlyGoalkeeperAttributes(Player player, Dictionary<string, double> yearlyTrainingEffect)
        {
            //player.age++; // Player ages up
            Type playerType = player.GetType();

            // List of GK attributes for iteration
            List<string> gkAttributeNames = new List<string>
        {
            "GoalkeepingDiving", "GoalKeepingHandling", "GoalKeepingKicking", "GoalKeepingReflexes", "GoalKeepingPositioning"
        };

            // Define GK-specific attribute categories for aging (if different from outfield)
            // For example:
            Dictionary<string, Enums.AttributeCategory> gkAttributeCategories = new Dictionary<string, Enums.AttributeCategory>
         {
             {"GoalkeepingDiving", Enums.AttributeCategory.Physical}, // GK Diving might decline similarly to outfield Physicals
             {"GoalKeepingHandling", Enums.AttributeCategory.Technical},
             {"GoalKeepingKicking", Enums.AttributeCategory.Physical}, // GK Diving might decline similarly to outfield Physicals
             {"GoalKeepingReflexes", Enums.AttributeCategory.Physical},
             {"GoalKeepingPositioning", Enums.AttributeCategory.Technical}, // GK Diving might decline similarly to outfield Physicals
             {"GoalKeepingHandling", Enums.AttributeCategory.Technical},
         };

            foreach (var attrName in gkAttributeNames)
            {
                PropertyInfo property = playerType.GetProperty(attrName);
                if (property == null || property.PropertyType != typeof(int))
                {
                    //Console.WriteLine($"Error: Goalkeeper object does not have an integer property named '{attrName}'. Skipping attribute update.");
                    continue;
                }
                int currentAttrValue = (int)property.GetValue(player.trueRating);

                // You'd use GK-specific categories or a general category if GKs are simpler
                Enums.AttributeCategory category = Enums.AttributeCategory.Technical;
                if (gkAttributeCategories.ContainsKey(attrName))
                {
                    category = gkAttributeCategories[attrName];
                }

                // Recalculate using GK-specific age decline curves if needed
                double baseGrowth = GetBaseGrowthRate(player.age); // Could be shared
                double potentialInfluence = GetPotentialInfluence(currentAttrValue, player.PotentialRating); // Could be shared
                double ageDecline = GetGKAgeDecline(player.age, attrName); // Potentially new GK-specific decline method
                double trainingEffect = 0.0;
                if (yearlyTrainingEffect.ContainsKey(attrName))
                {
                    trainingEffect = yearlyTrainingEffect[attrName];
                }
                double randomFactor = (Rng.NextDouble() * 2.0) - 1.0;

                double totalChange = baseGrowth + potentialInfluence + ageDecline + trainingEffect + randomFactor;

                int newAttrValue = (int)Math.Round(currentAttrValue + totalChange);
                newAttrValue = Math.Max(1, Math.Min(newAttrValue, 99));
                property.SetValue(player, newAttrValue);
            }

            // After updating all GK attributes, recalculate the player's OverallRating
            // player.OverallRating = PlayerRatingCalculator.CalculateOverallRatingForGoalkeeper(player);
        }

        public static double GetGKAgeDecline(int age, string gkAttributeName) // Could pass name directly or a GK-specific category
        {
            if (age <= 30) return 0.0; // GKs often peak later and hold form longer

            double baseDecline = (age - 30) * 0.15; // Slower base decline than outfielders

            // Adjust decline based on the specific GK attribute
            switch (gkAttributeName)
            {
                case "GoalkeepingDiving":
                case "GoalKeepingReflexes":
                    // These might decline faster as physical quickness is involved
                    return baseDecline * 1.5;
                case "GoalKeepingHandling":
                case "GoalKeepingKicking":
                case "Strength":
                    // More "technical" or "physical but less agile" might decline moderately
                    return baseDecline * 0.8;
                case "GoalKeepingPositioning":
                case "Composure":
                case "Awareness":
                    // Mental attributes decline very slowly, or even have a small positive experience bump
                    if (age < 36) return -0.05; // Small positive from experience
                    return baseDecline * 0.2;
                default:
                    return baseDecline;
            }
        }

        /// <summary>
        /// Updates a player's current fitness level and calculates the chance of injury for a single training period.
        /// </summary>
        /// <param name="player">The player whose fitness and injury risk are being updated.</param>
        /// <param name="club">The club the player belongs to (for medical staff/facilities).</param>
        /// <param name="currentIntensity">The training intensity for this period.</param>
        /// <returns>True if the player gets injured during this period, false otherwise.</returns>
        public static bool UpdatePlayerFitnessAndInjuryRiskForPeriod(
            Player player,
            Team club,
            Enums.TrainingIntensity currentIntensity)
        {
            // --- 1. Update Player Fitness (Fatigue) ---

            // Base fitness drain per period (e.g., for a "normal" session)
            double baseFitnessDrain = 5.0; // Points of fitness lost

            // Intensity Multiplier for Fitness Drain
            double intensityDrainMultiplier = 1.0;
            switch (currentIntensity)
            {
                case Enums.TrainingIntensity.Light: intensityDrainMultiplier = 0.7; break;
                case Enums.TrainingIntensity.Normal: intensityDrainMultiplier = 1.0; break;
                case Enums.TrainingIntensity.Heavy: intensityDrainMultiplier = 1.3; break;
                case Enums.TrainingIntensity.VeryHeavy: intensityDrainMultiplier = 1.6; break;
            }

            // Stamina Influence on Fitness Drain (Higher Stamina = Less Drain)
            // Assuming Player.Stamina is 1-99. Normalize it.
            // A player with 99 stamina loses very little, 1 stamina loses almost the full drain.
            double staminaDrainReduction = 1.0 - (player.trueRating.Stamina / 99.0); // (99-Stamina)/99 gives good range
            staminaDrainReduction = Math.Max(0.05, staminaDrainReduction); // Ensure at least 5% of drain even for max stamina

            // Random noise for variability in fitness drain
            double randomFitnessNoise = (Rng.NextDouble() * 3.0) - 1.5; // Random between -1.5 and +1.5

            double fitnessReduction = baseFitnessDrain
                                    * intensityDrainMultiplier
                                    * staminaDrainReduction
                                    + randomFitnessNoise;

            // Apply fitness reduction and clamp between 0 and 100
            player.CurrentFitness = (int)Math.Max(0, Math.Min(100, player.CurrentFitness - fitnessReduction));
            // Console.WriteLine($"  {player.Name} Fitness after training: {player.CurrentFitness}");


            // --- 2. Calculate Injury Chance ---

            double baseInjuryChance = 0.005; // 0.5% base chance per period (e.g., per week)

            // Player's inherent InjuryProneness (1-20 scale)
            // Higher proneness (e.g., 20) means full multiplier, lower (1) means very low multiplier
            double pronenessMultiplier = (player.InjuryResistance / 20.0); // Max 1.0, Min 0.05 (for proneness=1)
            pronenessMultiplier = Math.Max(0.05, pronenessMultiplier); // Ensure non-zero risk

            // Intensity Multiplier for Injury Risk (more aggressive than fitness drain)
            double intensityInjuryMultiplier = 1.0;
            switch (currentIntensity)
            {
                case Enums.TrainingIntensity.Light: intensityInjuryMultiplier = 0.6; break;
                case Enums.TrainingIntensity.Normal: intensityInjuryMultiplier = 1.0; break;
                case Enums.TrainingIntensity.Heavy: intensityInjuryMultiplier = 2.0; break;
                case Enums.TrainingIntensity.VeryHeavy: intensityInjuryMultiplier = 4.0; break;
            }

            // Fitness/Fatigue Multiplier for Injury Risk (Crucial: low fitness = high risk)
            double fitnessFatigueMultiplier = 1.0;
            if (player.CurrentFitness >= 80) fitnessFatigueMultiplier = 0.8; // Very fit, slightly lower risk
            else if (player.CurrentFitness >= 60) fitnessFatigueMultiplier = 1.0;
            else if (player.CurrentFitness >= 40) fitnessFatigueMultiplier = 1.5; // Moderate fatigue, increased risk
            else if (player.CurrentFitness >= 20) fitnessFatigueMultiplier = 2.5; // High fatigue, significantly increased risk
            else fitnessFatigueMultiplier = 4.0; // Extremely fatigued, very high risk

            // Age Multiplier for Injury Risk
            double ageInjuryMultiplier = 1.0;
            if (player.age >= 30) ageInjuryMultiplier = 1.0 + ((player.age - 29) * 0.1); // 10% increase per year over 29
            ageInjuryMultiplier = Math.Min(3.0, ageInjuryMultiplier); // Cap at 3x for very old players

            // Medical Staff Mitigation
            int effectiveMedicalSkill = club.GetEffectiveMedicalSkill(); // Assuming 1-20 scale
            double medicalStaffMitigation = 1.0 - (effectiveMedicalSkill / 20.0 * 0.5); // Max 50% reduction (e.g. skill 20 = 0.5)
            medicalStaffMitigation = Math.Max(0.5, medicalStaffMitigation); // Cap at 50% reduction

            // Medical Facilities Mitigation
            // Assuming MedicalFacilitiesLevel is 1-5 scale
            double medicalFacilitiesMitigation = 1.0 - (club.MedicalFacilitiesLevel / 5.0 * 0.2); // Max 20% reduction (e.g. level 5 = 0.8)
            medicalFacilitiesMitigation = Math.Max(0.8, medicalFacilitiesMitigation); // Cap at 20% reduction

            // Random fluctuation for injury chance
            double randomInjuryNoise = (Rng.NextDouble() * 0.005) - 0.0025; // Random between -0.25% and +0.25%

            // Calculate Total Injury Chance for this period
            double totalInjuryChance = (baseInjuryChance
                                       * pronenessMultiplier
                                       * intensityInjuryMultiplier
                                       * fitnessFatigueMultiplier
                                       * ageInjuryMultiplier
                                       * medicalStaffMitigation
                                       * medicalFacilitiesMitigation)
                                       + randomInjuryNoise;

            totalInjuryChance = Math.Max(0.0001, totalInjuryChance); // Ensure a very tiny non-zero chance
            totalInjuryChance = Math.Min(1.0, totalInjuryChance); // Cap at 100% chance (shouldn't happen often)

            // Console.WriteLine($"  {player.Name} Injury Chance for period: {totalInjuryChance:P2}"); // P2 for percentage format

            // --- 3. Determine if Injury Occurs ---
            if (Rng.NextDouble() < totalInjuryChance)
            {
                // Player got injured!
                // Call the new method to generate a specific injury
                Injury newInjury = InjuryManager.GenerateRandomInjury(player, currentIntensity, player.CurrentFitness);

                player.ActiveInjuries.Add(newInjury); // Add to active injuries
                player.InjuryHistory.Add(newInjury); // Add to history

                //Console.WriteLine($"!!! {player.Name} suffered a {newInjury.Severity} {newInjury.Type} ({newInjury.OriginalDurationDays} days) during training (Intensity: {currentIntensity}, Fitness: {player.CurrentFitness}) !!!");
                return true; // Indicate that an injury occurred
            }

            return false; // No injury this period
        }

        /// <summary>
        /// Advances the recovery process for all active injuries a player has by one day.
        /// Factors in medical staff and facilities.
        /// </summary>
        /// <param name="player">The player whose injuries are being advanced.</param>
        /// <param name="club">The club the player belongs to, for medical resources.</param>
        public static void AdvancePlayerRecoveryDaily(Player player, Team club)
        {
            // No need to do anything if the player isn't injured
            if (!player.IsInjured)
            {
                return;
            }

            // Create a temporary list to avoid modifying the collection while iterating
            List<Injury> injuriesToProcess = new List<Injury>(player.ActiveInjuries);
            List<Injury> recoveredInjuries = new List<Injury>();

            foreach (Injury injury in injuriesToProcess)
            {
                // --- Calculate Daily Recovery Rate ---
                double recoveryRate = 1.0; // Base: 1 day of recovery per real day

                // Player HealFactor Influence (Higher HealFactor = faster recovery)
                // This calculates a bonus based on HealFactor.
                // Example: At 100 HealFactor, maybe 20% faster recovery. At 1, no bonus.
                double healFactorRecoveryBonus = (player.trueRating.Stamina / 100.0) * 0.20; // Up to 20% faster
                recoveryRate *= (1.0 + healFactorRecoveryBonus);

                injury.RemainingDurationDays -= (int)Math.Round(recoveryRate);

                // Medical Staff Influence (Higher skill = faster recovery)
                int effectiveMedicalSkill = club.GetEffectiveMedicalSkill(); // 1-20 scale
                // Skill 20: 1.25x faster (25% faster recovery)
                // Skill 1: 1.0x (no bonus)
                recoveryRate *= (1.0 + ((effectiveMedicalSkill - 1) / 19.0) * 0.25);

                // Medical Facilities Influence (Higher level = faster recovery)
                // Level 5: 1.15x faster (15% faster recovery)
                // Level 1: 1.0x (no bonus)
                recoveryRate *= (1.0 + ((club.MedicalFacilitiesLevel - 1) / 4.0) * 0.15);

                // Severity Influence (More severe injuries might recover slower, or need a minimum recovery)
                switch (injury.Severity)
                {
                    case "Minor": recoveryRate *= 1.0; break;
                    case "Moderate": recoveryRate *= 0.9; break; // Slightly slower
                    case "Serious": recoveryRate *= 0.7; break; // Significantly slower
                    case "Career-Ending": recoveryRate *= 0.5; break; // Very slow, often with lasting effects
                }

                // Apply a small random variance to recovery
                recoveryRate += (Rng.NextDouble() * 0.1) - 0.05; // +/- 5% variance
                recoveryRate = Math.Max(0.1, recoveryRate); // Ensure at least some recovery, even if very slow


                // --- Advance Injury Duration ---
                injury.RemainingDurationDays -= (int)Math.Round(recoveryRate);
                injury.AdvanceRecoveryDay(); // Decrement by 1, and then effectively by the bonus from recoveryRate calculation

                if (injury.IsRecovered())
                {
                    recoveredInjuries.Add(injury);
                }
            }

            // --- Handle Recovered Injuries ---
            foreach (Injury recoveredInjury in recoveredInjuries)
            {
                player.ActiveInjuries.Remove(recoveredInjury);
                //Console.WriteLine($"{player.Name} has recovered from a {recoveredInjury.Type} ({recoveredInjury.Severity}).");

                // Apply Lingering Effects if any
                if (recoveredInjury.LingeringEffects != null && recoveredInjury.LingeringEffects.Any())
                {
                    //Console.WriteLine($"  Applying lingering effects for {player.Name}'s {recoveredInjury.Type}:");
                    foreach (var effect in recoveredInjury.LingeringEffects)
                    {
                        PropertyInfo property = player.GetType().GetProperty(effect.Key);
                        if (property != null && property.PropertyType == typeof(int))
                        {
                            int currentValue = (int)property.GetValue(player);
                            int newValue = currentValue + (int)Math.Round(effect.Value); // Value is negative for reduction
                            newValue = Math.Max(1, Math.Min(newValue, 99)); // Clamp attribute values

                            property.SetValue(player, newValue);
                            //Console.WriteLine($"    {effect.Key}: {currentValue} -> {newValue}");
                        }
                        else if (property != null && property.PropertyType == typeof(double)) // For properties like 'OverallRating' if you store as double temporarily
                        {
                            double currentValue = (double)property.GetValue(player);
                            double newValue = currentValue + effect.Value;
                            property.SetValue(player, newValue);
                            //Console.WriteLine($"    {effect.Key}: {currentValue:F0} -> {newValue:F0}"); // Format if needed
                        }
                        else // For specific properties like InjuryProneness
                        {
                            if (effect.Key == "InjuryProneness")
                            {
                                player.InjuryResistance = (int)Math.Max(1, Math.Min(20, player.InjuryResistance + (int)Math.Round(effect.Value)));
                                //Console.WriteLine($"    InjuryProneness: {player.InjuryProneness}");
                            }
                            // Add more specific handling for other non-int attributes if needed
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the training gain (or loss) for a single attribute of a player for one training period (e.g., a week).
        /// </summary>
        /// <param name="player">The Player object being trained.</param>
        /// <param name="attributeName">The string name of the attribute to calculate gain for (e.g., "Finishing").</param>
        /// <param name="club">The Club object, to access facilities and assigned coaches.</param>
        /// <param name="currentIntensity">The manager's chosen training intensity for the period.</param>
        /// <returns>The calculated gain (or loss) in attribute points for this period.</returns>
        public static double CalculateAttributeTrainingGainForPeriod(
            Player player,
            string attributeName,
            Team club,
            Enums.TrainingIntensity currentIntensity)
        {
            Enums.TrainingCategory category;

            // Determine if it's an outfield or goalkeeper attribute to use the correct map
            if (AttributeToTrainingCategoryMap.ContainsKey(attributeName))
            {
                category = AttributeToTrainingCategoryMap[attributeName];
            }
            else if (GkAttributeToTrainingCategoryMap.ContainsKey(attributeName))
            {
                category = GkAttributeToTrainingCategoryMap[attributeName];
            }
            else
            {
                // If attribute not mapped, return no gain
                //Console.WriteLine($"Warning: Attribute '{attributeName}' not mapped to a training category. No training gain.");
                return 0.0;
            }

            // --- 1. Base Gain Per Period ---
            // This is the fundamental potential gain if all factors were neutral.
            // It can be adjusted based on the attribute's category.
            double baseGain = 0.1; // Example: 0.1 points per week.

            // Slight adjustments based on category (e.g., physical might have higher base gain when young)
            if (category == Enums.TrainingCategory.Fitness) baseGain = 0.15;
            else if (category == Enums.TrainingCategory.Goalkeeping) baseGain = 0.12;


            // --- 2. Coach Influence ---
            // Gets the highest skill from assigned coaches for this category from the Club.
            int effectiveCoachSkill = club.GetEffectiveCoachingSkill(category);
            // Coach skill from 1 to 20, max influence is 1.0 (double the base gain)
            double coachInfluenceMultiplier = 1.0 + ((double)effectiveCoachSkill / 20.0); // E.g., Skill 20 = 1.0 + 1.0 = 2.0x boost
            coachInfluenceMultiplier = Math.Max(1.0, coachInfluenceMultiplier); // Ensure minimum multiplier of 1.0 even with low skill


            // --- 3. Facilities Influence ---
            // Assuming TrainingFacilitiesLevel is 1-5 scale, where 5 is max.
            double facilitiesLevel = club.TrainingFacilitiesLevel;
            double facilitiesInfluenceMultiplier = 1.0 + (facilitiesLevel / 5.0); // E.g., Level 5 = 1.0 + 1.0 = 2.0x boost
            facilitiesInfluenceMultiplier = Math.Max(1.0, facilitiesInfluenceMultiplier);


            // --- 4. Training Intensity Influence ---
            double intensityMultiplier = 1.0;
            switch (currentIntensity)
            {
                case Enums.TrainingIntensity.Light: intensityMultiplier = 0.7; break;
                case Enums.TrainingIntensity.Normal: intensityMultiplier = 1.0; break;
                case Enums.TrainingIntensity.Heavy: intensityMultiplier = 1.2; break;
                case Enums.TrainingIntensity.VeryHeavy: intensityMultiplier = 1.4; break; // High risk, high reward
            }


            // --- 5. Player Morale Influence ---
            // Morale from 0-100.
            double moraleInfluenceMultiplier = 1.0;
            if (player.Morale >= 90) moraleInfluenceMultiplier = 1.1; // Excellent morale
            else if (player.Morale >= 70) moraleInfluenceMultiplier = 1.05; // Good morale
            else if (player.Morale <= 30) moraleInfluenceMultiplier = 0.8; // Poor morale
            else if (player.Morale <= 10) moraleInfluenceMultiplier = 0.6; // Very poor morale


            // --- 6. Age Dampening (for training effectiveness, separate from natural decline) ---
            // Training becomes less effective as players get older.
            double ageTrainingDampening = 1.0;
            if (player.age >= 28)
            {
                // After 28, training gains might start to decrease.
                // Example: 5% less effective per year over 28
                ageTrainingDampening = 1.0 - ((player.age - 28) * 0.05);
                ageTrainingDampening = Math.Max(0.1, ageTrainingDampening); // Minimum 10% effectiveness
            }

            // --- 7. Potential Closeness Dampening (how close attribute is to player's potential) ---
            // This is similar to GetPotentialInfluence but specific to training gains.
            // It prevents attributes from skyrocketing way past potential, even with good training.
            double potentialClosenessDampening = 1.0;
            Type playerType = player.GetType();
            System.Reflection.PropertyInfo property = playerType.GetProperty(attributeName);
            if (property != null && property.PropertyType == typeof(int))
            {
                int currentAttrValue = (int)property.GetValue(player);
                // If attribute is very close to or above player's potential, training gains significantly diminish.
                if (currentAttrValue >= player.PotentialRating - 5) // Within 5 points of potential
                {
                    potentialClosenessDampening = 0.5;
                }
                if (currentAttrValue >= player.PotentialRating) // At or above potential
                {
                    potentialClosenessDampening = 0.1; // Very hard to gain more
                }
                if (currentAttrValue >= player.PotentialRating + 5) // Significantly above potential (e.g., from good random rolls)
                {
                    potentialClosenessDampening = 0.0; // Almost impossible to gain via training
                }
            }


            // --- Combine All Influences ---
            double totalGainForPeriod = baseGain
                                        * coachInfluenceMultiplier
                                        * facilitiesInfluenceMultiplier
                                        * intensityMultiplier
                                        * moraleInfluenceMultiplier
                                        * ageTrainingDampening
                                        * potentialClosenessDampening;

            // Add a small random element for variation
            totalGainForPeriod += (Rng.NextDouble() * 0.05) - 0.025; // +/- 0.025 per period

            // Ensure gain doesn't become excessively negative (can be slightly negative if morale is terrible etc.)
            totalGainForPeriod = Math.Max(-0.5, totalGainForPeriod); // Example: can lose up to 0.5 points per period

            return totalGainForPeriod;
        }

        /// <summary>
        /// Recovers a player's current fitness over one day,
        /// influenced by their natural attributes, medical staff, and facilities.
        /// </summary>
        /// <param name="player">The player whose fitness is recovering.</param>
        /// <param name="club">The club the player belongs to (for medical resources).</param>
        /// <param name="isResting">True if the player is specifically designated for rest (e.g., rest day, recovery day from injury).</param>
        public static void RecoverFitnessDaily(Player player, Team club, bool isResting)
        {
            // Fitness only recovers if player is not fully fit
            if (player.CurrentFitness >= 100)
            {
                return;
            }

            // Base recovery for a player per day when resting
            double baseRecovery = 7.0; // Example: recover 7 points per day when fully resting

            // Player Stamina influence (higher stamina = faster recovery)
            // Assuming Stamina is 1-99. Higher stamina gives a bonus.
            // This calculates a bonus between 0 and 3.0 points.
            double staminaRecoveryBonus = (player.trueRating.Stamina / 99.0) * 3.0;

            // Medical Staff Influence (higher skill = faster recovery)
            // This calculates a bonus between 0 and 2.0 points.
            int effectiveMedicalSkill = club.GetEffectiveMedicalSkill(); // 1-20 scale
            double medicalStaffRecoveryBonus = (effectiveMedicalSkill / 20.0) * 2.0;

            // Medical Facilities Influence (higher level = faster recovery)
            // This calculates a bonus between 0 and 1.5 points.
            double medicalFacilitiesRecoveryBonus = (club.MedicalFacilitiesLevel / 5.0) * 1.5;

            // Age Influence (older players recover slower)
            double ageRecoveryPenalty = 0.0;
            if (player.age >= 30)
            {
                ageRecoveryPenalty = (player.age - 29) * 0.5; // 0.5 point penalty per year over 29
                ageRecoveryPenalty = Math.Min(5.0, ageRecoveryPenalty); // Cap penalty at 5 points
            }

            // IsResting Multiplier: Significantly boosts recovery if player is actively resting
            double restingMultiplier = isResting ? 2.0 : 1.0; // Recover twice as fast if resting

            // If player is injured, they might recover fitness slower
            double injuryRecoveryPenalty = 0.0;
            if (player.IsInjured)
            {
                // Injured players recover slower, severity could influence this penalty more granularly
                injuryRecoveryPenalty = 3.0;
            }

            // Random noise for variability
            double randomRecoveryNoise = (Rng.NextDouble() * 2.0) - 1.0; // Between -1.0 and +1.0

            double fitnessRecovery = baseRecovery
                                   + staminaRecoveryBonus
                                   + medicalStaffRecoveryBonus
                                   + medicalFacilitiesRecoveryBonus
                                   - ageRecoveryPenalty
                                   - injuryRecoveryPenalty;

            fitnessRecovery *= restingMultiplier; // Apply resting multiplier to the sum

            fitnessRecovery += randomRecoveryNoise;

            // Apply fitness gain and clamp between 0 and 100
            player.CurrentFitness = (int)Math.Min(100, player.CurrentFitness + fitnessRecovery);
            player.CurrentFitness = Math.Max(0, player.CurrentFitness); // Ensure it doesn't go below 0 (shouldn't happen with recovery)

            // Console.WriteLine($"  {player.Name} Fitness after recovery: {player.CurrentFitness}"); // For debugging
        }
    }
}
