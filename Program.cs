using SecretSanta;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/api/santa/register", (RegisterRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new { error = "Имя не может быть пустым" });

    string name = request.Name.Trim();

    if (DataStore.Assignments.TryGetValue(name, out var existingGiftFor))
        return Results.Ok(new { userName = name, giftFor = existingGiftFor });

    if (DataStore.Participants.Any(p => string.Equals(p, name, StringComparison.OrdinalIgnoreCase)))
    {
        return Results.Ok(new { userName = name, giftFor = (string?)null });
    }

    DataStore.TryAddParticipant(name);

    if (DataStore.Participants.Count >= 2)
        DataStore.Redistribute();

    DataStore.Assignments.TryGetValue(name, out var giftFor);
    return Results.Ok(new { userName = name, giftFor });
});

app.MapPost("/api/santa/wish", (WishRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new { error = "Имя не может быть пустым" });

    if (string.IsNullOrWhiteSpace(request.Wish))
        return Results.BadRequest(new { error = "Пожелание не может быть пустым" });

    string name = request.Name.Trim();

    if (!DataStore.Participants.Any(p => string.Equals(p, name, StringComparison.OrdinalIgnoreCase)))
        return Results.NotFound(new { error = "Пользователь не найден" });

    DataStore.Wishes[name] = request.Wish;
    return Results.Ok(new { status = "wish saved" });
});

app.MapGet("/api/santa/wish/{name}", (string name) =>
{
    if (string.IsNullOrWhiteSpace(name))
        return Results.BadRequest(new { error = "Имя не может быть пустым" });

    var realName = DataStore.Participants.FirstOrDefault(p =>
        string.Equals(p, name, StringComparison.OrdinalIgnoreCase));

    if (realName == null)
        return Results.NotFound(new { error = "Пользователь не найден" });

    var wish = DataStore.Wishes.GetValueOrDefault(realName);
    return Results.Ok(new { name = realName, wish = wish ?? "" });
});

app.Run();

public record RegisterRequest(string Name);
public record WishRequest(string Name, string Wish);