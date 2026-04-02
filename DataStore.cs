using System.Collections.Concurrent;

namespace SecretSanta;

public static class DataStore
{
    public static List<string> Participants { get; } = new();

    public static ConcurrentDictionary<string, string> Assignments { get; } = new();

    public static ConcurrentDictionary<string, string> Wishes { get; } = new();

    private static readonly object _lock = new();

    public static void Redistribute()
    {
        lock (_lock)
        {
            if (Participants.Count < 2) return;

            var shuffled = Participants.OrderBy(_ => Guid.NewGuid()).ToList();
            var newAssignments = new Dictionary<string, string>();

            for (int i = 0; i < shuffled.Count; i++)
            {
                var giver = shuffled[i];
                var receiver = shuffled[(i + 1) % shuffled.Count];
                newAssignments[giver] = receiver;
            }

            Assignments.Clear();
            foreach (var kv in newAssignments)
                Assignments[kv.Key] = kv.Value;
        }
    }

    public static bool TryAddParticipant(string name)
    {
        lock (_lock)
        {
            if (Participants.Any(p => string.Equals(p, name, StringComparison.OrdinalIgnoreCase)))
                return false;

            Participants.Add(name);
            return true;
        }
    }
}