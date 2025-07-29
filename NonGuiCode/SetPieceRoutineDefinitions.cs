using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunnyOldGame; // For .ToList() and .FirstOrDefault() and Enums

public static class SetPieceRoutineDefinitions
{
    // --- Private Collections to store all routines ---
    private static readonly List<OffensiveCornerRoutine> _offensiveCornerRoutines = new List<OffensiveCornerRoutine>();
    private static readonly List<OffensiveFreeKickRoutine> _offensiveFreeKickRoutines = new List<OffensiveFreeKickRoutine>();
    private static readonly List<OffensiveThrowInRoutine> _offensiveThrowInRoutines = new List<OffensiveThrowInRoutine>();
    private static readonly List<DefensiveSetPieceRoutine> _defensiveSetPieceRoutines = new List<DefensiveSetPieceRoutine>();

    // --- Static Constructor: Called once when the class is first accessed ---
    static SetPieceRoutineDefinitions()
    {
        InitializeRoutines();
    }

    // --- Method to populate the routine lists ---
    private static void InitializeRoutines()
    {
        // Clear existing routines to prevent duplicates on re-initialization (if applicable in a dev environment)
        _offensiveCornerRoutines.Clear();
        _offensiveFreeKickRoutines.Clear();
        _offensiveThrowInRoutines.Clear();
        _defensiveSetPieceRoutines.Clear();

        // --- Add Offensive Corner Routines ---
        _offensiveCornerRoutines.Add(CreateShortCornerPlayRoutine());
        _offensiveCornerRoutines.Add(CreateInswingingLoftedCrossFarPostRoutine());
        _offensiveCornerRoutines.Add(CreateDrivenOutswingerNearPostRoutine());
        _offensiveCornerRoutines.Add(CreateCentralLoftedCrossRoutine());
        _offensiveCornerRoutines.Add(CreateDefaultLoftedInswingerCornerRoutine());

        // --- Add Offensive Free Kick Routines ---
        _offensiveFreeKickRoutines.Add(CreateDefaultOffensiveFreeKickRoutine());
        _offensiveFreeKickRoutines.Add(CreateDirectFreeKickShotRoutine()); // Add this new routine

        // --- Add Offensive Throw-In Routines ---
        _offensiveThrowInRoutines.Add(OffensiveThrowInRoutine.CreateDefaultShortThrowRoutine());
        _offensiveThrowInRoutines.Add(OffensiveThrowInRoutine.CreateDefaultLongThrowRoutine());

        // --- Add Defensive Set Piece Routines ---
        _defensiveSetPieceRoutines.Add(CreateDefaultDefensiveCornerRoutine());
        _defensiveSetPieceRoutines.Add(CreateDefaultDefensiveFreeKickRoutine());
        _defensiveSetPieceRoutines.Add(CreateDefaultDefensiveThrowInRoutine());
    }

    // --- Public Getters for Routines ---

    // Generic method to get any SetPieceRoutine by name and type
    public static T GetRoutineByName<T>(string name) where T : SetPieceRoutine
    {
        if (typeof(T) == typeof(OffensiveCornerRoutine))
        {
            return _offensiveCornerRoutines.FirstOrDefault(r => r.Name == name) as T;
        }
        else if (typeof(T) == typeof(OffensiveFreeKickRoutine))
        {
            return _offensiveFreeKickRoutines.FirstOrDefault(r => r.Name == name) as T;
        }
        else if (typeof(T) == typeof(OffensiveThrowInRoutine))
        {
            return _offensiveThrowInRoutines.FirstOrDefault(r => r.Name == name) as T;
        }
        else if (typeof(T) == typeof(DefensiveSetPieceRoutine))
        {
            return _defensiveSetPieceRoutines.FirstOrDefault(r => r.Name == name) as T;
        }
        return null;
    }

    // Get all offensive corner routines
    public static IEnumerable<OffensiveCornerRoutine> GetAllOffensiveCornerRoutines()
    {
        return _offensiveCornerRoutines;
    }

    // Get all offensive free kick routines
    public static IEnumerable<OffensiveFreeKickRoutine> GetAllOffensiveFreeKickRoutines()
    {
        return _offensiveFreeKickRoutines;
    }

