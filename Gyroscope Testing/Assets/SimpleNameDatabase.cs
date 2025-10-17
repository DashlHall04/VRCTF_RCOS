using System.Collections.Generic;

public static class SimpleNameDatabase
{
    private static readonly HashSet<string> _names = new HashSet<string>();
    public static int Count => _names.Count;

    public static void AddName(string name)
    {
        if (!string.IsNullOrEmpty(name)) _names.Add(name);
    }

    public static IEnumerable<string> AllNames() => _names;
}
