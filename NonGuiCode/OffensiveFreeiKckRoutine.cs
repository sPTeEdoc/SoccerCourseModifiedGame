using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// In a new file, e.g., OffensiveFreeKickRoutine.cs
using System.Collections.Generic;
using FunnyOldGame;

// using FunnyOldGame; // Make sure this is present if needed

public class OffensiveFreeKickRoutine : SetPieceRoutine
{
    // Specific properties for Offensive Free Kick Routines
    public Enums.FreeKickDeliveryType PreferredDeliveryType { get; private set; }
    public Enums.SetPieceTargetType PreferredTargetType { get; private set; } // How the delivery is aimed

    // This defines which player positions are preferred for each role within *this specific routine*
    public Dictionary<Enums.SetPieceRole, List<Enums.Positions>> RoleAssignments { get; private set; }

    // This maps each SetPieceRole *in this routine* to the conceptual zone that player should move to.
    public Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone> PreferredRoleConceptualSubZones { get; private set; }

    // Defines the primary conceptual target zone for the ball delivery itself
    public Enums.ConceptualSetPieceZone TargetConceptualZone { get; private set; } // NOW A PROPERTY ON THIS CLASS

    // Additional property specific to Free Kicks (e.g., if it's a direct shot, this might be relevant)
    public bool IsDirectShotAttempt { get; private set; }

    public Enums.SetPieceRoutine RoutineType {get; set;}

    // Constructor for OffensiveFreeKickRoutine
    public OffensiveFreeKickRoutine(
        string name,
        string description,
        Enums.FreeKickDeliveryType preferredDeliveryType,
        Enums.SetPieceTargetType preferredTargetType,
        Enums.ConceptualSetPieceZone targetConceptualZone, // This parameter is now assigned to *this* class's property
        Dictionary<Enums.SetPieceRole, List<Enums.Positions>> roleAssignments,
        Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone> preferredRoleConceptualSubZones = null,
        bool isDirectShotAttempt = false,
        Enums.Flank? applicableFlank = null,
        bool isHomeTeamSpecific = false,
        bool isAwayTeamSpecific = false,
        Enums.SetPieceRoutine routineType = Enums.SetPieceRoutine.None)
        : base(name, description, Enums.SetPieceType.FreeKick, applicableFlank, isHomeTeamSpecific, isAwayTeamSpecific) // Base constructor only takes these
    {
        PreferredDeliveryType = preferredDeliveryType;
        PreferredTargetType = preferredTargetType;
        TargetConceptualZone = targetConceptualZone; // Assign to property of this class
        RoleAssignments = roleAssignments ?? new Dictionary<Enums.SetPieceRole, List<Enums.Positions>>();
        PreferredRoleConceptualSubZones = preferredRoleConceptualSubZones ?? new Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone>();
        IsDirectShotAttempt = isDirectShotAttempt;
        this.RoutineType = routineType;
        
    }

    // Override the abstract method from the base class
    public override Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone> GetConceptualPositioningMap()
    {
        return PreferredRoleConceptualSubZones;
    }
}