    // Get all offensive throw-in routines
    public static IEnumerable<OffensiveThrowInRoutine> GetAllOffensiveThrowInRoutines()
    {
        return _offensiveThrowInRoutines;
    }

    // Get all defensive routines (you might add filters here later, e.g., for corners vs. free kicks)
    public static IEnumerable<DefensiveSetPieceRoutine> GetAllDefensiveRoutines()
    {
        return _defensiveSetPieceRoutines;
    }

    // --- Methods to get specific default routines ---
    public static OffensiveCornerRoutine GetDefaultOffensiveCornerRoutine()
    {
        return _offensiveCornerRoutines.FirstOrDefault(r => r.Name == "Default Lofted Inswinger")
               ?? _offensiveCornerRoutines.FirstOrDefault(); // Fallback
    }

    public static OffensiveFreeKickRoutine GetDefaultOffensiveFreeKickRoutine()
    {
        return _offensiveFreeKickRoutines.FirstOrDefault(r => r.Name == "Default Lofted Free Kick into Box")
               ?? _offensiveFreeKickRoutines.FirstOrDefault(); // Fallback
    }

    public static OffensiveThrowInRoutine GetDefaultOffensiveThrowInRoutine()
    {
        return _offensiveThrowInRoutines.FirstOrDefault(r => r.PreferredDeliveryType == Enums.ThrowInDeliveryType.ShortThrow)
               ?? _offensiveThrowInRoutines.FirstOrDefault(); // Fallback to any short throw
    }

    public static DefensiveSetPieceRoutine GetDefaultDefensiveCornerRoutine()
    {
        return _defensiveSetPieceRoutines.FirstOrDefault(r => r.Name == "Default Zonal Corner Defense")
               ?? _defensiveSetPieceRoutines.FirstOrDefault(r => r.AppliesToSetPieceType == Enums.SetPieceType.CornerKick); // Fallback
    }

    public static DefensiveSetPieceRoutine GetDefaultDefensiveFreeKickRoutine()
    {
        return _defensiveSetPieceRoutines.FirstOrDefault(r => r.AppliesToSetPieceType == Enums.SetPieceType.FreeKick && r.Name == "Default Free Kick Defense")
               ?? _defensiveSetPieceRoutines.FirstOrDefault(r => r.AppliesToSetPieceType == Enums.SetPieceType.FreeKick); // Fallback to any free kick defense
    }

    public static DefensiveSetPieceRoutine GetDefaultDefensiveThrowInRoutine()
    {
        return _defensiveSetPieceRoutines.FirstOrDefault(r => r.AppliesToSetPieceType == Enums.SetPieceType.ThrowIn && r.Name == "Default Throw-In Defense")
               ?? _defensiveSetPieceRoutines.FirstOrDefault(r => r.AppliesToSetPieceType == Enums.SetPieceType.ThrowIn); // Fallback to any throw-in defense
    }


    // --- Private Helper Methods to create routine instances ---

    // Offensive Corner Routines
    private static OffensiveCornerRoutine CreateShortCornerPlayRoutine()
    {
        return new OffensiveCornerRoutine(
            name: "Short Corner Play",
            description: "A quick short corner to bypass aerial defense, looking for a cut-back or a pass into the box.",
            preferredDeliveryType: Enums.CornerDeliveryType.ShortPass,
            preferredTargetType: Enums.SetPieceTargetType.SpecificPlayer, // Targeting the short option receiver
            targetConceptualZone: Enums.ConceptualSetPieceZone.ShortCornerArea,
            roleAssignments: new Dictionary<Enums.SetPieceRole, List<Enums.Positions>>
            {
                { Enums.SetPieceRole.SetPieceTaker, new List<Enums.Positions> { Enums.Positions.LeftWingForward, Enums.Positions.RightWingForward, Enums.Positions.CentralAttackingMidfielder } },
                { Enums.SetPieceRole.ShortOptionReceiver, new List<Enums.Positions> { Enums.Positions.LeftBack, Enums.Positions.RightBack, Enums.Positions.CentralMidfielder } },
                { Enums.SetPieceRole.ReboundAttacker, new List<Enums.Positions> { Enums.Positions.CentralMidfielder, Enums.Positions.CentralAttackingMidfielder } }, // For potential shots/rebounds
                { Enums.SetPieceRole.BoxAttacker, new List<Enums.Positions> { Enums.Positions.Striker, Enums.Positions.CenterBack } }
            },
            preferredRoleConceptualSubZones: new Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone>
            {
                { Enums.SetPieceRole.SetPieceTaker, Enums.ConceptualSetPieceZone.CornerFlag },
                { Enums.SetPieceRole.ShortOptionReceiver, Enums.ConceptualSetPieceZone.ShortCornerArea },
                { Enums.SetPieceRole.ReboundAttacker, Enums.ConceptualSetPieceZone.EdgeOfBox },
                { Enums.SetPieceRole.BoxAttacker, Enums.ConceptualSetPieceZone.OpponentPenaltyArea_Central }
            },
            routineType: Enums.SetPieceRoutine.ShortCornerPlay // NEW: Pass the routine type
        );
    }

