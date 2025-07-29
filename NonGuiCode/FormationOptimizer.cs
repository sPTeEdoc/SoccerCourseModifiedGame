using FunnyOldGame;
using FunnyOldGameRedux;
using System;
using System.Collections.Generic;

// Contains the algorithm to optimize the formation based on the roster.
public class FormationOptimizer
{
    private static FormationOptimizer m_instance = null;

    // The ranked list of formations, ordered from most preferred to least preferred.
    private readonly List<SoccerFormation> _rankedFormationsAllPositionsFactored;

    // The ranked list of formations, ordered from most preferred to least preferred.
    private readonly List<SoccerFormation> _rankedFormationsBasicFormationsFactored;

    /// <summary>
    /// Initializes a new instance of the FormationOptimizer with the predefined
    /// ranked list of soccer formations, each with specific positional requirements.
    /// The specific D-M-F breakdown for each formation is an interpretation based on
    /// common tactical setups, ensuring each formation accounts for exactly 10 outfield players.
    /// </summary>
    public FormationOptimizer()
    {
        _rankedFormationsAllPositionsFactored = new List<SoccerFormation>
        {
            // Formation: (Name, LB, RB, CB, AM, CM, LM, RM, CDM, LWF, RWF, ST)
            // Total players for each formation must sum to 10 (outfield players)

            // 1. 4-3-3: Balanced, often with wingers.
            new SoccerFormation("4-3-3", 1, 1, 2, 0, 2, 0, 0, 1, 1, 1, 1), // (4 Def, 3 Mid, 3 Fwd)

            // 2. 3-4-2-1: Three at the back, strong midfield with attacking mids behind a lone striker.
            new SoccerFormation("3-4-2-1", 0, 0, 3, 2, 2, 1, 1, 0, 0, 0, 1), // (3 Def, 6 Mid, 1 Fwd)

            // 3. 3-5-2: Three at the back, dominant midfield, two strikers.
            new SoccerFormation("3-5-2", 0, 0, 3, 1, 2, 1, 1, 1, 0, 0, 2), // (3 Def, 5 Mid, 2 Fwd)

            // 4. 4-4-1-1: Four at the back, flat midfield, attacking midfielder supporting a striker.
            new SoccerFormation("4-4-1-1", 1, 1, 2, 1, 2, 1, 1, 0, 0, 0, 1), // (4 Def, 5 Mid, 1 Fwd)

            // 5. 4-4-2: Classic formation with two strikers.
            new SoccerFormation("4-4-2", 1, 1, 2, 0, 2, 1, 1, 0, 0, 0, 2), // (4 Def, 4 Mid, 2 Fwd)

            // 6. 4-2-3-1: Four at the back, two holding midfielders, three attacking midfielders/wingers, one striker.
            new SoccerFormation("4-2-3-1", 1, 1, 2, 1, 0, 1, 1, 2, 0, 0, 1), // (4 Def, 5 Mid, 1 Fwd)

            // 7. 4-1-2-1-2 (Diamond): Four at the back, a defensive midfielder, two central, one attacking, two strikers.
            new SoccerFormation("4-1-2-1-2", 1, 1, 2, 1, 2, 0, 0, 1, 0, 0, 2), // (4 Def, 4 Mid, 2 Fwd)

            // 8. 3-4-3: Three at the back, four in midfield (often wide), three attacking forwards.
            new SoccerFormation("3-4-3", 0, 0, 3, 0, 2, 1, 1, 0, 1, 1, 1), // (3 Def, 4 Mid, 3 Fwd)

            // 9. 4-2-1-3: Four at the back, two holding midfielders, one attacking midfielder, three true forwards.
            new SoccerFormation("4-2-1-3", 1, 1, 2, 1, 0, 0, 0, 2, 1, 1, 1)  // (4 Def, 3 Mid, 3 Fwd)
        };

        _rankedFormationsBasicFormationsFactored = new List<SoccerFormation>
        {
            // 1. 4-3-3: 4 Defenders, 3 Midfielders, 3 Forwards
            new SoccerFormation("4-3-3", 4, 3, 3),
            // 2. 3-4-2-1: Typically 3 Defenders, 4 central/wide Midfielders,
            //    2 Attacking Midfielders, 1 Striker.
            //    Here, attacking midfielders are counted as part of the midfield group
            //    to reach 10 outfield players (3D + 6M + 1F).
            new SoccerFormation("3-4-2-1", 3, 6, 1),
            // 3. 3-5-2: 3 Defenders, 5 Midfielders, 2 Forwards
            new SoccerFormation("3-5-2", 3, 5, 2),
            // 4. 4-4-1-1: 4 Defenders, 4 Midfielders, 1 Attacking Midfielder (behind striker),
            //    1 Striker.
            //    Attacking midfielder counted with midfield (4D + 5M + 1F).
            new SoccerFormation("4-4-1-1", 4, 5, 1),
            // 5. 4-4-2: 4 Defenders, 4 Midfielders, 2 Forwards
            new SoccerFormation("4-4-2", 4, 4, 2),
            // 6. 4-2-3-1: 4 Defenders, 2 Defensive Midfielders, 3 Attacking Midfielders/Wingers,
            //    1 Striker.
            //    All central/attacking/wide midfielders counted as midfield (4D + 5M + 1F).
            new SoccerFormation("4-2-3-1", 4, 5, 1),
            // 7. 4-1-2-1-2: 4 Defenders, 1 Defensive Midfielder, 2 Central Midfielders,
            //    1 Attacking Midfielder, 2 Strikers. (Diamond midfield)
            //    All central/attacking/defensive midfielders counted as midfield (4D + 4M + 2F).
            new SoccerFormation("4-1-2-1-2", 4, 4, 2),
            // 8. 3-4-3: 3 Defenders, 4 Midfielders, 3 Forwards
            new SoccerFormation("3-4-3", 3, 4, 3),
            // 9. 4-2-1-3: 4 Defenders, 2 Defensive Midfielders, 1 Attacking Midfielder, 3 Forwards.
            //    All central/attacking/defensive midfielders counted as midfield (4D + 3M + 3F).
            new SoccerFormation("4-2-1-3", 4, 3, 3)
        };
    }

