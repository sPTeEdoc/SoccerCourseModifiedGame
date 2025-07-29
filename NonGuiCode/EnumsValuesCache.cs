using System;
using System.Collections.Concurrent;

public static class EnumValuesCache<T>
{
    private static readonly Lazy<T[]> _values = new Lazy<T[]>(() =>
        (T[])Enum.GetValues(typeof(T)));

    public static T[] GetValues() => _values.Value;
}