    private static OffensiveCornerRoutine CreateInswingingLoftedCrossFarPostRoutine()
    {
        return new OffensiveCornerRoutine(
            name: "Inswinging Lofted Cross - Far Post",
            description: "A high inswinging cross aimed at the far post, targeting tall players.",
            preferredDeliveryType: Enums.CornerDeliveryType.InswingerLofted,
            preferredTargetType: Enums.SetPieceTargetType.SpecificZone,
            targetConceptualZone: Enums.ConceptualSetPieceZone.OpponentPenaltyArea_FarPost,
            roleAssignments: new Dictionary<Enums.SetPieceRole, List<Enums.Positions>>
            {
                { Enums.SetPieceRole.TargetMan, new List<Enums.Positions> { Enums.Positions.Striker, Enums.Positions.CenterBack } },
                { Enums.SetPieceRole.BoxAttacker, new List<Enums.Positions> { Enums.Positions.CentralMidfielder, Enums.Positions.Striker } },
                { Enums.SetPieceRole.ReboundAttacker, new List<Enums.Positions> { Enums.Positions.CentralDefendingMidfielder, Enums.Positions.CentralMidfielder } }
            },
            preferredRoleConceptualSubZones: new Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone>
            {
                { Enums.SetPieceRole.TargetMan, Enums.ConceptualSetPieceZone.OpponentPenaltyArea_FarPost },
                { Enums.SetPieceRole.BoxAttacker, Enums.ConceptualSetPieceZone.OpponentPenaltyArea_General }, // Can roam in general area
                { Enums.SetPieceRole.ReboundAttacker, Enums.ConceptualSetPieceZone.EdgeOfBox }
            },
            routineType: Enums.SetPieceRoutine.FarPostRun // NEW: Pass the routine type
        );
    }

    private static OffensiveCornerRoutine CreateDrivenOutswingerNearPostRoutine()
    {
        return new OffensiveCornerRoutine(
            name: "Driven Outswinger - Near Post",
            description: "A powerful outswinging cross aimed at the near post, looking for a flick-on or quick shot.",
            preferredDeliveryType: Enums.CornerDeliveryType.OutswingerDriven,
            preferredTargetType: Enums.SetPieceTargetType.SpecificZone,
            targetConceptualZone: Enums.ConceptualSetPieceZone.OpponentPenaltyArea_NearPost,
            roleAssignments: new Dictionary<Enums.SetPieceRole, List<Enums.Positions>>
            {
                { Enums.SetPieceRole.NearPostRunner, new List<Enums.Positions> { Enums.Positions.Striker, Enums.Positions.LeftWingForward, Enums.Positions.RightWingForward } },
                { Enums.SetPieceRole.BoxAttacker, new List<Enums.Positions> { Enums.Positions.Striker, Enums.Positions.CentralMidfielder, Enums.Positions.CenterBack } }, // Also attacking the box
                { Enums.SetPieceRole.ReboundAttacker, new List<Enums.Positions> { Enums.Positions.CentralMidfielder, Enums.Positions.CentralAttackingMidfielder } }
            },
            preferredRoleConceptualSubZones: new Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone>
            {
                { Enums.SetPieceRole.NearPostRunner, Enums.ConceptualSetPieceZone.OpponentPenaltyArea_NearPost },
                { Enums.SetPieceRole.BoxAttacker, Enums.ConceptualSetPieceZone.OpponentPenaltyArea_General },
                { Enums.SetPieceRole.ReboundAttacker, Enums.ConceptualSetPieceZone.EdgeOfBox }
            },
            routineType: Enums.SetPieceRoutine.NearPostRun // NEW: Pass the routine type
        );
    }

