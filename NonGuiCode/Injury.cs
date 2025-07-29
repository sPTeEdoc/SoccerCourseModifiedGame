using FunnyOldGame;
using System;
using System.Collections.Generic;

public class Injury
{
    /// <summary>
    /// Unique identifier for the injury instance.
    /// </summary>
    public string InjuryID { get; set; }

    /// <summary>
    /// Type of injury (e.g., "Hamstring Strain", "Twisted Ankle", "Broken Arm").
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Severity of the injury (e.g., "Minor", "Moderate", "Serious", "Career-Ending").
    /// This can affect duration and potential lingering effects.
    /// </summary>
    public string Severity { get; set; }

    public Enums.InjurySeverity InjurySeverity {get; set;}

    public Enums.InjuryType InjuryType { get; set; }

    /// <summary>
    /// The original estimated duration of the injury in days.
    /// </summary>
    public int OriginalDurationDays { get; set; }

    /// <summary>
    /// The remaining duration of the injury in days. Decrements over time.
    /// </summary>
    public int RemainingDurationDays { get; set; }

    /// <summary>
    /// The date when the injury occurred.
    /// </summary>
    public DateTime DateOccurred { get; set; }

    /// <summary>
    /// Any long-term negative effect on player attributes or injury proneness after recovery.
    /// (Optional, but adds realism for serious injuries).
    /// </summary>
    public Dictionary<string, double> LingeringEffects { get; set; } // e.g., "SprintSpeed": -2, "InjuryProneness": +5

    public Injury(string type, string severity, int durationDays, Enums.InjuryType injuryType, Enums.InjurySeverity injurySeverity)
    {
        // For simplicity, generate a unique ID (in a real game, you might use GUIDs or a sequence)
        InjuryID = Guid.NewGuid().ToString();
        Type = type;
        Severity = severity;
        OriginalDurationDays = durationDays;
        RemainingDurationDays = durationDays;
        DateOccurred = DateTime.Now; // Or whatever your game's current date is
        LingeringEffects = new Dictionary<string, double>();
        InjuryType = injuryType;
        InjurySeverity = injurySeverity;
    }

    /// <summary>
    /// Advances the recovery by one day.
    /// </summary>
    public void AdvanceRecoveryDay()
    {
        RemainingDurationDays = Math.Max(0, RemainingDurationDays - 1);
    }

    /// <summary>
    /// Checks if the injury has fully recovered.
    /// </summary>
    public bool IsRecovered()
    {
        return RemainingDurationDays <= 0;
    }
}