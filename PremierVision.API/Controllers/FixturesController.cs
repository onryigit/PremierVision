using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PremierVision.Contracts;
using PremierVision.Data;
using PremierVision.Models;

namespace PremierVision.API.Controllers;

[ApiController]
[Route("api/fixtures")]
public class FixturesController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<FixtureListDto>> Get([FromQuery] int? week, CancellationToken cancellationToken)
    {
        var availableWeeks = await context.Fixtures
            .AsNoTracking()
            .Select(x => x.MatchWeek)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var upcomingWeek = await context.Fixtures
            .AsNoTracking()
            .Where(x => x.Status != FixtureStatus.Completed)
            .OrderBy(x => x.KickoffUtc)
            .Select(x => x.MatchWeek)
            .FirstOrDefaultAsync(cancellationToken);

        var selectedWeek = week
            ?? (upcomingWeek != 0 ? upcomingWeek : availableWeeks.LastOrDefault());

        var fixtures = (await context.Fixtures
            .AsNoTracking()
            .Include(x => x.HomeTeam)
            .Include(x => x.AwayTeam)
            .Where(x => x.MatchWeek == selectedWeek)
            .OrderBy(x => x.KickoffUtc)
            .ToListAsync(cancellationToken))
            .Select(x => x.ToFixtureCardDto())
            .ToList();

        return Ok(new FixtureListDto
        {
            CurrentWeek = selectedWeek,
            AvailableWeeks = availableWeeks,
            Fixtures = fixtures
        });
    }
}