    private static OffensiveCornerRoutine CreateCentralLoftedCrossRoutine()
    {
        return new OffensiveCornerRoutine(
            name: "Central Lofted Cross",
            description: "A lofted cross delivered to the central area of the box, for powerful headers.",
            preferredDeliveryType: Enums.CornerDeliveryType.InswingerLofted, // Can be inswinger or outswinger
            preferredTargetType: Enums.SetPieceTargetType.SpecificZone,
            targetConceptualZone: Enums.ConceptualSetPieceZone.OpponentPenaltyArea_Central,
            roleAssignments: new Dictionary<Enums.SetPieceRole, List<Enums.Positions>>
            {
                { Enums.SetPieceRole.TargetMan, new List<Enums.Positions> { Enums.Positions.Striker, Enums.Positions.CenterBack } },
                { Enums.SetPieceRole.BoxAttacker, new List<Enums.Positions> { Enums.Positions.CentralMidfielder, Enums.Positions.Striker, Enums.Positions.CenterBack } },
                { Enums.SetPieceRole.ReboundAttacker, new List<Enums.Positions> { Enums.Positions.CentralDefendingMidfielder, Enums.Positions.CentralMidfielder } }
            },
            preferredRoleConceptualSubZones: new Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone>
            {
                { Enums.SetPieceRole.TargetMan, Enums.ConceptualSetPieceZone.OpponentPenaltyArea_Central },
                { Enums.SetPieceRole.BoxAttacker, Enums.ConceptualSetPieceZone.OpponentPenaltyArea_General },
                { Enums.SetPieceRole.ReboundAttacker, Enums.ConceptualSetPieceZone.EdgeOfBox }
            },
            routineType: Enums.SetPieceRoutine.CrowdTheBox // NEW: Pass the routine type (or a more specific one if you create it)
        );
    }

    private static OffensiveCornerRoutine CreateDefaultLoftedInswingerCornerRoutine()
    {
        return new OffensiveCornerRoutine(
            name: "Default Lofted Inswinger",
            description: "A basic inswinging lofted cross to the center of the box, aiming for general threat.",
            preferredDeliveryType: Enums.CornerDeliveryType.InswingerLofted,
            preferredTargetType: Enums.SetPieceTargetType.SpecificZone,
            targetConceptualZone: Enums.ConceptualSetPieceZone.OpponentPenaltyArea_Central,
            roleAssignments: new Dictionary<Enums.SetPieceRole, List<Enums.Positions>>
            {
                { Enums.SetPieceRole.BoxAttacker, new List<Enums.Positions> { Enums.Positions.Striker, Enums.Positions.CentralMidfielder, Enums.Positions.CenterBack } },
                { Enums.SetPieceRole.ReboundAttacker, new List<Enums.Positions> { Enums.Positions.CentralDefendingMidfielder, Enums.Positions.CentralMidfielder } }
            },
            preferredRoleConceptualSubZones: new Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone>
            {
                { Enums.SetPieceRole.BoxAttacker, Enums.ConceptualSetPieceZone.OpponentPenaltyArea_Central },
                { Enums.SetPieceRole.ReboundAttacker, Enums.ConceptualSetPieceZone.EdgeOfBox }
            },
            routineType: Enums.SetPieceRoutine.CrowdTheBox // NEW: Pass the routine type (adjust if you have a "GeneralCornerCross" enum)
        );
    }

