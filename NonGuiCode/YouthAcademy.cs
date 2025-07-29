using FunnyOldGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunnyOldGameRedux.NonGuiCode
{

    public class YouthAcademy
    {
        public string Name { get; set; } // E.g., "La Masia", "Hale End"

        public List<Player> YouthTeam { get; set; }

        // The descriptive quality level of the academy
        public Enums.YouthAcademyTier Tier { get; set; }

        // --- New Properties ---
        /// <summary>
        /// Level of the academy's physical facilities (e.g., training pitches, gyms, medical).
        /// Influences player development speed and potentially injury risk. (e.g., 1-100)
        /// </summary>
        public int FacilitiesLevel { get; set; }

        /// <summary>
        /// Level of the academy's scouting network.
        /// Primarily influences the potential and initial quality of generated youth players. (e.g., 1-100)
        /// </summary>
        public int ScoutingNetworkLevel { get; set; }

        /// <summary>
        /// Overall quality/skill of the youth coaching staff.
        /// Directly influences the rate of attribute progression for youth players. (e.g., 1-100)
        /// </summary>
        public int YouthCoachQuality { get; set; }

        /// <summary>
        /// Annual budget allocated to the youth academy.
        /// Can influence staffing levels, facility upgrades, and scouting reach. (e.g., in game currency)
        /// </summary>
        public decimal BudgetAllocation { get; set; } // Using decimal for currency to avoid floating point inaccuracies

        // This property calculates the InfluenceFactor based on the Tier
        // It remains as previously defined, providing a single numeric value for tier impact
        public double InfluenceFactor
        {
            get
            {
                double factor;
                switch (Tier) // Traditional switch statement
                {
                    case Enums.YouthAcademyTier.Basic:
                        factor = 0.1;
                        break;
                    case Enums.YouthAcademyTier.Average:
                        factor = 0.5;
                        break;
                    case Enums.YouthAcademyTier.Good:
                        factor = 1.0;
                        break;
                    case Enums.YouthAcademyTier.Excellent:
                        factor = 1.5;
                        break;
                    case Enums.YouthAcademyTier.Elite:
                        factor = 2.0;
                        break;
                    default:
                        factor = 0.0; // Default or error case
                        break;
                }
                return factor;
            }
        }

        public YouthAcademy()
        {
        }

        // Constructor (Optional: to easily initialize these properties when creating an academy)
        public YouthAcademy(string name, Enums.YouthAcademyTier tier, int facilitiesLevel, int scoutingNetworkLevel, int youthCoachQuality, decimal budgetAllocation)
        {
            Name = name;
            Tier = tier;
            FacilitiesLevel = facilitiesLevel;
            ScoutingNetworkLevel = scoutingNetworkLevel;
            YouthCoachQuality = youthCoachQuality;
            BudgetAllocation = budgetAllocation;
        }
    }
}
