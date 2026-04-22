using Microsoft.EntityFrameworkCore;
using PremierVision.Data;
using PremierVision.Models;
using PremierVision.Models.ViewModels;

namespace PremierVision.Services;

public class StandingsService(AppDbContext context) : IStandingsService
{
    public async Task<IReadOnlyList<StandingRowViewModel>> CalculateAsync(CancellationToken cancellationToken = default)
    {
        var teams = await context.Teams
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var completedFixtures = await context.Fixtures
            .AsNoTracking()
            .Where(x => x.Status == FixtureStatus.Completed)
            .OrderBy(x => x.KickoffUtc)
            .ToListAsync(cancellationToken);

        var rows = teams.Select(team =>
        {
            var recentForm = new Queue<string>();
            var playedMatches = completedFixtures
                .Where(x => x.HomeTeamId == team.Id || x.AwayTeamId == team.Id)
                .ToList();

            var wins = 0;
            var draws = 0;
            var losses = 0;
            var goalsFor = 0;
            var goalsAgainst = 0;

            foreach (var match in playedMatches)
            {
                var isHome = match.HomeTeamId == team.Id;
                var goalsScored = isHome ? match.HomeFullTimeScore ?? 0 : match.AwayFullTimeScore ?? 0;
                var goalsConceded = isHome ? match.AwayFullTimeScore ?? 0 : match.HomeFullTimeScore ?? 0;

                goalsFor += goalsScored;
                goalsAgainst += goalsConceded;

                if (goalsScored > goalsConceded)
                {
                    wins++;
                    TrackForm(recentForm, "W");
                }
                else if (goalsScored == goalsConceded)
                {
                    draws++;
                    TrackForm(recentForm, "D");
                }
                else
                {
                    losses++;
                    TrackForm(recentForm, "L");
                }
            }

            return new StandingRowViewModel
            {
                TeamId = team.Id,
                TeamName = team.Name,
                TeamLogoUrl = team.LogoUrl,
                Played = playedMatches.Count,
                Wins = wins,
                Draws = draws,
                Losses = losses,
                GoalsFor = goalsFor,
                GoalsAgainst = goalsAgainst,
                GoalDifference = goalsFor - goalsAgainst,
                Points = wins * 3 + draws,
                Form = string.Join(" - ", recentForm)
            };
        })
        .OrderByDescending(x => x.Points)
        .ThenByDescending(x => x.GoalDifference)
        .ThenByDescending(x => x.GoalsFor)
        .ThenBy(x => x.TeamName)
        .ToList();

        return rows
            .Select((row, index) => new StandingRowViewModel
            {
                Position = index + 1,
                TeamId = row.TeamId,
                TeamName = row.TeamName,
                TeamLogoUrl = row.TeamLogoUrl,
                Played = row.Played,
                Wins = row.Wins,
                Draws = row.Draws,
                Losses = row.Losses,
                GoalsFor = row.GoalsFor,
                GoalsAgainst = row.GoalsAgainst,
                GoalDifference = row.GoalDifference,
                Points = row.Points,
                Form = row.Form
            })
            .ToList();
    }

    private static void TrackForm(Queue<string> form, string result)
    {
        form.Enqueue(result);
        while (form.Count > 5)
        {
            form.Dequeue();
        }
    }
}
