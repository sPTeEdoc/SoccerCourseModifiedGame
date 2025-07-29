using FunnyOldGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Key for the zone distance cache. Immutable for good dictionary performance.
/// </summary>
public struct ZoneDistanceKey : IEquatable<ZoneDistanceKey>
{
    public readonly Enums.PitchZone Zone1;
    public readonly Enums.PitchZone Zone2;
    public readonly bool IsHomeTeamPerspective;

    public ZoneDistanceKey(Enums.PitchZone z1, Enums.PitchZone z2, bool isHome)
    {
        Zone1 = z1;
        Zone2 = z2;
        IsHomeTeamPerspective = isHome;
    }

    public override bool Equals(object obj) => obj is ZoneDistanceKey other && Equals(other);
    public bool Equals(ZoneDistanceKey other) =>
        Zone1 == other.Zone1 &&
        Zone2 == other.Zone2 &&
        IsHomeTeamPerspective == other.IsHomeTeamPerspective;

    public override int GetHashCode()
    {
        unchecked // Allow arithmetic overflow to wrap around.
        {
            int hash = 17;
            hash = hash * 23 + (int)Zone1;
            hash = hash * 23 + (int)Zone2;
            hash = hash * 23 + IsHomeTeamPerspective.GetHashCode();
            return hash;
        }
    }
}
