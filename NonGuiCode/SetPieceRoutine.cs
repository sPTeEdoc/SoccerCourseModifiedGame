using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// In a new file, e.g., SetPieceRoutine.cs or BaseSetPieceRoutine.cs
using System.Collections.Generic;
using FunnyOldGame;

public abstract class SetPieceRoutine
{
    // Common properties for all routines
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public Enums.SetPieceType AppliesToSetPieceType { get; protected set; }

    // Is this routine specifically for a left or right flank?
    // If null, it applies to both (or is not flank-specific, like a penalty).
    public Enums.Flank? ApplicableFlank { get; protected set; }

    // Should the routine AI consider if it's the home or away team?
    // Useful if you have routines that are only for home team, or away team
    public bool IsHomeTeamSpecific { get; protected set; }
    public bool IsAwayTeamSpecific { get; protected set; }

    // Constructor to ensure basic properties are set upon creation
    protected SetPieceRoutine(string name, string description, Enums.SetPieceType appliesToType, Enums.Flank? applicableFlank = null, bool isHomeTeamSpecific = false, bool isAwayTeamSpecific = false)
    {
        Name = name;
        Description = description;
        AppliesToSetPieceType = appliesToType;
        ApplicableFlank = applicableFlank;
        IsHomeTeamSpecific = isHomeTeamSpecific;
        IsAwayTeamSpecific = isAwayTeamSpecific;
    }

    // Abstract method: Forces derived classes to implement how players are assigned to conceptual zones
    // This could also be a method that returns a dictionary directly from the routine.
    // For now, let's keep it simple as a property that derived classes set.
    public abstract Dictionary<Enums.SetPieceRole, Enums.ConceptualSetPieceZone> GetConceptualPositioningMap();

    // You could also add common validation or utility methods here
    // For example, a method to check if the routine is applicable given certain match conditions.
    public virtual bool IsApplicable(Enums.SetPieceType currentSetPieceType, Enums.Flank currentFlank, bool isAttackingTeamHome)
    {
        if (currentSetPieceType != AppliesToSetPieceType)
        {
            return false;
        }

        if (ApplicableFlank.HasValue && ApplicableFlank.Value != currentFlank)
        {
            return false;
        }

        if (IsHomeTeamSpecific && !isAttackingTeamHome)
        {
            return false;
        }

        if (IsAwayTeamSpecific && isAttackingTeamHome)
        {
            return false;
        }

        return true;
    }
}