    // Offensive Free Kick Routines
    private static OffensiveFreeKickRoutine CreateDefaultOffensiveFreeKickRoutine()
    {
        return new OffensiveFreeKickRoutine(
            name: "Default Lofted Free Kick into Box",
            description: "A standard offensive free kick, aiming for an aerial duel in the box.",
            preferredDeliveryType: Enums.FreeKickDeliveryType.LoftedPassIntoBox,
            preferredTargetType: Enums.SetPieceTargetType.SpecificPlayer, // Can target a specific player or a zone
            targetConceptualZone: Enums.ConceptualSetPieceZone.OpponentPenaltyArea_Central,
            roleAssignments: new Dictionary<Enums.SetPieceRole, List<Enums.Positions>>
            {
                { Enums.SetPieceRole.SetPieceTaker, new List<Enums.Positions> { Enums.Positions.CentralMidfielder, Enums.Positions.CentralAttackingMidfielder, Enums.Positions.LeftBack, Enums.Positions.RightBack } },
                { Enums.SetPieceRole.TargetMan, new List<Enums.Positions> { Enums.Positions.Striker, Enums.Positions.CenterBack } },
                { Enums.SetPieceRole.BoxAttacker, new List<Enums.Positions> { Enums.Positions.Striker, Enums.Positions.CentralMidfielder, Enums.Positions.CentralAttackingMidfielder } },
                { Enums.SetPieceRole.ReboundAttacker, new List<Enums.Positions> { Enums.Positions.CentralDefendingMidfielder, Enums.Positions.CentralMidfielder } },
                { Enums.SetPieceRole.MidfieldSupport, new List<Enums.Positions> { Enums.Positions.CentralDefendingMidfielder, Enums.Positions.CentralMidfielder } }
            },
            preferredRoleConceptualSubZones: new Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone>
            {
                // Note: The taker's 'zone' might be flexible or just 'near the ball'
                { Enums.SetPieceRole.TargetMan, Enums.ConceptualSetPieceZone.OpponentPenaltyArea_Central },
                { Enums.SetPieceRole.BoxAttacker, Enums.ConceptualSetPieceZone.OpponentPenaltyArea_General },
                { Enums.SetPieceRole.ReboundAttacker, Enums.ConceptualSetPieceZone.EdgeOfBox },
                { Enums.SetPieceRole.MidfieldSupport, Enums.ConceptualSetPieceZone.Midfield_Center }
            },
            isDirectShotAttempt: false,
            routineType: Enums.SetPieceRoutine.CrowdTheBox // NEW: For free kicks aimed into the box, CrowdTheBox is a good fit
        );
    }

    private static OffensiveFreeKickRoutine CreateDirectFreeKickShotRoutine()
    {
        return new OffensiveFreeKickRoutine(
            name: "Default Lofted Free Kick into Box",
            description: "A standard offensive free kick, aiming for an aerial duel in the box.",
            preferredDeliveryType: Enums.FreeKickDeliveryType.LoftedPassIntoBox,
            preferredTargetType: Enums.SetPieceTargetType.SpecificPlayer, // Can target a specific player or a zone
            targetConceptualZone: Enums.ConceptualSetPieceZone.OpponentPenaltyArea_Central,
            roleAssignments: new Dictionary<Enums.SetPieceRole, List<Enums.Positions>>
            {
                { Enums.SetPieceRole.SetPieceTaker, new List<Enums.Positions> { Enums.Positions.CentralMidfielder, Enums.Positions.CentralAttackingMidfielder, Enums.Positions.LeftBack, Enums.Positions.RightBack } },
                { Enums.SetPieceRole.TargetMan, new List<Enums.Positions> { Enums.Positions.Striker, Enums.Positions.CenterBack } },
                { Enums.SetPieceRole.BoxAttacker, new List<Enums.Positions> { Enums.Positions.Striker, Enums.Positions.CentralMidfielder, Enums.Positions.CentralAttackingMidfielder } },
                { Enums.SetPieceRole.ReboundAttacker, new List<Enums.Positions> { Enums.Positions.CentralDefendingMidfielder, Enums.Positions.CentralMidfielder } },
                { Enums.SetPieceRole.MidfieldSupport, new List<Enums.Positions> { Enums.Positions.CentralDefendingMidfielder, Enums.Positions.CentralMidfielder } }
            },
            preferredRoleConceptualSubZones: new Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone>
            {
                // Note: The taker's 'zone' might be flexible or just 'near the ball'
                { Enums.SetPieceRole.TargetMan, Enums.ConceptualSetPieceZone.OpponentPenaltyArea_Central },
                { Enums.SetPieceRole.BoxAttacker, Enums.ConceptualSetPieceZone.OpponentPenaltyArea_General },
                { Enums.SetPieceRole.ReboundAttacker, Enums.ConceptualSetPieceZone.EdgeOfBox },
                { Enums.SetPieceRole.MidfieldSupport, Enums.ConceptualSetPieceZone.Midfield_Center }
            },
            isDirectShotAttempt: false, routineType: Enums.SetPieceRoutine.ShotOnGoal
        );
    }

