using System;
using System.Collections.Concurrent;

public static class EnumCache<T> where T : struct
{
    private static readonly ConcurrentDictionary<T, string> _cache = new ConcurrentDictionary<T, string>();
    private static readonly string _nullValue = "null";

    public static string ToString(T? value)
    {
        if (!value.HasValue)
        {
            return _nullValue;
        }

        return _cache.GetOrAdd(value.Value, v => v.ToString());
    }
}
