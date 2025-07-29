using FunnyOldGame;
// And your InjuryManager (where RollForInjury and GenerateInjury would reside)
using System;
using System.Collections.Generic;
public static class InjuryManager // Or part of a larger GameCalculations/PlayerStats class
{
    private static Random Rng = new Random();

    public static bool RollForInjury(Player player, double baseInjuryChance)
    {
        baseInjuryChance = Math.Max(0.0, Math.Min(baseInjuryChance, 1.0));

        // Toughness reduces injury chance
        double maxToughnessReduction = 0.6; // Max 60% reduction in injury chance at 100 Toughness
        double toughnessFactor = (player.InjuryResistance - 1) / 99.0 * maxToughnessReduction;

        double finalInjuryChance = baseInjuryChance * (1.0 - toughnessFactor);

        return Rng.NextDouble() < finalInjuryChance;
    }

    /// <summary>
    /// Generates a specific injury for a player based on a calculated chance.
    /// Uses the provided existing Injury class and Player properties.
    /// </summary>
    /// <param name="injuredPlayer">The player who might get injured.</param>
    /// <param name="instigator">The player who caused the tackle/foul (can be the same as injuredPlayer if self-injury).</param>
    /// <param name="finalInjuryChance">The calculated chance (0-100) of an injury occurring.</param>
    /// <param name="gameCurrentDateTime">The current date/time in your game for injury logging.</param>
    public static Injury GeneratePlayerInjury(Player injuredPlayer, Player instigator, double finalInjuryChance, DateTime gameCurrentDateTime)
    {
        // Important: Initialize ActiveInjuries if it's null when adding the first injury
        if (injuredPlayer.ActiveInjuries == null)
        {
            injuredPlayer.ActiveInjuries = new List<Injury>();
        }

        // Adjust final chance by player's injury proneness and toughness
        // Your InjuryProneness is 1-20. Let's normalize it to affect chance.
        // Higher proneness (e.g., 20) increases chance, lower (e.g., 1) decreases.
        // Assuming 10 is average, (proneness - 10) * X multiplier
        finalInjuryChance += (injuredPlayer.InjuryResistance - 10) * 1.5; // Example adjustment value

        // Assuming player.Toughness exists as 1-100 scale; higher toughness means lower chance
        // If not, you'll need to define how toughness is represented.
        // finalInjuryChance -= (injuredPlayer.Toughness - 50) * 0.3; // This line assumes player.Toughness 1-100

        finalInjuryChance = Math.Max(1, Math.Min(100, finalInjuryChance));

        if (Dice.d100() < finalInjuryChance)
        {
            string type = "Unknown Injury";
            string severity = "Minor";
            Enums.InjurySeverity injurySeverity = Enums.InjurySeverity.Minor;
            Enums.InjuryType injuryType = Enums.InjuryType.None;

            int durationDays = 0;

            // Determine injury severity based on a roll, similar to previous logic
            int severityRoll = Dice.d100();

            if (severityRoll < 30) // 0-29: Minor injuries
            {
                type = "Minor Knock";
                severity = "Minor";

                injuryType = Enums.InjuryType.MinorKnock;
                injurySeverity = Enums.InjurySeverity.Minor;

                durationDays = Dice.d4() + 1; // 2-5 days
            }
            else if (severityRoll < 55) // 30-54: Bruises/Minor Strains
            {
                type = "Bruise";
                severity = "Minor";

                injuryType = Enums.InjuryType.Bruise;
                injurySeverity = Enums.InjurySeverity.Minor;

                durationDays = Dice.d6() + 2; // 3-8 days
            }
            else if (severityRoll < 75) // 55-74: Moderate Strains/Minor Sprains
            {
                type = "Muscle Strain";
                severity = "Moderate";

                injuryType = Enums.InjuryType.MuscleStrain_Minor;
                injurySeverity = Enums.InjurySeverity.Moderate;

                durationDays = Dice.d6() * 2 + 7; // 9-19 days (1-3 weeks)
            }
            else if (severityRoll < 90) // 75-89: Major Strains/Moderate Sprains/Concussion
            {
                if (Dice.d100() < 60)
                {
                    type = "Muscle Strain";
                    severity = "Major";

                    injuryType = Enums.InjuryType.MuscleStrain_Major;
                    injurySeverity = Enums.InjurySeverity.Major;

                    durationDays = Dice.d6() * 3 + 14; // 17-32 days (3-5 weeks)
                }
                else
                {
                    type = "Concussion";
                    severity = "Moderate";

                    injuryType = Enums.InjuryType.Concussion;
                    injurySeverity = Enums.InjurySeverity.Moderate;

                    durationDays = Dice.d6() * 3 + 7; // 10-25 days (2-4 weeks)
                }
            }
            else // 90-100: Serious Injuries (Ligament, Fracture, ACL)
            {
                int seriousInjuryRoll = Dice.d100();
                if (seriousInjuryRoll < 40)
                {
                    type = "Ligament Sprain";
                    severity = "Major";

                    injuryType = Enums.InjuryType.LigamentSprain_Major;
                    injurySeverity = Enums.InjurySeverity.Major;

                    durationDays = Dice.d6() * 7 + 28; // 35-70 days (5-10 weeks)
                }
                else if (seriousInjuryRoll < 70)
                {
                    type = "Fracture";
                    severity = "Serious";

                    injuryType = Enums.InjuryType.Fracture_Major;
                    injurySeverity = Enums.InjurySeverity.Serious;

                    durationDays = Dice.d6() * 14 + 60; // 74-144 days (2.5-5 months)
                }
                else
                {
                    type = "Torn ACL";
                    severity = "Career-Threatening";

                    injuryType = Enums.InjuryType.TornACL;
                    injurySeverity = Enums.InjurySeverity.CareerThreatening;

                    durationDays = Dice.d6() * 30 + 180; // 210-360 days (7-12 months)
                }
            }

            // Adjust duration based on player's healing rate
            // Assuming player.HealingRate exists as 1-100 scale; higher healing rate means faster recovery
            // If not, you'll need to define how healing rate is represented.
            // For example, if player.HealingRate is 1-20 like InjuryProneness:
            // durationDays -= (injuredPlayer.HealingRate - 10) * durationDays / 20; // More healing rate, less duration
            // For now, I'll comment out the HealingRate adjustment as it's an unknown
            // durationDays -= (injuredPlayer.HealingRate - 50) * durationDays / 100;
            durationDays = Math.Max(1, durationDays); // Minimum 1 day recovery

            // Create the injury using your constructor
            Injury newInjury = new Injury(type, severity, durationDays, injuryType, injurySeverity);
            newInjury.DateOccurred = gameCurrentDateTime; // Set the current game date

            injuredPlayer.ActiveInjuries.Add(newInjury);
            injuredPlayer.InjuryHistory.Add(newInjury); // Add to history immediately

            //ShowMessage(string.Format("{0} ({1}) is injured! {2} {3} ({4} days).",
            //                            injuredPlayer.fullName, injuredPlayer.Team.Name, newInjury.Severity, newInjury.Type, newInjury.RemainingDurationDays));

            // Consider adding logic here for lingering effects for serious injuries
            // For example:
            if (newInjury.Severity == "Serious" || newInjury.Severity == "Career-Threatening")
            {
                // Example: A serious injury might give a minor, permanent reduction in a key attribute
                if (!newInjury.LingeringEffects.ContainsKey("Pace"))
                {
                    newInjury.LingeringEffects["Pace"] = -(Dice.d4() * 0.5); // -0.5 to -2.0 pace
                }
                //ShowMessage(string.Format("Warning: {0}'s injury may have lingering effects on stats.", injuredPlayer.fullName));
            }

            injuredPlayer.ResetInjuryStatus();

            return newInjury;

            // Future: Handle immediate in-match substitutions if the injury is severe enough.
        }
        return null;
    }

