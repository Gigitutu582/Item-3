using SecretSanta;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

static void AddParticipantAndRedistribute(string name)
{
    lock (DataStore.Lock)
    {
        if (DataStore.Participants.Any(p => string.Equals(p, name, StringComparison.OrdinalIgnoreCase)))
            return;

        DataStore.Participants.Add(name);

        if (DataStore.Participants.Count >= 2)
        {
            var shuffled = DataStore.Participants.OrderBy(_ => Guid.NewGuid()).ToList();
            var newAssignments = new Dictionary<string, string>();

            for (int i = 0; i < shuffled.Count; i++)
            {
                var giver = shuffled[i];
                var receiver = shuffled[(i + 1) % shuffled.Count];
                newAssignments[giver] = receiver;
            }

            DataStore.Assignments.Clear();
            foreach (var kv in newAssignments)
                DataStore.Assignments[kv.Key] = kv.Value;
        }
    }
}


app.MapGet("/api/santa/register", () => Results.Json(new { error = "Этот эндпоинт принимает только POST-запросы. Используйте POST с JSON { \"name\": \"Ваше имя\" }" }, statusCode: 405));

app.MapPost("/api/santa/register", (RegisterRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new { error = "Имя не может быть пустым" });

    string name = request.Name.Trim();

    lock (DataStore.Lock)
    {
        if (DataStore.Assignments.TryGetValue(name, out var existingGiftFor))
            return Results.Ok(new { userName = name, giftFor = existingGiftFor });

        if (DataStore.Participants.Any(p => string.Equals(p, name, StringComparison.OrdinalIgnoreCase)))
            return Results.Ok(new { userName = name, giftFor = (string?)null });

        AddParticipantAndRedistribute(name);

        DataStore.Assignments.TryGetValue(name, out var giftFor);
        return Results.Ok(new { userName = name, giftFor });
    }
});

app.MapGet("/api/santa/wish", () => Results.Json(new { error = "Укажите имя в пути: /api/santa/wish/{name}" }, statusCode: 404));

app.MapPost("/api/santa/wish", (WishRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new { error = "Имя не может быть пустым" });

    if (string.IsNullOrWhiteSpace(request.Wish))
        return Results.BadRequest(new { error = "Пожелание не может быть пустым" });

    string name = request.Name.Trim();

    lock (DataStore.Lock)
    {
        if (!DataStore.Participants.Any(p => string.Equals(p, name, StringComparison.OrdinalIgnoreCase)))
            return Results.NotFound(new { error = "Пользователь не найден" });

        DataStore.Wishes[name] = request.Wish;
        return Results.Ok(new { status = "wish saved" });
    }
});

app.MapGet("/api/santa/wish/{name}", (string name) =>
{
    if (string.IsNullOrWhiteSpace(name))
        return Results.BadRequest(new { error = "Имя не может быть пустым" });

    lock (DataStore.Lock)
    {
        var realName = DataStore.Participants.FirstOrDefault(p =>
            string.Equals(p, name, StringComparison.OrdinalIgnoreCase));

        if (realName == null)
            return Results.NotFound(new { error = "Пользователь не найден" });

        DataStore.Wishes.TryGetValue(realName, out var wish);
        return Results.Ok(new { name = realName, wish = wish ?? "" });
    }
});

app.Run();

public record RegisterRequest(string Name);
public record WishRequest(string Name, string Wish);