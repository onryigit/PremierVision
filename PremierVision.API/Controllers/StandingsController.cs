using Microsoft.AspNetCore.Mvc;
using PremierVision.Contracts;
using PremierVision.Services;

namespace PremierVision.API.Controllers;

[ApiController]
[Route("api/standings")]
public class StandingsController(IStandingsService standingsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StandingRowDto>>> Get(CancellationToken cancellationToken)
    {
        return Ok(await standingsService.CalculateAsync(cancellationToken));
    }
}
