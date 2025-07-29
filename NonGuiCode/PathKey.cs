using FunnyOldGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Key for the zone path cache. Immutable for good dictionary performance.
/// </summary>
public struct PathKey : IEquatable<PathKey>
{
    public readonly Enums.PitchZone StartZone;
    public readonly Enums.PitchZone EndZone;
    public readonly bool IsHomeTeamPerspective;

    public PathKey(Enums.PitchZone start, Enums.PitchZone end, bool isHome)
    {
        StartZone = start;
        EndZone = end;
        IsHomeTeamPerspective = isHome;
    }

    public override bool Equals(object obj) => obj is PathKey other && Equals(other);
    public bool Equals(PathKey other) =>
        StartZone == other.StartZone &&
        EndZone == other.EndZone &&
        IsHomeTeamPerspective == other.IsHomeTeamPerspective;

    public override int GetHashCode()
    {
        unchecked // Allow arithmetic overflow to wrap around.
        {
            int hash = 17;
            hash = hash * 23 + (int)StartZone;
            hash = hash * 23 + (int)EndZone;
            hash = hash * 23 + IsHomeTeamPerspective.GetHashCode();
            return hash;
        }
    }
}