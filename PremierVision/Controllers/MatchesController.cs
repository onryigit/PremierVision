using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PremierVision.Data;
using PremierVision.Models.ViewModels;

namespace PremierVision.Controllers;

public class MatchesController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Detail(int? id, CancellationToken cancellationToken)
    {
        var fixture = await context.Fixtures
            .AsNoTracking()
            .Include(x => x.HomeTeam)
            .Include(x => x.AwayTeam)
            .Include(x => x.Events)
                .ThenInclude(x => x.Team)
            .Include(x => x.Statistics)
                .ThenInclude(x => x.Team)
            .OrderByDescending(x => x.KickoffUtc)
            .FirstOrDefaultAsync(x => !id.HasValue || x.Id == id.Value, cancellationToken);

        if (fixture is null)
        {
            return NotFound();
        }

        return View(new MatchDetailViewModel
        {
            Fixture = new FixtureCardViewModel
            {
                Id = fixture.Id,
                MatchWeek = fixture.MatchWeek,
                HomeTeamName = fixture.HomeTeam.Name,
                AwayTeamName = fixture.AwayTeam.Name,
                KickoffUtc = fixture.KickoffUtc,
                VenueName = fixture.VenueName,
                Status = fixture.Status,
                HomeHalfTimeScore = fixture.HomeHalfTimeScore,
                AwayHalfTimeScore = fixture.AwayHalfTimeScore,
                HomeFullTimeScore = fixture.HomeFullTimeScore,
                AwayFullTimeScore = fixture.AwayFullTimeScore
            },
            Events = fixture.Events
                .OrderBy(x => x.Minute)
                .Select(x => new MatchEventItemViewModel
                {
                    Minute = x.Minute,
                    TeamName = x.Team?.Name ?? "-",
                    PlayerName = x.PlayerName,
                    EventType = x.EventType,
                    Description = x.Description
                })
                .ToList(),
            Statistics = fixture.Statistics
                .Select(x => new MatchStatisticItemViewModel
                {
                    TeamName = x.Team.Name,
                    Name = x.Name,
                    Value = x.Value
                })
                .ToList()
        });
    }
}
