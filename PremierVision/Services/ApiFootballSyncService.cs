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

        var count = 0;
        foreach (var item in document.RootElement.GetProperty("response").EnumerateArray())
        {
            var teamJson = item.GetProperty("team");
            var venueJson = item.GetProperty("venue");
            var apiTeamId = teamJson.GetProperty("id").GetInt32();

            var entity = await context.Teams.FirstOrDefaultAsync(
                x => x.ApiFootballTeamId == apiTeamId,
                cancellationToken);

            if (entity is null)
            {
                entity = new Team();
                context.Teams.Add(entity);
            }

            entity.ApiFootballTeamId = apiTeamId;
            entity.Name = teamJson.GetProperty("name").GetString() ?? string.Empty;
            entity.ShortName = teamJson.TryGetProperty("code", out var codeProp)
                ? codeProp.GetString() ?? entity.Name
                : entity.Name;
            entity.Code = teamJson.TryGetProperty("code", out var codeValue)
                ? codeValue.GetString() ?? entity.ShortName
                : entity.ShortName;
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

            var entity = await context.Fixtures.FirstOrDefaultAsync(
                x => x.ApiFootballFixtureId == apiFixtureId,
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
            entity.KickoffUtc = fixtureJson.GetProperty("date").GetDateTime();
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
}
