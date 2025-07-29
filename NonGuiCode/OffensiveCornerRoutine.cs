using FunnyOldGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// In your project, likely within a "SetPieces" or "Routines" folder
using System.Collections.Generic;
// Make sure FunnyOldGame and Enums namespace are accessible
// using FunnyOldGame; // Assuming Enums are in FunnyOldGame.Enums or a similar structure

public class OffensiveCornerRoutine : SetPieceRoutine // *** CHANGE 1: Inherit from SetPieceRoutine ***
{
    // Specific properties for Offensive Corner Routines
    public Enums.CornerDeliveryType PreferredDeliveryType { get; private set; } // *** CHANGE 4: Renamed for consistency ***
    public Enums.SetPieceTargetType PreferredTargetType { get; private set; } // *** CHANGE 3: Added new enum property ***

    // This dictionary defines which player positions are preferred for each role within *this specific routine*
    // Key: SetPieceRole (e.g., TargetMan), Value: List of preferred PlayerPositions (e.g., Striker, CB)
    // *** CHANGE 5: Changed PlayerRole to Positions, and removed direct initialization ***
    public Dictionary<Enums.SetPieceRole, List<Enums.Positions>> RoleAssignments { get; private set; }

    // NEW: Map specific SetPieceRoles to their intended conceptual sub-zones
    // This allows a "TargetMan" to go to "NearPost" even if the overall routine target is "Central"
    // *** CHANGE 7: Removed direct initialization ***
    public Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone> PreferredRoleConceptualSubZones { get; private set; }

    // This is the primary target zone where the cross is aimed (already good, just ensure private set)
    public Enums.ConceptualSetPieceZone TargetConceptualZone { get; private set; }

    public Enums.SetPieceRoutine RoutineType { get; private set; } // New property

    // Constructor for OffensiveCornerRoutine (now primary constructor)
    public OffensiveCornerRoutine(
        string name,
        string description, // *** CHANGE: Added description for base class ***
        Enums.CornerDeliveryType preferredDeliveryType, // *** CHANGE: Renamed ***
        Enums.SetPieceTargetType preferredTargetType,  // *** CHANGE: Added ***
        Enums.ConceptualSetPieceZone targetConceptualZone,
        Dictionary<Enums.SetPieceRole, List<Enums.Positions>> roleAssignments, // *** CHANGE: Type changed ***
        Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone> preferredRoleConceptualSubZones = null,
        Enums.Flank? applicableFlank = null,       // *** CHANGE: Added for base class ***
        bool isHomeTeamSpecific = false,           // *** CHANGE: Added for base class ***
        bool isAwayTeamSpecific = false,
        Enums.SetPieceRoutine routineType = Enums.SetPieceRoutine.None)           // *** CHANGE: Added for base class ***
        // *** CHANGE 2: Call base constructor, setting AppliesToSetPieceType to CornerKick ***
        : base(name, description, Enums.SetPieceType.CornerKick, applicableFlank, isHomeTeamSpecific, isAwayTeamSpecific)
    {
        RoutineType = routineType; // Assign the new property
        PreferredDeliveryType = preferredDeliveryType;
        PreferredTargetType = preferredTargetType;
        TargetConceptualZone = targetConceptualZone;
        RoleAssignments = roleAssignments ?? new Dictionary<Enums.SetPieceRole, List<Enums.Positions>>();
        PreferredRoleConceptualSubZones = preferredRoleConceptualSubZones ?? new Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone>();
    }

    // *** CHANGE 7: Implement the abstract method from SetPieceRoutine ***
    public override Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone> GetConceptualPositioningMap()
    {
        return PreferredRoleConceptualSubZones;
    }
}