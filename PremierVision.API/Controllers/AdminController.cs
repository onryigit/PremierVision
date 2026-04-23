using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PremierVision.Contracts;
using PremierVision.Data;
using PremierVision.Models;
using PremierVision.Services;

namespace PremierVision.API.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(
    AppDbContext context,
    IApiFootballImportService apiFootballImportService) : ControllerBase
{
    [HttpGet("options")]
    public async Task<ActionResult<AdminPanelDto>> GetOptions(CancellationToken cancellationToken)
    {
        var teams = await context.Teams
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SimpleOptionDto
            {
                Value = x.Id,
                Label = x.Name
            })
            .ToListAsync(cancellationToken);

        var fixtures = await context.Fixtures
            .AsNoTracking()
            .Include(x => x.HomeTeam)
            .Include(x => x.AwayTeam)
            .OrderByDescending(x => x.KickoffUtc)
            .Select(x => new SimpleOptionDto
            {
                Value = x.Id,
                Label = $"Hafta {x.MatchWeek} - {x.HomeTeam.Name} - {x.AwayTeam.Name}"
            })
            .ToListAsync(cancellationToken);

        return Ok(new AdminPanelDto
        {
            Teams = teams,
            Fixtures = fixtures
        });
    }

    [HttpPost("import/teams")]
    public async Task<ActionResult> ImportTeams(CancellationToken cancellationToken)
    {
        var count = await apiFootballImportService.ImportTeamsAsync(cancellationToken);
        return Ok(new { count });
    }

    [HttpPost("import/fixtures")]
    public async Task<ActionResult> ImportFixtures(CancellationToken cancellationToken)
    {
        var count = await apiFootballImportService.ImportFixturesAsync(cancellationToken);
        return Ok(new { count });
    }

    [HttpPost("import/live")]
    public async Task<ActionResult> ImportLive(CancellationToken cancellationToken)
    {
        var count = await apiFootballImportService.ImportLiveFixturesAsync(cancellationToken);
        return Ok(new { count });
    }

    [HttpPost("fixtures")]
    public async Task<ActionResult> AddFixture([FromBody] CreateFixtureRequest request, CancellationToken cancellationToken)
    {
        if (request.HomeTeamId == request.AwayTeamId)
        {
            ModelState.AddModelError(nameof(request.AwayTeamId), "Ayni takim iki kez secilemez.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        context.Fixtures.Add(new Fixture
        {
            MatchWeek = request.MatchWeek,
            HomeTeamId = request.HomeTeamId,
            AwayTeamId = request.AwayTeamId,
            KickoffUtc = DateTime.SpecifyKind(request.KickoffUtc, DateTimeKind.Utc),
            VenueName = request.VenueName,
            ImageUrl = request.ImageUrl,
            Status = request.Status,
            HomeHalfTimeScore = request.HomeHalfTimeScore,
            AwayHalfTimeScore = request.AwayHalfTimeScore,
            HomeFullTimeScore = request.HomeFullTimeScore,
            AwayFullTimeScore = request.AwayFullTimeScore
        });

        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetOptions), new { }, null);
    }

    [HttpPost("events")]
    public async Task<ActionResult> AddEvent([FromBody] CreateMatchEventRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        context.MatchEvents.Add(new MatchEvent
        {
            FixtureId = request.FixtureId,
            TeamId = request.TeamId,
            Minute = request.Minute,
            PlayerName = request.PlayerName,
            EventType = request.EventType,
            Description = request.Description
        });

        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetOptions), new { }, null);
    }

    [HttpPost("statistics")]
    public async Task<ActionResult> AddStatistic([FromBody] CreateMatchStatisticRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        context.MatchStatistics.Add(new MatchStatistic
        {
            FixtureId = request.FixtureId,
            TeamId = request.TeamId,
            Name = request.Name,
            Value = request.Value
        });

        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetOptions), new { }, null);
    }
}
