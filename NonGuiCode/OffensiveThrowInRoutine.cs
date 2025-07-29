// In a new file, e.g., OffensiveThrowInRoutine.cs
using System;
using System.Collections.Generic;
using System.Linq;
using FunnyOldGame; // Assuming FunnyOldGame provides Player and other core classes

public class OffensiveThrowInRoutine : SetPieceRoutine
{
    // Specific properties for Offensive Throw-In Routines
    public Enums.ThrowInDeliveryType PreferredDeliveryType { get; private set; }

    // This defines which player positions are preferred for each role within *this specific routine*
    public Dictionary<Enums.SetPieceRole, List<Enums.Positions>> RoleAssignments { get; private set; }

    // This maps each SetPieceRole *in this routine* to the conceptual zone that player should move to.
    public Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone> PreferredRoleConceptualSubZones { get; private set; }

    // Defines the primary conceptual target zone for the ball delivery itself
    public Enums.ConceptualSetPieceZone TargetConceptualZone { get; private set; }

    // Constructor for OffensiveThrowInRoutine
    public OffensiveThrowInRoutine(
        string name,
        string description,
        Enums.ThrowInDeliveryType preferredDeliveryType,
        Enums.ConceptualSetPieceZone targetConceptualZone, // Make this a direct parameter for the constructor
        Dictionary<Enums.SetPieceRole, List<Enums.Positions>> roleAssignments,
        Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone> preferredRoleConceptualSubZones = null,
        Enums.Flank? applicableFlank = null,
        bool isHomeTeamSpecific = false,
        bool isAwayTeamSpecific = false)
        : base(name, description, Enums.SetPieceType.ThrowIn, applicableFlank, isHomeTeamSpecific, isAwayTeamSpecific)
    {
        PreferredDeliveryType = preferredDeliveryType;
        TargetConceptualZone = targetConceptualZone; // Assign directly from constructor parameter
        RoleAssignments = roleAssignments ?? new Dictionary<Enums.SetPieceRole, List<Enums.Positions>>();
        PreferredRoleConceptualSubZones = preferredRoleConceptualSubZones ?? new Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone>();
    }

    // Implement the abstract method from SetPieceRoutine
    public override Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone> GetConceptualPositioningMap()
    {
        return PreferredRoleConceptualSubZones;
    }

    // Static factory methods for common throw-in routines
    public static OffensiveThrowInRoutine CreateDefaultShortThrowRoutine()
    {
        var roleAssignments = new Dictionary<Enums.SetPieceRole, List<Enums.Positions>>
        {
            { Enums.SetPieceRole.ShortOptionReceiver, new List<Enums.Positions> { Enums.Positions.CentralMidfielder, Enums.Positions.LeftBack, Enums.Positions.RightBack, Enums.Positions.LeftWingForward, Enums.Positions.RightWingForward, Enums.Positions.CentralAttackingMidfielder } },
            { Enums.SetPieceRole.MidfieldSupport, new List<Enums.Positions> { Enums.Positions.CentralMidfielder, Enums.Positions.CentralDefendingMidfielder } }
        };
        var preferredZones = new Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone>
        {
            { Enums.SetPieceRole.ShortOptionReceiver, Enums.ConceptualSetPieceZone.OpponentAttackingThird_Flank } // Use your specific enum
        };

        return new OffensiveThrowInRoutine(
            name: "Short Throw-In",
            description: "A quick short throw to a nearby teammate to retain possession on the flank.",
            preferredDeliveryType: Enums.ThrowInDeliveryType.ShortThrow,
            targetConceptualZone: Enums.ConceptualSetPieceZone.OpponentAttackingThird_Flank, // General target for the throw itself
            roleAssignments: roleAssignments,
            preferredRoleConceptualSubZones: preferredZones
        );
    }

    public static OffensiveThrowInRoutine CreateDefaultLongThrowRoutine()
    {
        var roleAssignments = new Dictionary<Enums.SetPieceRole, List<Enums.Positions>>
        {
            { Enums.SetPieceRole.TargetMan, new List<Enums.Positions> { Enums.Positions.Striker, Enums.Positions.CenterBack, Enums.Positions.CentralMidfielder } },
            { Enums.SetPieceRole.BoxAttacker, new List<Enums.Positions> { Enums.Positions.CentralMidfielder, Enums.Positions.LeftWingForward, Enums.Positions.RightWingForward, Enums.Positions.Striker } }
        };
        var preferredZones = new Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone>
        {
            { Enums.SetPieceRole.TargetMan, Enums.ConceptualSetPieceZone.OpponentPenaltyArea_NearPost },
            { Enums.SetPieceRole.BoxAttacker, Enums.ConceptualSetPieceZone.OpponentPenaltyArea_Central }
        };

        return new OffensiveThrowInRoutine(
            name: "Long Throw-In into Box",
            description: "A long throw towards the penalty area for an aerial duel or second ball.",
            preferredDeliveryType: Enums.ThrowInDeliveryType.LongThrowIntoBox,
            targetConceptualZone: Enums.ConceptualSetPieceZone.OpponentPenaltyArea_General, // General target for the throw itself
            roleAssignments: roleAssignments,
            preferredRoleConceptualSubZones: preferredZones
        );
    }
}