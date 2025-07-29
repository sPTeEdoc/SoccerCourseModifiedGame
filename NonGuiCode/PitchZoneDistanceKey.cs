using FunnyOldGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct PitchZoneDistanceKey : IEquatable<PitchZoneDistanceKey>
{
    public readonly Enums.PitchZone Zone1;
    public readonly Enums.PitchZone Zone2;
    public readonly bool IsHomeTeamPerspective;

    public PitchZoneDistanceKey(Enums.PitchZone zone1, Enums.PitchZone zone2, bool isHomeTeamPerspective)
    {
        Zone1 = zone1;
        Zone2 = zone2;
        IsHomeTeamPerspective = isHomeTeamPerspective;
    }

    public override bool Equals(object obj) => obj is PitchZoneDistanceKey other && Equals(other);

    public bool Equals(PitchZoneDistanceKey other)
    {
        return Zone1 == other.Zone1 &&
               Zone2 == other.Zone2 &&
               IsHomeTeamPerspective == other.IsHomeTeamPerspective;
    }

    public override int GetHashCode()
    {
        // A common and efficient way to combine hash codes.
        // The order of combining matters for good distribution.
        unchecked // Allows arithmetic overflow without throwing an exception
        {
            int hash = 17; // A prime number
            hash = hash * 23 + (int)Zone1; // Use the int value of the enum
            hash = hash * 23 + (int)Zone2;
            hash = hash * 23 + IsHomeTeamPerspective.GetHashCode();
            return hash;
        }
    }

    // Optional: Override == and != operators for convenience
    public static bool operator ==(PitchZoneDistanceKey left, PitchZoneDistanceKey right) => left.Equals(right);
    public static bool operator !=(PitchZoneDistanceKey left, PitchZoneDistanceKey right) => !(left == right);
}
