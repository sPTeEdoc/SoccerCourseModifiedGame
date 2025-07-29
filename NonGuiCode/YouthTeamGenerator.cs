using FunnyOldGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunnyOldGameRedux.NonGuiCode
{
    class YouthTeamGenerator
    {
        public Player GenerateYouthPlayer(int currentYear, YouthAcademy youthAcademy)
        {
            Random rng = new Random();

            //string name = GenerateRandomName(); // Helper function to create names
            int age = rng.Next(16, 18); // 16 or 17

            // Determine PotentialRating first, as it influences current rating
            int potentialRating = 0;
            double potentialRoll = rng.NextDouble(); // 0.0 to 1.0

            // Example distribution for potential (adjust probabilities as needed)
            if (potentialRoll < 0.01) potentialRating = rng.Next(95, 100); // 1% chance for elite wonderkid
            else if (potentialRoll < 0.05) potentialRating = rng.Next(88, 95); // 4% chance for high potential
            else if (potentialRoll < 0.20) potentialRating = rng.Next(80, 88); // 15% chance for good potential
            else if (potentialRoll < 0.60) potentialRating = rng.Next(70, 80); // 40% chance for average potential
            else potentialRating = rng.Next(60, 70); // 40% chance for lower potential

            // Adjust potential based on academy quality (e.g., +0-5 points for good academy)
            potentialRating += (int)(youthAcademy.InfluenceFactor * rng.NextDouble() * 5);
            potentialRating = Math.Min(potentialRating, 99); // Cap at 99

            // Initial OverallRating will be significantly lower than potential
            // Can be related to potential, e.g., potential - (random_diff between 20-40)
            int initialOverallRating = potentialRating - rng.Next(20, 41);
            initialOverallRating = Math.Max(initialOverallRating, 40); // Minimum 40 overall

            // Determine Position (can be random, or weighted by academy focus)
            Enums.Positions position = PositionGenerator.SelectRandomPosition(); // Helper function
            Enums.Positions preferredPosition = position;
            List<Enums.Positions> secondaryPositions = new List<Enums.Positions>();

            // Generate individual attributes based on initialOverallRating and position bias
            // This is a more complex step, typically done by generating attribute clusters
            // and then ensuring they sum roughly to the initialOverallRating after weighting.
            // For simplicity, let's just create a Player object here.
            TeamRepository.Instance.LatestID++;

            Player newYouth = new Player(TeamRepository.Instance.LatestID, "Some Guy");
            newYouth.age = age;
            newYouth.Position = position;
            newYouth.PreferredPosition = position;
            newYouth.secondPos = secondaryPositions;
            newYouth.CreatePlayerRatings();
            newYouth.PotentialRating= potentialRating;
            //name, initialOverallRating, age, position, potentialRating, contractYears: 5.0); // Youth often sign longer initial deals

            // Now, distribute initial attributes. This would be a separate method:
            PlayerGenerator.GeneratePlayerWithQuality(newYouth, initialOverallRating); // Sets Pace, Shooting, etc.

            return newYouth;
        }
    }
}
