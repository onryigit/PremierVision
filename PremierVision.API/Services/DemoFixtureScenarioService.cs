using Microsoft.EntityFrameworkCore;
using PremierVision.Data;
using PremierVision.Models;

namespace PremierVision.Services;

public class DemoFixtureScenarioService(AppDbContext context) : IDemoFixtureScenarioService
{
    private static readonly string[] DefaultStatisticNames =
    [
        "Ball Possession",
        "Total Shots",
        "Shots on Goal",
        "Corner Kicks",
        "Offsides",
        "Fouls",
        "Yellow Cards",
        "Passes %",
        "Expected Goals"
    ];

    public async Task<object> EnsureScenarioAsync(CancellationToken cancellationToken = default)
    {
        var teams = await context.Teams
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        if (teams.Count < 10)
        {
            throw new InvalidOperationException("Demo senaryosu icin yeterli takim verisi bulunamadi.");
        }

        var existingDemoFixtures = await context.Fixtures
            .Include(x => x.Events)
            .Include(x => x.Statistics)
            .Where(x => x.ApiFootballFixtureId.HasValue && x.ApiFootballFixtureId < 0)
            .ToListAsync(cancellationToken);

        if (existingDemoFixtures.Count > 0)
        {
            context.Fixtures.RemoveRange(existingDemoFixtures);
            await context.SaveChangesAsync(cancellationToken);
        }

        var hasFixtures = await context.Fixtures.AnyAsync(cancellationToken);
        var nextWeek = (hasFixtures
            ? await context.Fixtures.MaxAsync(x => x.MatchWeek, cancellationToken)
            : 0) + 1;

        var fixturePlans = BuildFixturePlans(teams, nextWeek);
        var createdFixtures = new List<Fixture>();

        foreach (var plan in fixturePlans.Select((value, index) => new { value, index }))
        {
            var fixture = new Fixture
            {
                ApiFootballFixtureId = -1000 - plan.index,
                MatchWeek = nextWeek,
                HomeTeamId = plan.value.HomeTeam.Id,
                AwayTeamId = plan.value.AwayTeam.Id,
                KickoffUtc = plan.value.KickoffUtc,
                VenueName = plan.value.VenueName,
                ImageUrl = plan.value.HomeTeam.LogoUrl,
                Status = plan.value.Status,
                HomeHalfTimeScore = plan.value.HomeHalfTimeScore,
                AwayHalfTimeScore = plan.value.AwayHalfTimeScore,
                HomeFullTimeScore = plan.value.HomeFullTimeScore,
                AwayFullTimeScore = plan.value.AwayFullTimeScore
            };

            context.Fixtures.Add(fixture);
            createdFixtures.Add(fixture);
        }

        await context.SaveChangesAsync(cancellationToken);

        foreach (var plan in fixturePlans.Zip(createdFixtures, (plan, fixture) => new { plan, fixture }))
        {
            AddStatistics(plan.fixture.Id, plan.plan);
            AddEvents(plan.fixture.Id, plan.plan);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new
        {
            createdFixtures = createdFixtures.Count,
            createdLiveFixtures = createdFixtures.Count(x => x.Status == FixtureStatus.Live),
            createdUpcomingFixtures = createdFixtures.Count(x => x.Status == FixtureStatus.NotStarted),
            matchWeek = nextWeek
        };
    }

    private void AddStatistics(int fixtureId, FixturePlan plan)
    {
        string[] homeValues = plan.Status == FixtureStatus.NotStarted
            ? ["52%", "15", "6", "5", "2", "10", "1", "86%", "1.74"]
            : ["54%", "13", "5", "6", "1", "9", "2", "87%", "1.58"];
        string[] awayValues = plan.Status == FixtureStatus.NotStarted
            ? ["48%", "12", "4", "4", "1", "12", "2", "83%", "1.21"]
            : ["46%", "10", "4", "3", "2", "11", "1", "82%", "1.11"];

        for (var i = 0; i < DefaultStatisticNames.Length; i++)
        {
            context.MatchStatistics.Add(new MatchStatistic
            {
                FixtureId = fixtureId,
                TeamId = plan.HomeTeam.Id,
                Name = DefaultStatisticNames[i],
                Value = homeValues[i]
            });

            context.MatchStatistics.Add(new MatchStatistic
            {
                FixtureId = fixtureId,
                TeamId = plan.AwayTeam.Id,
                Name = DefaultStatisticNames[i],
                Value = awayValues[i]
            });
        }
    }

    private void AddEvents(int fixtureId, FixturePlan plan)
    {
        foreach (var item in plan.Events)
        {
            context.MatchEvents.Add(new MatchEvent
            {
                FixtureId = fixtureId,
                TeamId = item.TeamId,
                Minute = item.Minute,
                PlayerName = item.PlayerName,
                EventType = item.EventType,
                Description = item.Description
            });
        }
    }

    private static List<FixturePlan> BuildFixturePlans(IReadOnlyList<Team> teams, int week)
    {
        var kickoffBase = DateTime.UtcNow.Date.AddHours(15);

        return
        [
            CreateLivePlan(teams, week, kickoffBase.AddHours(-2), "Liverpool", "Arsenal", 1, 0, 2, 1,
            [
                new PlannedEvent(ResolveTeam(teams, "Liverpool").Id, 18, "Luis Diaz", MatchEventType.Goal, "Yakın mesafe vuruşu"),
                new PlannedEvent(ResolveTeam(teams, "Arsenal").Id, 42, "Martin Odegaard", MatchEventType.Goal, "Ceza sahası dışı şut"),
                new PlannedEvent(ResolveTeam(teams, "Liverpool").Id, 57, "Alexis Mac Allister", MatchEventType.YellowCard, "Geç müdahale"),
                new PlannedEvent(ResolveTeam(teams, "Liverpool").Id, 71, "Darwin Nunez", MatchEventType.Substitution, "Cikan: Diogo Jota"),
                new PlannedEvent(ResolveTeam(teams, "Liverpool").Id, 76, "Mohamed Salah", MatchEventType.Goal, "Hızlı hücum sonrası bitiriş")
            ]),
            CreateLivePlan(teams, week, kickoffBase.AddHours(-1), "Manchester City", "Chelsea", 0, 0, 1, 1,
            [
                new PlannedEvent(ResolveTeam(teams, "Chelsea").Id, 24, "Cole Palmer", MatchEventType.Goal, "Penaltı"),
                new PlannedEvent(ResolveTeam(teams, "Manchester City").Id, 49, "Phil Foden", MatchEventType.Goal, "Ceza yayı üstü plase"),
                new PlannedEvent(ResolveTeam(teams, "Chelsea").Id, 63, "Enzo Fernandez", MatchEventType.YellowCard, "Taktik faul")
            ]),
            CreateUpcomingPlan(teams, week, kickoffBase.AddDays(1), "Tottenham", "Newcastle"),
            CreateUpcomingPlan(teams, week, kickoffBase.AddDays(1).AddHours(2), "Aston Villa", "Brighton"),
            CreateUpcomingPlan(teams, week, kickoffBase.AddDays(2).AddHours(1), "Everton", "Fulham")
        ];
    }

    private static FixturePlan CreateLivePlan(
        IReadOnlyList<Team> teams,
        int week,
        DateTime kickoffUtc,
        string homeTeamName,
        string awayTeamName,
        int homeHalfTimeScore,
        int awayHalfTimeScore,
        int homeFullTimeScore,
        int awayFullTimeScore,
        IReadOnlyList<PlannedEvent> events)
    {
        var homeTeam = ResolveTeam(teams, homeTeamName);
        var awayTeam = ResolveTeam(teams, awayTeamName);

        return new FixturePlan(
            homeTeam,
            awayTeam,
            week,
            kickoffUtc,
            homeTeam.StadiumName ?? $"{homeTeam.Name} Stadium",
            FixtureStatus.Live,
            homeHalfTimeScore,
            awayHalfTimeScore,
            homeFullTimeScore,
            awayFullTimeScore,
            events);
    }

    private static FixturePlan CreateUpcomingPlan(
        IReadOnlyList<Team> teams,
        int week,
        DateTime kickoffUtc,
        string homeTeamName,
        string awayTeamName)
    {
        var homeTeam = ResolveTeam(teams, homeTeamName);
        var awayTeam = ResolveTeam(teams, awayTeamName);

        return new FixturePlan(
            homeTeam,
            awayTeam,
            week,
            kickoffUtc,
            homeTeam.StadiumName ?? $"{homeTeam.Name} Stadium",
            FixtureStatus.NotStarted,
            null,
            null,
            null,
            null,
            []);
    }

    private static Team ResolveTeam(IReadOnlyList<Team> teams, string name)
    {
        return teams.FirstOrDefault(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"'{name}' takimi veritabaninda bulunamadi.");
    }

    private sealed record FixturePlan(
        Team HomeTeam,
        Team AwayTeam,
        int MatchWeek,
        DateTime KickoffUtc,
        string VenueName,
        FixtureStatus Status,
        int? HomeHalfTimeScore,
        int? AwayHalfTimeScore,
        int? HomeFullTimeScore,
        int? AwayFullTimeScore,
        IReadOnlyList<PlannedEvent> Events);

    private sealed record PlannedEvent(
        int TeamId,
        int Minute,
        string PlayerName,
        MatchEventType EventType,
        string Description);
}
