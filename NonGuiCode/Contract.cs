using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunnyOldGameRedux.NonGuiCode
{
    // --- NEW: Contract Class ---
    // This class encapsulates all the details of a player's contract.
    public class Contract
    {
        public double YearsRemaining { get; set; }
        public double WeeklyWage { get; set; }
        public double SigningBonus { get; set; }
        public double? MinimumFeeReleaseClause { get; set; } // Nullable decimal for optional clause
        public List<PerformanceBonus> PerformanceBonuses { get; set; }
        public double LoyaltyBonus { get; set; }
        public double AgentFee { get; set; }

        public Contract()
        {
            SigningBonus = 0; // Keep default parameters if your C# version supports them (C# 4.0+)
            MinimumFeeReleaseClause = null;
            LoyaltyBonus = 0;
            AgentFee = 0;
            PerformanceBonuses = new List<PerformanceBonus>(); // Always initialize the list to an empty one
        }

        // Constructor for the Contract class - now handles defaults
        public Contract(double yearsRemaining, double weeklyWage,
                        double signingBonus = 0, // Keep default parameters if your C# version supports them (C# 4.0+)
                        double? minimumFeeReleaseClause = null,
                        double loyaltyBonus = 0,
                        double agentFee = 0)
        {
            YearsRemaining = yearsRemaining;
            WeeklyWage = weeklyWage;
            SigningBonus = signingBonus; // Value comes from parameter, defaults to 0m if not provided
            MinimumFeeReleaseClause = minimumFeeReleaseClause; // Value comes from parameter, defaults to null if not provided
            LoyaltyBonus = loyaltyBonus; // Value comes from parameter, defaults to 0m if not provided
            AgentFee = agentFee; // Value comes from parameter, defaults to 0m if not provided

            PerformanceBonuses = new List<PerformanceBonus>(); // Always initialize the list to an empty one
        }

        // Method to decrement contract years
        public void DecrementYear()
        {
            YearsRemaining = Math.Max(0, YearsRemaining - 1);
        }
    }
}
