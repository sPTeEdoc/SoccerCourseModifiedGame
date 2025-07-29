using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ZoneCoordinate : IEquatable<ZoneCoordinate>
{
    public int X { get; set; }
    public int Y { get; set; }

    public ZoneCoordinate(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as ZoneCoordinate);
    }

    public bool Equals(ZoneCoordinate other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return X == other.X && Y == other.Y;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + X.GetHashCode();
            hash = hash * 23 + Y.GetHashCode();
            return hash;
        }
    }

    public override string ToString()
    {
        return "({X}, {Y})";
    }

    public static bool operator ==(ZoneCoordinate left, ZoneCoordinate right)
    {
        if (ReferenceEquals(left, null))
        {
            return ReferenceEquals(right, null);
        }
        return left.Equals(right);
    }

    public static bool operator !=(ZoneCoordinate left, ZoneCoordinate right)
    {
        return !(left == right);
    }
}