    /// <summary>
    /// Generates a random injury based on factors like training intensity and player fitness.
    /// </summary>
    /// <param name="player">The player who got injured.</param>
    /// <param name="intensityAtTimeOfInjury">The training intensity when the injury occurred.</param>
    /// <param name="fitnessAtTimeOfInjury">The player's fitness level when the injury occurred.</param>
    /// <returns>A new Injury object.</returns>
    public static Injury GenerateRandomInjury(Player player, Enums.TrainingIntensity intensityAtTimeOfInjury, int fitnessAtTimeOfInjury)
    {
        string injuryType = "Muscle Strain"; // Default
        string severity = "Minor";          // Default
        int durationDays = 7;               // Default: 1 week

        // --- Determine Severity based on Intensity, Fitness, and Player's InjuryProneness ---
        // Higher intensity, lower fitness, and higher proneness increase severity chance.
        double severityRoll = Rng.NextDouble(); // 0.0 to 1.0

        double severityFactor = 0.0;
        switch (intensityAtTimeOfInjury)
        {
            case Enums.TrainingIntensity.Light: severityFactor += 0.1; break;
            case Enums.TrainingIntensity.Normal: severityFactor += 0.2; break;
            case Enums.TrainingIntensity.Heavy: severityFactor += 0.4; break;
            case Enums.TrainingIntensity.VeryHeavy: severityFactor += 0.6; break;
        }

        // Fitness contribution: Lower fitness means higher severity factor
        if (fitnessAtTimeOfInjury < 20) severityFactor += 0.5; // Very low fitness
        else if (fitnessAtTimeOfInjury < 40) severityFactor += 0.3;
        else if (fitnessAtTimeOfInjury < 60) severityFactor += 0.1;

        // Injury Proneness contribution: Higher proneness increases severity factor
        severityFactor += (player.InjuryResistance / 20.0) * 0.2; // Max 0.2 contribution from proneness

        // Clamp severityFactor to prevent going over the top
        severityFactor = Math.Min(1.0, severityFactor);

        Enums.InjurySeverity injurySeverity = Enums.InjurySeverity.Minor;
        Enums.InjuryType injuryTypeEnum = Enums.InjuryType.MuscleStrain_Minor;

        // Assign Severity and influence duration
        if (severityRoll < (0.05 * severityFactor)) // Small chance for Serious/Career-Ending
        {
            severity = "Career-Ending"; // Very rare!

            injurySeverity = Enums.InjurySeverity.CareerThreatening;
            injuryTypeEnum = Enums.InjuryType.TornACL;

            durationDays = Rng.Next(180, 730); // 6 months to 2 years
            injuryType = "ACL Rupture"; // Example severe injury
            // Add lingering effects for career-ending:
            Injury injury = new Injury(injuryType, severity, durationDays, injuryTypeEnum, injurySeverity);
            injury.LingeringEffects["OverallRating"] = -5.0 - (Rng.NextDouble() * 5.0); // e.g., -5 to -10
            injury.LingeringEffects["InjuryProneness"] = 5.0 + (Rng.NextDouble() * 5.0); // +5 to +10
            return injury;

        }
        else if (severityRoll < (0.20 * severityFactor)) // Moderate chance for Serious
        {
            severity = "Serious";
            durationDays = Rng.Next(60, 180); // 2 to 6 months
            // Randomize serious injury types
            string[] seriousTypes = { "Fractured Bone", "Torn Ligament", "Meniscus Tear" };
            injuryType = seriousTypes[Rng.Next(seriousTypes.Length)];

            injurySeverity = Enums.InjurySeverity.Serious;
            injuryTypeEnum = Enums.InjuryType.Fracture_Major;

            // Add some lingering effects for serious injuries
            Injury injury = new Injury(injuryType, severity, durationDays, injuryTypeEnum, injurySeverity);
            injury.LingeringEffects["OverallRating"] = -1.0 - (Rng.NextDouble() * 2.0); // e.g., -1 to -3
            injury.LingeringEffects["InjuryProneness"] = 2.0 + (Rng.NextDouble() * 3.0); // +2 to +5
            return injury;
        }
        else if (severityRoll < (0.50 * severityFactor)) // Higher chance for Moderate
        {
            severity = "Moderate";

            injurySeverity = Enums.InjurySeverity.Moderate;
            injuryTypeEnum = Enums.InjuryType.LigamentSprain_Minor;

            durationDays = Rng.Next(14, 60); // 2 weeks to 2 months
            // Randomize moderate injury types
            string[] moderateTypes = { "Hamstring Strain", "Groin Strain", "Ankle Sprain", "Knee Sprain" };
            injuryType = moderateTypes[Rng.Next(moderateTypes.Length)];
        }
        else // Most common: Minor
        {
            severity = "Minor";

            injurySeverity = Enums.InjurySeverity.Minor;
            injuryTypeEnum = Enums.InjuryType.Bruise;

            durationDays = Rng.Next(3, 14); // 3 days to 2 weeks
            // Randomize minor injury types
            string[] minorTypes = { "Knock", "Bruise", "Minor Muscle Fatigue", "Twisted Ankle (Minor)" };
            injuryType = minorTypes[Rng.Next(minorTypes.Length)];
        }

        // Apply a slight variance to duration based on player's overall durability (Stamina, Strength)
        // Stronger/more durable players might recover slightly faster for the same injury.
        durationDays = (int)Math.Round(durationDays * (1.0 - (player.trueRating.Stamina / 200.0))); // Max 50% reduction for 100 stamina
        durationDays = Math.Max(1, durationDays); // Minimum 1 day duration

        return new Injury(injuryType, severity, durationDays, injuryTypeEnum, injurySeverity);
    }
}