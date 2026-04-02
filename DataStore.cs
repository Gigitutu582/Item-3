using System.Collections.Concurrent;

namespace SecretSanta;

public static class DataStore
{
    public static List<string> Participants { get; } = new();
    public static ConcurrentDictionary<string, string> Assignments { get; } = new();
    public static ConcurrentDictionary<string, string> Wishes { get; } = new();
    public static object Lock { get; } = new object();   
}