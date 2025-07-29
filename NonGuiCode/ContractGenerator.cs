using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunnyOldGameRedux.NonGuiCode
{
    using FunnyOldGame; // For extension methods like Average, if you use them for player stats
    using System;
    using System.Collections.Generic; // For List<T> if you want to initialize PerformanceBonuses
    using System.Linq;

    // Assuming your Player class now has the CurrentContract property
    // and your Contract class is defined as we just refactored it.

    // You might put this method in a PlayerFactory, YouthAcademy, or PlayerGenerator class.
    public static class ContractGenerator
    {
        private static readonly Random Rng = new Random();

        /// <summary>
        /// Generates an initial contract for a newly created youth player.
        /// </summary>
        /// <param name="playerOverall">The player's initial overall rating.</param>
        /// <param name="playerPotential">The player's potential rating.</
        /// <param name="playerAge">The player's age.</param>
        /// <param name="clubYouthAcademyQualityFactor">A numerical factor representing the club's academy quality (e.g., from YouthAcademy.InfluenceFactor).</param>
        /// <returns>A new Contract object with initial terms.</returns>
        public static Contract GenerateContract(
            Player player,
            int playerOverall,
            int playerPotential,
            int playerAge,
            double clubYouthAcademyQualityFactor) // Passed from the YouthAcademy object
        {
            // --- 1. Determine Contract Length (Years) ---
            // Younger, higher potential players get longer youth contracts for club stability.
            double contractYears = 3.0; // Base youth contract length
            if (playerPotential >= 85) contractYears += Rng.NextDouble() * 1.0; // Highly potential players get slightly longer deals
            if (playerAge <= 17) contractYears += 1.0; // Very young players might sign for an extra year

            contractYears = Math.Round(contractYears); // Round to nearest whole year for simplicity

            double finalWeeklyWage = TeamRepository.Instance.CalculateSalary(player);

            //// --- 2. Determine Weekly Wage ---
            //// Wage is influenced by OVR, Potential, Age, and Club quality.
            //decimal baseWage = 500m; // Starting point for a very young, low OVR youth player (e.g., 500 currency units per week)

            //// Influence by Overall/Potential: Higher stats = higher wage
            //decimal wageMultiplierByRating = (decimal)(playerOverall * 0.5 + playerPotential * 0.7); // Potential weighs more for future earnings
            //baseWage += wageMultiplierByRating * 10m; // Example: each combined rating point adds 10 currency units

            //// Influence by Age: Older youth players command slightly more
            //baseWage += (playerAge - 16) * 50m; // Example: +50 per year above 16

            //// Influence by Club Youth Academy Quality: Better academy might mean slightly lower wage demands
            //// Or it could mean higher because they expect more. Let's assume higher quality means
            //// they can demand a bit more, but also might be willing to compromise for prestige.
            //// For simplicity here, let's say a better academy allows *you* to offer slightly less as they value development.
            //baseWage -= (decimal)(clubYouthAcademyQualityFactor * 50m); // Example: Elite academy might reduce wage by 100 currency units.
            //baseWage = Math.Max(100m, baseWage); // Minimum wage

            // Add some random variance to the wage
            //decimal wageVariance = (decimal)(Rng.NextDouble() * 100m - 50m); // +/- 50 currency units
            //decimal finalWeeklyWage = baseWage + wageVariance;
            //finalWeeklyWage = Math.Max(100m, finalWeeklyWage); // Ensure wage doesn't go too low

            // --- 3. Determine Signing Bonus (Optional for Youth) ---
            // Youth players might not get large signing bonuses, or only if highly sought after.
            double signingBonus = 0;
            if (playerPotential >= 88 && Rng.NextDouble() > 0.5) // Small chance for high potential players
            {
                if (playerAge <= 17)
                    signingBonus = (double)(Rng.Next(1000, 5000)); // Small bonus
                else
                    signingBonus = (double)(Rng.Next(10000, 25000));
            }

            // --- 4. Initialize Other Contract Terms ---
            // Youth contracts typically don't have release clauses or complex bonuses initially.
            double? minimumFeeReleaseClause = null;
            double loyaltyBonus = 0;
            double agentFee = signingBonus * 0.1; // Example: Agent gets 10% of signing bonus

            // --- 5. Create and Return the Contract Object ---
            Contract initialContract = new Contract(
                yearsRemaining: contractYears,
                weeklyWage: finalWeeklyWage,
                signingBonus: signingBonus,
                minimumFeeReleaseClause: minimumFeeReleaseClause,
                loyaltyBonus: loyaltyBonus,
                agentFee: agentFee
            );

            // You could add some simple performance bonuses here if desired, e.g., for appearances
            // initialContract.PerformanceBonuses.Add(new PerformanceBonus("Appearance Bonus", 100m, 10, "Appearances"));

            return initialContract;
        }
    }
}
