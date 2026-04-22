using Microsoft.EntityFrameworkCore;
using PremierVision.Models;

namespace PremierVision.Data;

public static class SeedData
{
    public static async Task EnsureSeededAsync(AppDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Teams.AnyAsync())
        {
            return;
        }

        var teams = new List<Team>
        {
            new() { Name = "Arsenal", ShortName = "ARS", Code = "ARS", StadiumName = "Emirates Stadium", StadiumCity = "London" },
            new() { Name = "Liverpool", ShortName = "LIV", Code = "LIV", StadiumName = "Anfield", StadiumCity = "Liverpool" },
            new() { Name = "Manchester City", ShortName = "MCI", Code = "MCI", StadiumName = "Etihad Stadium", StadiumCity = "Manchester" },
            new() { Name = "Chelsea", ShortName = "CHE", Code = "CHE", StadiumName = "Stamford Bridge", StadiumCity = "London" }
        };

        context.Teams.AddRange(teams);
        await context.SaveChangesAsync();

        var fixtures = new List<Fixture>
        {
            new()
            {
                MatchWeek = 34,
                HomeTeamId = teams[0].Id,
                AwayTeamId = teams[1].Id,
                KickoffUtc = new DateTime(2024, 4, 21, 15, 30, 0, DateTimeKind.Utc),
                VenueName = "Emirates Stadium",
                Status = FixtureStatus.Completed,
                HomeHalfTimeScore = 1,
                AwayHalfTimeScore = 1,
                HomeFullTimeScore = 2,
                AwayFullTimeScore = 2
            },
            new()
            {
                MatchWeek = 34,
                HomeTeamId = teams[2].Id,
                AwayTeamId = teams[3].Id,
                KickoffUtc = new DateTime(2024, 4, 22, 18, 0, 0, DateTimeKind.Utc),
                VenueName = "Etihad Stadium",
                Status = FixtureStatus.NotStarted
            }
        };

        context.Fixtures.AddRange(fixtures);
        await context.SaveChangesAsync();

        context.MatchEvents.AddRange(
            new MatchEvent
            {
                FixtureId = fixtures[0].Id,
                TeamId = teams[0].Id,
                Minute = 18,
                PlayerName = "Player A",
                EventType = MatchEventType.Goal,
                Description = "Acilis golu"
            },
            new MatchEvent
            {
                FixtureId = fixtures[0].Id,
                TeamId = teams[1].Id,
                Minute = 77,
                PlayerName = "Player B",
                EventType = MatchEventType.YellowCard,
                Description = "Sert faul"
            });

        context.MatchStatistics.AddRange(
            new MatchStatistic
            {
                FixtureId = fixtures[0].Id,
                TeamId = teams[0].Id,
                Name = "Topa Sahip Olma",
                Value = "%54"
            },
            new MatchStatistic
            {
                FixtureId = fixtures[0].Id,
                TeamId = teams[1].Id,
                Name = "Topa Sahip Olma",
                Value = "%46"
            });

        await context.SaveChangesAsync();
    }
}
