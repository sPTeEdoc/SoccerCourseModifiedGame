using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunnyOldGame
{
    public class Referee
    {
        public string firstName;
        public string lastName;
        public string fullName;
        public Enums.RefAttitude attitudeToHomeTeam;
        public Enums.RefAttitude attitudeToVisitingTeam;
        public List<Player> yellowCardBook = new List<Player>();
        public List<Player> redCardBook = new List<Player>();
        public double strictnessForGameCalculations { get; set; }
        public Enums.RefereeStrictness CurrentRefereeStrictness;

        public Referee(string firstName, string lastName)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            fullName = firstName + " " + this.lastName;

            CurrentRefereeStrictness = Enums.RefereeStrictness.Normal;

            ResetAttitude();
        }

        public void ResetAttitude()
        {
            attitudeToHomeTeam = Enums.RefAttitude.Neutral;
            attitudeToVisitingTeam = Enums.RefAttitude.Neutral;
        }

        public void AddPlayerToBookYellowCard(Player p)
        {
            this.yellowCardBook.Add(p);
        }

        public void AddPlayerToBookRedCard(Player p)
        {
            this.redCardBook.Add(p);
        }

        public Boolean PlayerAlreadyBookedYellowCard(Player p)
        {
            return this.yellowCardBook.Contains(p);
        }

        public Boolean PlayerAlreadyBookedRedCard(Player p)
        {
            return this.redCardBook.Contains(p);
        }
    }
}
