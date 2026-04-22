using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PremierVision.Data;
using PremierVision.Models;
using PremierVision.Options;

namespace PremierVision.Services;

public class ApiFootballSyncService(
    HttpClient httpClient,
    IOptions<ApiFootballOptions> options,
    AppDbContext context) : IApiFootballSyncService
{
    private readonly ApiFootballOptions _options = options.Value;

    public async Task<int> SyncTeamsAsync(CancellationToken cancellationToken = default)
    {
        EnsureApiKey();

        using var document = await GetAsync(
            $"teams?league={_options.PremierLeagueId}&season={_options.Season}",
            cancellationToken);

        var existingTeams = await context.Teams.ToListAsync(cancellationToken);
        var count = 0;
        foreach (var item in document.RootElement.GetProperty("response").EnumerateArray())
        {
            var teamJson = item.GetProperty("team");
            var venueJson = item.GetProperty("venue");
            var apiTeamId = teamJson.GetProperty("id").GetInt32();
            var teamName = teamJson.GetProperty("name").GetString() ?? string.Empty;
            var teamCode = teamJson.TryGetProperty("code", out var codeProp)
                ? codeProp.GetString()
                : null;

            var entity = existingTeams.FirstOrDefault(x => x.ApiFootballTeamId == apiTeamId)
                ?? existingTeams.FirstOrDefault(x =>
                    x.ApiFootballTeamId == null &&
                    (x.Name == teamName ||
                     (!string.IsNullOrWhiteSpace(teamCode) && x.Code == teamCode) ||
                     (!string.IsNullOrWhiteSpace(teamCode) && x.ShortName == teamCode)));

            if (entity is null)
            {
                entity = new Team();
                context.Teams.Add(entity);
                existingTeams.Add(entity);
            }

            entity.ApiFootballTeamId = apiTeamId;
            entity.Name = teamName;
            entity.ShortName = teamCode ?? entity.Name;
            entity.Code = teamCode ?? entity.ShortName;
            entity.LogoUrl = teamJson.TryGetProperty("logo", out var logoProp)
                ? logoProp.GetString()
                : null;
            entity.StadiumName = venueJson.TryGetProperty("name", out var venueNameProp)
                ? venueNameProp.GetString()
                : null;
            entity.StadiumCity = venueJson.TryGetProperty("city", out var venueCityProp)
                ? venueCityProp.GetString()
                : null;

            count++;
        }

        await context.SaveChangesAsync(cancellationToken);
        return count;
    }

    public async Task<int> SyncFixturesAsync(CancellationToken cancellationToken = default)
    {
        EnsureApiKey();

        if (!await context.Teams.AnyAsync(x => x.ApiFootballTeamId.HasValue, cancellationToken))
        {
            await SyncTeamsAsync(cancellationToken);
        }

        using var document = await GetAsync(
            $"fixtures?league={_options.PremierLeagueId}&season={_options.Season}",
            cancellationToken);

        var teams = await context.Teams
            .Where(x => x.ApiFootballTeamId.HasValue)
            .ToDictionaryAsync(x => x.ApiFootballTeamId!.Value, cancellationToken);

        var count = 0;
        foreach (var item in document.RootElement.GetProperty("response").EnumerateArray())
        {
            var fixtureJson = item.GetProperty("fixture");
            var leagueJson = item.GetProperty("league");
            var teamsJson = item.GetProperty("teams");
            var goalsJson = item.GetProperty("goals");
            var scoreJson = item.GetProperty("score");

            var apiFixtureId = fixtureJson.GetProperty("id").GetInt32();
            var homeApiTeamId = teamsJson.GetProperty("home").GetProperty("id").GetInt32();
            var awayApiTeamId = teamsJson.GetProperty("away").GetProperty("id").GetInt32();

            if (!teams.TryGetValue(homeApiTeamId, out var homeTeam) || !teams.TryGetValue(awayApiTeamId, out var awayTeam))
            {
                continue;
            }

            var kickoffUtc = fixtureJson.GetProperty("date").GetDateTime();
            var entity = await context.Fixtures.FirstOrDefaultAsync(
                x => x.ApiFootballFixtureId == apiFixtureId,
                cancellationToken);

            entity ??= await context.Fixtures.FirstOrDefaultAsync(
                x => x.ApiFootballFixtureId == null &&
                     x.HomeTeamId == homeTeam.Id &&
                     x.AwayTeamId == awayTeam.Id &&
                     x.KickoffUtc == kickoffUtc,
                cancellationToken);

            if (entity is null)
            {
                entity = new Fixture();
                context.Fixtures.Add(entity);
            }

            entity.ApiFootballFixtureId = apiFixtureId;
            entity.HomeTeamId = homeTeam.Id;
            entity.AwayTeamId = awayTeam.Id;
            entity.MatchWeek = ParseMatchWeek(leagueJson.TryGetProperty("round", out var roundProp)
                ? roundProp.GetString()
                : null);
            entity.KickoffUtc = kickoffUtc;
            entity.VenueName = fixtureJson.GetProperty("venue").TryGetProperty("name", out var venueName)
                ? venueName.GetString() ?? string.Empty
                : string.Empty;
            entity.ImageUrl = homeTeam.LogoUrl;
            entity.Status = ParseStatus(fixtureJson.GetProperty("status").GetProperty("short").GetString());
            entity.HomeFullTimeScore = GetNullableInt(goalsJson, "home");
            entity.AwayFullTimeScore = GetNullableInt(goalsJson, "away");

            if (scoreJson.TryGetProperty("halftime", out var halfTimeJson))
            {
                entity.HomeHalfTimeScore = GetNullableInt(halfTimeJson, "home");
                entity.AwayHalfTimeScore = GetNullableInt(halfTimeJson, "away");
            }

            count++;
        }

        await context.SaveChangesAsync(cancellationToken);
        return count;
    }

    public async Task<bool> SyncFixtureDetailsAsync(int fixtureId, CancellationToken cancellationToken = default)
    {
        EnsureApiKey();

        var fixture = await context.Fixtures
            .Include(x => x.HomeTeam)
            .Include(x => x.AwayTeam)
            .FirstOrDefaultAsync(x => x.Id == fixtureId, cancellationToken);

        if (fixture is null || !fixture.ApiFootballFixtureId.HasValue || fixture.Status == FixtureStatus.NotStarted)
        {
            return false;
        }

        using var eventsDocument = await GetAsync(
            $"fixtures/events?fixture={fixture.ApiFootballFixtureId.Value}",
            cancellationToken);

        using var statisticsDocument = await GetAsync(
            $"fixtures/statistics?fixture={fixture.ApiFootballFixtureId.Value}",
            cancellationToken);

        var teams = await context.Teams
            .Where(x => x.ApiFootballTeamId.HasValue)
            .ToDictionaryAsync(x => x.ApiFootballTeamId!.Value, cancellationToken);

        var existingEvents = await context.MatchEvents
            .Where(x => x.FixtureId == fixture.Id)
            .ToListAsync(cancellationToken);

        var existingStatistics = await context.MatchStatistics
            .Where(x => x.FixtureId == fixture.Id)
            .ToListAsync(cancellationToken);

        if (existingEvents.Count > 0)
        {
            context.MatchEvents.RemoveRange(existingEvents);
        }

        if (existingStatistics.Count > 0)
        {
            context.MatchStatistics.RemoveRange(existingStatistics);
        }

        foreach (var item in eventsDocument.RootElement.GetProperty("response").EnumerateArray())
        {
            var elapsed = item.GetProperty("time").TryGetProperty("elapsed", out var elapsedProp) && elapsedProp.ValueKind != JsonValueKind.Null
                ? elapsedProp.GetInt32()
                : 0;

            var teamId = item.GetProperty("team").TryGetProperty("id", out var teamIdProp) && teamIdProp.ValueKind != JsonValueKind.Null
                ? teamIdProp.GetInt32()
                : (int?)null;

            var playerName = item.GetProperty("player").TryGetProperty("name", out var playerNameProp)
                ? playerNameProp.GetString() ?? string.Empty
                : string.Empty;

            var assistName = item.TryGetProperty("assist", out var assistProp) &&
                             assistProp.ValueKind != JsonValueKind.Null &&
                             assistProp.TryGetProperty("name", out var assistNameProp)
                ? assistNameProp.GetString()
                : null;

            var type = item.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : null;
            var detail = item.TryGetProperty("detail", out var detailProp) ? detailProp.GetString() : null;
            var comments = item.TryGetProperty("comments", out var commentsProp) ? commentsProp.GetString() : null;

            if (!TryMapEventType(type, detail, out var eventType))
            {
                continue;
            }

            var description = BuildEventDescription(eventType, detail, assistName, comments);

            context.MatchEvents.Add(new MatchEvent
            {
                FixtureId = fixture.Id,
                TeamId = teamId.HasValue && teams.TryGetValue(teamId.Value, out var team) ? team.Id : null,
                Minute = elapsed,
                PlayerName = playerName,
                EventType = eventType,
                Description = description
            });
        }

        foreach (var teamStats in statisticsDocument.RootElement.GetProperty("response").EnumerateArray())
        {
            var apiTeamId = teamStats.GetProperty("team").GetProperty("id").GetInt32();
            if (!teams.TryGetValue(apiTeamId, out var team))
            {
                continue;
            }

            foreach (var statItem in teamStats.GetProperty("statistics").EnumerateArray())
            {
                var name = statItem.GetProperty("type").GetString();
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var valueElement = statItem.GetProperty("value");
                if (valueElement.ValueKind == JsonValueKind.Null)
                {
                    continue;
                }

                var value = valueElement.ValueKind switch
                {
                    JsonValueKind.String => valueElement.GetString(),
                    JsonValueKind.Number => valueElement.ToString(),
                    JsonValueKind.True => "True",
                    JsonValueKind.False => "False",
                    _ => null
                };

                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                context.MatchStatistics.Add(new MatchStatistic
                {
                    FixtureId = fixture.Id,
                    TeamId = team.Id,
                    Name = name,
                    Value = value
                });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<JsonDocument> GetAsync(string path, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("x-apisports-key", _options.ApiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
    }

    private void EnsureApiKey()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("API-Football key tanimli degil.");
        }
    }

    private static int ParseMatchWeek(string? round)
    {
        if (string.IsNullOrWhiteSpace(round))
        {
            return 0;
        }

        var digits = new string(round.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, CultureInfo.InvariantCulture, out var week) ? week : 0;
    }

    private static FixtureStatus ParseStatus(string? shortStatus)
    {
        return shortStatus switch
        {
            "FT" or "AET" or "PEN" => FixtureStatus.Completed,
            "NS" or "TBD" or "PST" => FixtureStatus.NotStarted,
            _ => FixtureStatus.Live
        };
    }

    private static int? GetNullableInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return property.GetInt32();
    }

    private static bool TryMapEventType(string? type, string? detail, out MatchEventType eventType)
    {
        if (string.Equals(type, "Goal", StringComparison.OrdinalIgnoreCase))
        {
            eventType = MatchEventType.Goal;
            return true;
        }

        if (string.Equals(type, "subst", StringComparison.OrdinalIgnoreCase))
        {
            eventType = MatchEventType.Substitution;
            return true;
        }

        if (string.Equals(type, "Card", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(detail, "Yellow Card", StringComparison.OrdinalIgnoreCase))
            {
                eventType = MatchEventType.YellowCard;
                return true;
            }

            if (string.Equals(detail, "Red Card", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(detail, "Yellow Red Card", StringComparison.OrdinalIgnoreCase))
            {
                eventType = MatchEventType.RedCard;
                return true;
            }
        }

        eventType = default;
        return false;
    }

    private static string? BuildEventDescription(
        MatchEventType eventType,
        string? detail,
        string? assistName,
        string? comments)
    {
        return eventType switch
        {
            MatchEventType.Substitution => string.IsNullOrWhiteSpace(assistName)
                ? detail
                : $"Cikan: {assistName}",
            MatchEventType.Goal => string.Join(" | ", new[] { detail, assistName is null ? null : $"Asist: {assistName}", comments }
                .Where(x => !string.IsNullOrWhiteSpace(x))),
            _ => string.Join(" | ", new[] { detail, comments }.Where(x => !string.IsNullOrWhiteSpace(x)))
        };
    }
}