    // Defensive Set Piece Routines (no changes here as they don't use the 'routineType' parameter currently)
    private static DefensiveSetPieceRoutine CreateDefaultDefensiveCornerRoutine()
    {
        return new DefensiveSetPieceRoutine(
            name: "Default Zonal Corner Defense",
            description: "A standard zonal defense for corners, using post blockers and covering key areas.",
            appliesToType: Enums.SetPieceType.CornerKick,
            strategy: Enums.DefensiveStrategy.ZonalMarking,
            usePostBlockers: true,
            roleAssignments: new Dictionary<Enums.SetPieceRole, List<Enums.Positions>>
            {
                { Enums.SetPieceRole.PostBlocker, new List<Enums.Positions> { Enums.Positions.LeftBack, Enums.Positions.RightBack, Enums.Positions.CenterBack } },
                { Enums.SetPieceRole.ZonalMarker, new List<Enums.Positions> { Enums.Positions.CenterBack, Enums.Positions.CentralDefendingMidfielder, Enums.Positions.CentralMidfielder } },
                { Enums.SetPieceRole.ManMarker, new List<Enums.Positions> { Enums.Positions.CentralMidfielder, Enums.Positions.LeftBack, Enums.Positions.RightBack } }, // Can have some man-markers
                { Enums.SetPieceRole.ClearingDefender, new List<Enums.Positions> { Enums.Positions.CenterBack, Enums.Positions.CentralDefendingMidfielder } }, // For clearances
                { Enums.SetPieceRole.CounterAttackOutlet, new List<Enums.Positions> { Enums.Positions.LeftWingForward, Enums.Positions.RightWingForward, Enums.Positions.Striker, Enums.Positions.CentralAttackingMidfielder } },
                { Enums.SetPieceRole.EdgeOfBoxSweeper, new List<Enums.Positions> { Enums.Positions.CentralDefendingMidfielder, Enums.Positions.CentralMidfielder } }
            },
            preferredZoneCoverage: new Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone>
            {
                // PostBlockers' zones are implicit by the role
                { Enums.SetPieceRole.ZonalMarker, Enums.ConceptualSetPieceZone.OpponentPenaltyArea_General }, // Cover the main penalty area
                { Enums.SetPieceRole.EdgeOfBoxSweeper, Enums.ConceptualSetPieceZone.EdgeOfBox },
                { Enums.SetPieceRole.CounterAttackOutlet, Enums.ConceptualSetPieceZone.Midfield_Center }
            },
            attackerRoleToDefenderRoleMapping: new Dictionary<Enums.SetPieceRole, Enums.SetPieceRole>() // Empty for pure zonal default
        );
    }

