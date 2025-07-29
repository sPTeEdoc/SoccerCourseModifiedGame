using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunnyOldGameRedux.NonGuiCode
{
    // --- PerformanceBonus Class (Remains the same) ---
    public class PerformanceBonus
    {
        public string Description { get; set; } // E.g., "Goal Scored Bonus", "Clean Sheet Bonus"
        public decimal Amount { get; set; }     // How much is paid for achieving it
        public int Target { get; set; }         // How many goals/clean sheets etc., for a payout
        public string StatisticTracked { get; set; } // E.g., "Goals", "CleanSheets", "Appearances"
        public bool IsAchieved { get; set; }   // Whether this bonus has been met in the current period (e.g., season)

        public PerformanceBonus(string description, decimal amount, int target, string statisticTracked)
        {
            Description = description;
            Amount = amount;
            Target = target;
            StatisticTracked = statisticTracked;
            IsAchieved = false;
        }
    }
}
