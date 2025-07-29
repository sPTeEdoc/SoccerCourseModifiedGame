using FunnyOldGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// In a new file, e.g., DefensiveSetPieceRoutine.cs
using System.Collections.Generic;

// using System.Collections.Generic; // Make sure this is present if needed

public class DefensiveSetPieceRoutine : SetPieceRoutine // Inherit from our base abstract class
{
    // Specific properties for Defensive Set Piece Routines
    public Enums.DefensiveStrategy Strategy { get; private set; } // e.g., ManMarking, ZonalMarking, Hybrid
    public bool UsePostBlockers { get; private set; } // Whether to assign players to block posts

    // This will map a DefensiveSetPieceRole (from Enums.SetPieceRole) to a list of suitable PlayerPositions
    public Dictionary<Enums.SetPieceRole, List<Enums.Positions>> RoleAssignments { get; private set; } // Corrected to Enums.Positions

    // For zonal markers, define their preferred conceptual zones to cover
    public Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone> PreferredZoneCoverage { get; private set; }

    // New property: Map specific attacker roles to the defender roles assigned to mark them
    public Dictionary<Enums.SetPieceRole, Enums.SetPieceRole> AttackerRoleToDefenderRoleMapping { get; private set; }

    // Constructor for DefensiveSetPieceRoutine
    public DefensiveSetPieceRoutine(
        string name,
        string description,
        Enums.SetPieceType appliesToType,
        Enums.DefensiveStrategy strategy,
        bool usePostBlockers,
        Dictionary<Enums.SetPieceRole, List<Enums.Positions>> roleAssignments,
        Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone> preferredZoneCoverage = null,
        Dictionary<Enums.SetPieceRole, Enums.SetPieceRole> attackerRoleToDefenderRoleMapping = null,
        Enums.Flank? applicableFlank = null,
        bool isHomeTeamSpecific = false,
        bool isAwayTeamSpecific = false)
        : base(name, description, appliesToType, applicableFlank, isHomeTeamSpecific, isAwayTeamSpecific) // Base constructor only takes these
    {
        Strategy = strategy;
        UsePostBlockers = usePostBlockers;
        RoleAssignments = roleAssignments ?? new Dictionary<Enums.SetPieceRole, List<Enums.Positions>>();
        PreferredZoneCoverage = preferredZoneCoverage ?? new Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone>();
        AttackerRoleToDefenderRoleMapping = attackerRoleToDefenderRoleMapping ?? new Dictionary<Enums.SetPieceRole, Enums.SetPieceRole>();
    }

    // Implement the abstract method from SetPieceRoutine
    public override Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone> GetConceptualPositioningMap()
    {
        return PreferredZoneCoverage;
    }
}