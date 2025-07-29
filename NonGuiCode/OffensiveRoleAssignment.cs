using FunnyOldGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class OffensiveRoleAssignment
{
    public Enums.SetPieceRole Role { get; set; } // e.g., SetPieceAttacker, ZonalAttacker
    public Enums.PlayerRole PreferredPlayerRole { get; set; } // e.g., Striker, CenterBack for target
    public Enums.ConceptualSetPieceZone TargetZone { get; set; } // Where this player should move
    public int Quantity { get; set; } // How many players for this role
}
