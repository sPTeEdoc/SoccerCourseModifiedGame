using FunnyOldGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct ZonePairKey : IEquatable<ZonePairKey>
{
    public Enums.PitchZone Zone1 { get; }
    public Enums.PitchZone Zone2 { get; }
    public bool IsHomePerspective { get; }

    public ZonePairKey(Enums.PitchZone zone1, Enums.PitchZone zone2, bool isHomePerspective)
    {
        Zone1 = zone1;
        Zone2 = zone2;
        IsHomePerspective = isHomePerspective;
    }

    public override bool Equals(object obj)
    {
        return obj is ZonePairKey other && Equals(other);
    }

    public bool Equals(ZonePairKey other)
    {
        // Order doesn't matter for distance (A to B is same as B to A), but might for path
        // For distance, you might want to normalize (e.g., sort zones) before comparison.
        // For path, order absolutely matters. Keep as is for now.
        return Zone1 == other.Zone1 &&
               Zone2 == other.Zone2 &&
               IsHomePerspective == other.IsHomePerspective;
    }

    public override int GetHashCode()
    {
        // Simple hash code combination. You can use a utility like HashCode.Combine in C# 8+
        // For older versions, a manual combination is needed.
        unchecked // Overflow is fine
        {
            int hash = 17;
            hash = hash * 23 + Zone1.GetHashCode();
            hash = hash * 23 + Zone2.GetHashCode();
            hash = hash * 23 + IsHomePerspective.GetHashCode();
            return hash;
        }
    }

    public static bool operator ==(ZonePairKey left, ZonePairKey right) => left.Equals(right);
    public static bool operator !=(ZonePairKey left, ZonePairKey right) => !(left == right);
}