    private static DefensiveSetPieceRoutine CreateDefaultDefensiveFreeKickRoutine()
    {
        return new DefensiveSetPieceRoutine(
            name: "Default Free Kick Defense",
            description: "A standard zonal defense for indirect free kicks, covering aerial threats and rebound areas.",
            appliesToType: Enums.SetPieceType.FreeKick,
            strategy: Enums.DefensiveStrategy.ZonalMarking,
            usePostBlockers: false,
            roleAssignments: new Dictionary<Enums.SetPieceRole, List<Enums.Positions>>
            {
                { Enums.SetPieceRole.WallPlayer, new List<Enums.Positions> { Enums.Positions.CentralMidfielder, Enums.Positions.Striker, Enums.Positions.LeftWingForward, Enums.Positions.RightWingForward } }, // Players forming the wall
                { Enums.SetPieceRole.ZonalMarker, new List<Enums.Positions> { Enums.Positions.CenterBack, Enums.Positions.CentralDefendingMidfielder, Enums.Positions.CentralMidfielder } },
                { Enums.SetPieceRole.EdgeOfBoxSweeper, new List<Enums.Positions> { Enums.Positions.CentralDefendingMidfielder, Enums.Positions.CentralMidfielder } },
                { Enums.SetPieceRole.CounterAttackOutlet, new List<Enums.Positions> { Enums.Positions.LeftWingForward, Enums.Positions.RightWingForward, Enums.Positions.Striker } }
            },
            preferredZoneCoverage: new Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone>
            {
                // WallPlayer's conceptual zone is implicitly 'near the ball location for the wall'
                { Enums.SetPieceRole.ZonalMarker, Enums.ConceptualSetPieceZone.OpponentPenaltyArea_Central }, // Covering the heart of the box
                { Enums.SetPieceRole.EdgeOfBoxSweeper, Enums.ConceptualSetPieceZone.EdgeOfBox }, // Covering just outside the box
                { Enums.SetPieceRole.CounterAttackOutlet, Enums.ConceptualSetPieceZone.Midfield_Center } // Midfield for counter-attacks
            },
            attackerRoleToDefenderRoleMapping: new Dictionary<Enums.SetPieceRole, Enums.SetPieceRole>() // Minimal direct mapping for zonal defense
        );
    }

    private static DefensiveSetPieceRoutine CreateDefaultDefensiveThrowInRoutine()
    {
        return new DefensiveSetPieceRoutine(
            name: "Default Throw-In Defense",
            description: "Hybrid marking for throw-ins, with focus on immediate threats near the ball and covering deeper zones.",
            appliesToType: Enums.SetPieceType.ThrowIn,
            strategy: Enums.DefensiveStrategy.HybridMarking,
            usePostBlockers: false,
            roleAssignments: new Dictionary<Enums.SetPieceRole, List<Enums.Positions>>
            {
                { Enums.SetPieceRole.ManMarker, new List<Enums.Positions> { Enums.Positions.LeftBack, Enums.Positions.RightBack, Enums.Positions.CentralMidfielder, Enums.Positions.CentralDefendingMidfielder } },
                { Enums.SetPieceRole.ZonalMarker, new List<Enums.Positions> { Enums.Positions.CentralMidfielder, Enums.Positions.CenterBack } },
                { Enums.SetPieceRole.DeepDefender, new List<Enums.Positions> { Enums.Positions.CenterBack, Enums.Positions.LeftBack, Enums.Positions.RightBack} }, // Covering deeper threats, especially near far post
                { Enums.SetPieceRole.CounterAttackOutlet, new List<Enums.Positions> { Enums.Positions.LeftWingForward, Enums.Positions.RightWingForward, Enums.Positions.Striker } }
            },
            preferredZoneCoverage: new Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone>
            {
                // ManMarker's exact position is dynamic, but they operate broadly in the attacking third flank
                { Enums.SetPieceRole.ZonalMarker, Enums.ConceptualSetPieceZone.OpponentAttackingThird_Flank }, // Covering the wider area where throw-ins occur, or just outside the box near the flank
                { Enums.SetPieceRole.DeepDefender, Enums.ConceptualSetPieceZone.OpponentPenaltyArea_Central }, // Covering deep inside the box if the throw is long
                { Enums.SetPieceRole.CounterAttackOutlet, Enums.ConceptualSetPieceZone.Midfield_Center } // Central midfield for counter-attacks
            },
            attackerRoleToDefenderRoleMapping: new Dictionary<Enums.SetPieceRole, Enums.SetPieceRole>
            {
                { Enums.SetPieceRole.ShortOptionReceiver, Enums.SetPieceRole.ManMarker },
                { Enums.SetPieceRole.TargetMan, Enums.SetPieceRole.ManMarker },
                { Enums.SetPieceRole.BoxAttacker, Enums.SetPieceRole.ManMarker }
            }
        );
    }
}