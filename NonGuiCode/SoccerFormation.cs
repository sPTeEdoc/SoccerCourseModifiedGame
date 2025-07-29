using FunnyOldGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    public class SoccerFormation
    {
        public string Name { get; set; }
        // A dictionary to hold the required count of players for each specific position.
        public Dictionary<Enums.Positions, int> RequiredPlayerCounts { get; set; }


        /// <summary>
        /// Initializes a new instance of the SoccerFormation class.
        /// Defines the precise number of players needed for each specific outfield position.
        /// </summary>
        /// <param name="name">The common name of the formation (e.g., "4-3-3").</param>
        /// <param name="leftBack">Required Left Backs.</param>
        /// <param name="rightBack">Required Right Backs.</param>
        /// <param name="centerBack">Required Center Backs.</param>
        /// <param name="attackingMidfielder">Required Attacking Midfielders.</param>
        /// <param name="centralMidfielder">Required Central Midfielders.</param>
        /// <param name="leftMidfielder">Required Left Midfielders (Wingers/Wide Midfielders).</param>
        /// <param name="rightMidfielder">Required Right Midfielders (Wingers/Wide Midfielders).</param>
        /// <param name="centralDefendingMidfielder">Required Central Defending Midfielders.</param>
        /// <param name="leftWingForward">Required Left Wing Forwards.</param>
        /// <param name="rightWingForward">Required Right Wing Forwards.</param>
        /// <param name="striker">Required Strikers.</param>
        public SoccerFormation(string name,
            int leftBack, int rightBack, int centerBack,
            int attackingMidfielder, int centralMidfielder, int leftMidfielder,
            int rightMidfielder, int centralDefendingMidfielder,
            int leftWingForward, int rightWingForward, int striker)
        {
            Name = name;
            RequiredPlayerCounts = new Dictionary<Enums.Positions, int>
        {
            { Enums.Positions.LeftBack, leftBack },
            { Enums.Positions.RightBack, rightBack },
            { Enums.Positions.CenterBack, centerBack },
            { Enums.Positions.CentralAttackingMidfielder, attackingMidfielder },
            { Enums.Positions.CentralMidfielder, centralMidfielder },
            { Enums.Positions.LeftMidfielder, leftMidfielder },
            { Enums.Positions.RightMidfielder, rightMidfielder },
            { Enums.Positions.CentralDefendingMidfielder, centralDefendingMidfielder },
            { Enums.Positions.LeftWingForward, leftWingForward },
            { Enums.Positions.RightWingForward, rightWingForward },
            { Enums.Positions.Striker, striker }
        };

            // Validate that the total required outfield players for any formation is exactly 10.
            int totalRequired = RequiredPlayerCounts.Sum(kv => kv.Value);
            if (totalRequired != 10)
            {
                //throw new ArgumentException($"Formation '{name}' must require exactly 10 outfield players. Currently requires: {totalRequired}.");
            }
        }

        public int Defenders { get; set; }
        public int Midfielders { get; set;  }
        public int Forwards { get; set;  }

        /// <summary>
        /// Initializes a new instance of the SoccerFormation class.
        /// </summary>
        /// <param name="name">The common name of the formation (e.g., "4-3-3").</param>
        /// <param name="defenders">The number of defenders required.</param>
        /// <param name="midfielders">The number of midfielders required.</param>
        /// <param name="forwards">The number of forwards required.</param>
        public SoccerFormation(string name, int defenders, int midfielders, int forwards)
        {
            Name = name;
            Defenders = defenders;
            Midfielders = midfielders;
            Forwards = forwards;
        }

        /// <summary>
        /// Calculates the total number of players available who can play in defensive roles.
        /// </summary>
        public int GetTotalDefenders()
        {
            return RequiredPlayerCounts[Enums.Positions.LeftBack] + RequiredPlayerCounts[Enums.Positions.RightBack] +
                RequiredPlayerCounts[Enums.Positions.CenterBack];
        }

        /// <summary>
        /// Calculates the total number of players available who can play in midfield roles.
        /// </summary>
        public int GetTotalMidfielders()
        {
            return RequiredPlayerCounts[Enums.Positions.CentralAttackingMidfielder] + RequiredPlayerCounts[Enums.Positions.CentralMidfielder] +
                   RequiredPlayerCounts[Enums.Positions.LeftMidfielder] + RequiredPlayerCounts[Enums.Positions.RightMidfielder] +
                   RequiredPlayerCounts[Enums.Positions.CentralDefendingMidfielder];
        }

        /// <summary>
        /// Calculates the total number of players available who can play in forward roles.
        /// </summary>
        public int GetTotalForwards()
        {
            return RequiredPlayerCounts[Enums.Positions.LeftWingForward] + RequiredPlayerCounts[Enums.Positions.RightWingForward] +
                RequiredPlayerCounts[Enums.Positions.Striker];
        }
    }