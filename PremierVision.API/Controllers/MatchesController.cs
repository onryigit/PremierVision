using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PremierVision.Contracts;
using PremierVision.Data;

namespace PremierVision.API.Controllers;

[ApiController]
[Route("api/matches")]
public class MatchesController(AppDbContext context) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MatchDetailDto>> Get(int id, CancellationToken cancellationToken)
    {
        var fixture = await context.Fixtures
            .AsNoTracking()
            .Include(x => x.HomeTeam)
            .Include(x => x.AwayTeam)
            .Include(x => x.Events)
                .ThenInclude(x => x.Team)
            .Include(x => x.Statistics)
                .ThenInclude(x => x.Team)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (fixture is null)
        {
            return NotFound();
        }

        return Ok(new MatchDetailDto
        {
            Fixture = fixture.ToFixtureCardDto(),
            Events = fixture.Events
                .OrderBy(x => x.Minute)
                .Select(x => new MatchEventItemDto
                {
                    Minute = x.Minute,
                    TeamName = x.Team?.Name ?? "-",
                    PlayerName = x.PlayerName,
                    EventType = x.EventType,
                    Description = x.Description
                })
                .ToList(),
            Statistics = fixture.Statistics
                .Select(x => new MatchStatisticItemDto
                {
                    TeamName = x.Team.Name,
                    Name = x.Name,
                    Value = x.Value
                })
                .ToList()
        });
    }
}
