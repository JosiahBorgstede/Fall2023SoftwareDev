namespace Q.Common;

public class PlacementEnumerableComparer : IEqualityComparer<IEnumerable<Placement>>
{
    public bool Equals(IEnumerable<Placement> lhs, IEnumerable<Placement> rhs)
    {
        if (ReferenceEquals(lhs, rhs)) return true;
        if (lhs is null || rhs is null) return false;

        return lhs
            .OrderBy(p => p.Coordinate.X).ThenBy(p => p.Coordinate.Y)
            .SequenceEqual(rhs.OrderBy(p => p.Coordinate.X).ThenBy(p => p.Coordinate.Y));
    }

    public int GetHashCode(IEnumerable<Placement> e)
    {
        return e.Count();
    }
}
