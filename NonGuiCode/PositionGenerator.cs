using FunnyOldGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunnyOldGameRedux.NonGuiCode
{
    public static class PositionGenerator
    {
        private static readonly Random Rng = new Random();

        // Define positions and their relative weights for generation
        // Weights are arbitrary, higher number means more common.
        // You'll need to fine-tune these based on your desired player distribution.
        private static readonly Dictionary<Enums.Positions, int> PositionWeights = new Dictionary<Enums.Positions, int>()
        {
            // Attack
            {Enums.Positions.Striker, 10},
            {Enums.Positions.LeftWingForward, 8},
            {Enums.Positions.RightWingForward, 8},
            {Enums.Positions.CentralAttackingMidfielder, 7},

            // Midfield
            {Enums.Positions.CentralMidfielder, 15}, // More common, versatile
            {Enums.Positions.CentralDefendingMidfielder, 12},
            {Enums.Positions.LeftMidfielder, 6},
            {Enums.Positions.RightMidfielder, 6},

            // Defense
            {Enums.Positions.CenterBack, 14}, // Common defensive role
            {Enums.Positions.RightBack, 9},
            {Enums.Positions.LeftBack, 9},

            // Goalkeepers
            {Enums.Positions.Goalkeeper,10}
            // Note: Goalkeeper is generally handled separately as they have different stats.
        };

        // Pre-calculate the total weight for efficiency
        private static readonly int TotalWeight = PositionWeights.Values.Sum();

        /// <summary>
        /// Selects a random outfield position based on predefined weights.
        /// </summary>
        /// <returns>A string representing the selected position (e.g., "Striker", "Center Back").</returns>
        public static Enums.Positions SelectRandomPosition()
        {
            // Generate a random number between 0 (inclusive) and TotalWeight (exclusive)
            int randomNumber = Rng.Next(TotalWeight);

            int currentWeightSum = 0;
            foreach (var entry in PositionWeights)
            {
                currentWeightSum += entry.Value;
                if (randomNumber < currentWeightSum)
                {
                    return entry.Key; // This is the selected position
                }
            }

            // Fallback (should ideally not be reached if weights sum correctly)
            return Enums.Positions.CentralMidfielder; // Or throw an exception
        }
    }
}
