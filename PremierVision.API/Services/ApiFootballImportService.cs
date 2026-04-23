using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PremierVision.Data;
using PremierVision.Models;
using PremierVision.Options;

namespace PremierVision.Services;

public class ApiFootballImportService(
    HttpClient httpClient,
    IOptions<ApiFootballOptions> options,
    AppDbContext context) : IApiFootballImportService
{
    private readonly ApiFootballOptions _options = options.Value;

    public async Task<int> ImportTeamsAsync(CancellationToken cancellationToken = default)
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
                ?? existingTeams.FirstOrDefault(x => x.Name == teamName);

            if (entity is null)
            {
                entity = new Team();
                context.Teams.Add(entity);
                existingTeams.Add(entity);
            }

            entity.ApiFootballTeamId = apiTeamId;
            entity.Name = teamName;
            entity.ShortName = teamCode ?? teamName;
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

    public async Task<int> ImportFixturesAsync(CancellationToken cancellationToken = default)
    {
        EnsureApiKey();
        await EnsureTeamsAsync(cancellationToken);

        using var document = await GetAsync(
            $"fixtures?league={_options.PremierLeagueId}&season={_options.Season}",
            cancellationToken);

        return await ImportFixturePayloadAsync(document.RootElement, cancellationToken);
    }

    public async Task<int> ImportLiveFixturesAsync(CancellationToken cancellationToken = default)
    {
        EnsureApiKey();
        await EnsureTeamsAsync(cancellationToken);

        using var document = await GetAsync("fixtures?live=all", cancellationToken);
        return await ImportFixturePayloadAsync(
            document.RootElement,
            cancellationToken,
            filterByLeague: true);
    }

    private async Task<int> ImportFixturePayloadAsync(
        JsonElement root,
        CancellationToken cancellationToken,
        bool filterByLeague = false)
    {
        var teams = await context.Teams
            .Where(x => x.ApiFootballTeamId.HasValue)
            .ToDictionaryAsync(x => x.ApiFootballTeamId!.Value, cancellationToken);

        var fixtures = await context.Fixtures.ToListAsync(cancellationToken);
        var count = 0;

        foreach (var item in root.GetProperty("response").EnumerateArray())
        {
            var fixtureJson = item.GetProperty("fixture");
            var leagueJson = item.GetProperty("league");
            if (filterByLeague && leagueJson.GetProperty("id").GetInt32() != _options.PremierLeagueId)
            {
                continue;
            }

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

            var entity = fixtures.FirstOrDefault(x => x.ApiFootballFixtureId == apiFixtureId)
                ?? fixtures.FirstOrDefault(x =>
                    x.HomeTeamId == homeTeam.Id &&
                    x.AwayTeamId == awayTeam.Id &&
                    x.KickoffUtc == kickoffUtc);

            if (entity is null)
            {
                entity = new Fixture();
                context.Fixtures.Add(entity);
                fixtures.Add(entity);
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

            if (scoreJson.TryGetProperty("halftime", out var halftimeJson))
            {
                entity.HomeHalfTimeScore = GetNullableInt(halftimeJson, "home");
                entity.AwayHalfTimeScore = GetNullableInt(halftimeJson, "away");
            }

            count++;
        }

        await context.SaveChangesAsync(cancellationToken);
        return count;
    }

    private async Task EnsureTeamsAsync(CancellationToken cancellationToken)
    {
        if (!await context.Teams.AnyAsync(x => x.ApiFootballTeamId.HasValue, cancellationToken))
        {
            await ImportTeamsAsync(cancellationToken);
        }
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