    public static FormationOptimizer Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new FormationOptimizer();
            }
            return m_instance;
        }
    }

    /// <summary>
    /// Determines the best-ranked formation a team should use based on its roster composition.
    /// The algorithm prioritizes higher-ranked formations and checks if the roster has
    /// enough players in the general defender, midfielder, and forward categories to meet
    /// the formation's requirements.
    /// </summary>
    /// <param name="roster">The team's roster composition with specific player counts.</param>
    /// <returns>The name of the best-suited formation, or null if no formation can be matched
    /// or if the roster has insufficient total outfield players.</returns>
    public string DetermineBestFormationBasicRostersFactored(List<Player> roster)
    {
        // Get the total counts of players available from the roster for each general category.
        int availableDefenders = 0;
        int availableMidfielders = 0;
        int availableForwards = 0;
        foreach (Player p in roster)
            if (TeamRepository.Instance.IsDefender(p.PreferredPosition))
            {
                availableDefenders++;
            }
            else if (TeamRepository.Instance.IsForward(p.PreferredPosition))
            {
                availableForwards++;
            }
            else if (TeamRepository.Instance.IsMidfielder(p.PreferredPosition))
            {
                availableMidfielders++;
            }

        int totalOutfieldPlayers = availableDefenders + availableMidfielders + availableForwards;

        // A standard soccer formation requires 10 outfield players (excluding the goalkeeper).
        // If the roster doesn't meet this minimum, no formation can be fully supported.
        if (totalOutfieldPlayers < 10)
        {
            throw new Exception("Warning: Roster does not have enough total outfield players (minimum 10 required for a complete formation).");
            //Console.WriteLine($"Available: Defenders={availableDefenders}, Midfielders={availableMidfielders}, Forwards={availableForwards}. Total={totalOutfieldPlayers}.");
            //return null; // Indicate that no suitable formation can be found.
        }

        // Iterate through the ranked formations in the order they were provided.
        foreach (var formation in _rankedFormationsBasicFormationsFactored)
        {
            // Check if the available players satisfy the requirements for the current formation.
            // The roster must have AT LEAST the number of players required for each category.
            if (availableDefenders >= formation.Defenders &&
                availableMidfielders >= formation.Midfielders &&
                availableForwards >= formation.Forwards)
            {
                // Since we are iterating through the ranked list, the first match found
                // is the highest-ranked suitable formation.
                return formation.Name;
            }
            else
            {
                //Console.WriteLine($"  Roster cannot fully support {formation.Name}. Insufficient players in one or more categories.");
            }
        }

        // If the loop completes, it means no formation from the ranked list could be matched
        // with the given roster composition.
        //Console.WriteLine("\nNo suitable formation found for the given roster composition based on the ranked list.");
        if (availableForwards <= 1)
        {
            return "4-4-1-1";
        }
        else if (availableForwards <= 3)
        {
            return "3-4-2-1";
        }
        if (availableForwards <= 4)
        {
            return "4-4-2";
        }
        else
        {
            return "4-3-3";
        }
    }

    /// <summary>
    /// Determines the best-ranked formation a team should use based on its roster's
    /// specific player composition. The algorithm prioritizes higher-ranked formations
    /// and checks if the roster has enough players for each individual required position.
    /// </summary>
    /// <param name="roster">The team's roster composition with specific player counts.</param>
    /// <returns>The name of the best-suited formation, or null if no formation can be matched
    /// or if the roster has insufficient total outfield players.</returns>
    public string DetermineBestFormationAllPositionsFactored(List<Player> roster)
    {
        List<Player> outfielders = new List<Player>();
        int[] positionsCount = new int[11];
        foreach (Player p in roster)
            if (!TeamRepository.Instance.IsGoalKeeper(p.PreferredPosition))
            {
                outfielders.Add(p);
                if (p.PreferredPosition == Enums.Positions.LeftBack)
                    positionsCount[0]++;
                else if (p.PreferredPosition == Enums.Positions.RightBack)
                    positionsCount[1]++;
                else if (p.PreferredPosition == Enums.Positions.CenterBack)
                    positionsCount[2]++;
                else if (p.PreferredPosition == Enums.Positions.CentralAttackingMidfielder)
                    positionsCount[3]++;
                else if (p.PreferredPosition == Enums.Positions.CentralMidfielder)
                    positionsCount[4]++;
                else if (p.PreferredPosition == Enums.Positions.LeftMidfielder)
                    positionsCount[5]++;
                else if (p.PreferredPosition == Enums.Positions.RightMidfielder)
                    positionsCount[6]++;
                else if (p.PreferredPosition == Enums.Positions.CentralDefendingMidfielder)
                    positionsCount[7]++;
                else if (p.PreferredPosition == Enums.Positions.LeftWingForward)
                    positionsCount[8]++;
                else if (p.PreferredPosition == Enums.Positions.RightWingForward)
                    positionsCount[9]++;
                else
                    positionsCount[10]++;
            }
        // Calculate the total number of outfield players from the specific position counts.
        int totalOutfieldPlayers = outfielders.Count;

        Dictionary<Enums.Positions, int> PlayerCounts = new Dictionary<Enums.Positions, int>
        {
            { Enums.Positions.LeftBack, positionsCount[0] },
            { Enums.Positions.RightBack, positionsCount[1] },
            { Enums.Positions.CenterBack, positionsCount[2] },
            { Enums.Positions.CentralAttackingMidfielder, positionsCount[3] },
            { Enums.Positions.CentralMidfielder, positionsCount[4] },
            { Enums.Positions.LeftMidfielder, positionsCount[5] },
            { Enums.Positions.RightMidfielder, positionsCount[6] },
            { Enums.Positions.CentralDefendingMidfielder, positionsCount[7] },
            { Enums.Positions.LeftWingForward, positionsCount[8] },
            { Enums.Positions.RightWingForward, positionsCount[9] },
            { Enums.Positions.Striker, positionsCount[10] }
        };

        // A standard soccer formation requires 10 outfield players (excluding the goalkeeper).
        // If the roster doesn't meet this minimum, no formation can be fully supported.
        if (totalOutfieldPlayers < 10)
        {
            throw new Exception("You don't have enough outfielders.");
            //Console.WriteLine("Warning: Roster does not have enough total outfield players (minimum 10 required for a complete formation).");
            //Console.WriteLine($"Available Players: {string.Join(", ", roster.PlayerCounts.Select(kv => $"{kv.Key}={kv.Value}"))}. Total={totalOutfieldPlayers}.");
            //return null; // Indicate that no suitable formation can be found.
        }

        //Console.WriteLine($"\n--- Analyzing Roster ---");
        //Console.WriteLine($"Available Players (Specific Positions): {string.Join(", ", roster.PlayerCounts.Select(kv => $"{kv.Key}={kv.Value}"))}");
        //Console.WriteLine($"Total Outfield Players: {totalOutfieldPlayers}");
        //Console.WriteLine("------------------------");


        // Iterate through the ranked formations in the order they were provided (highest preference first).
        foreach (var formation in _rankedFormationsAllPositionsFactored)
        {
            //Console.WriteLine($"\nChecking Formation: {formation.Name}");
            // Display only positions that actually require players for readability
            //Console.WriteLine($"  Required: {string.Join(", ", formation.RequiredPlayerCounts.Where(kv => kv.Value > 0).Select(kv => $"{kv.Key}={kv.Value}"))}");

            bool canSupportFormation = true;
            // Iterate through each specific position required by the current formation
            foreach (var requiredPosition in formation.RequiredPlayerCounts)
            {
                Enums.Positions positionName = requiredPosition.Key;
                int requiredCount = requiredPosition.Value;
                int availableCount = 0; // Initialize available count to 0

                // Use TryGetValue to safely get the count, returning 0 if the key is not found
                if (!PlayerCounts.TryGetValue(positionName, out availableCount))
                {
                    // If TryGetValue returns false, it means the key doesn't exist, so availableCount remains 0.
                    // This is robust, though current Roster ensures all are present.
                    availableCount = 0; // Explicitly set to 0 just in case
                }

                // If the formation requires players for this position, check if the roster has enough.
                if (availableCount < requiredCount)
                {
                    canSupportFormation = false; // Roster cannot support this formation
                    //Console.WriteLine($"  Insufficient {positionName} (Needed: {requiredCount}, Available: {availableCount})");
                    break; // No need to check other positions for this formation, move to next formation
                }
            }

            if (canSupportFormation)
            {
                //Console.WriteLine($"  Match found! Roster can support {formation.Name}.");
                // Since _rankedFormations is ordered, the first match is the best-ranked suitable formation.
                return formation.Name;
            }
            else
            {
                //Console.WriteLine($"  Roster cannot fully support {formation.Name} due to specific position shortages.");
            }
        }

        // If the loop completes, it means no formation from the ranked list could be matched
        // with the given roster's specific player composition.
        //Console.WriteLine("\nNo suitable formation found for the given roster composition based on the ranked list.");
        return DetermineBestFormationBasicRostersFactored(roster);
    }
}

