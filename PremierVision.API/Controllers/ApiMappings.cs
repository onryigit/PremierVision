using PremierVision.Contracts;
using PremierVision.Models;

namespace PremierVision.API.Controllers;

internal static class ApiMappings
{
    public static FixtureCardDto ToFixtureCardDto(this Fixture fixture)
    {
        return new FixtureCardDto
        {
            Id = fixture.Id,
            MatchWeek = fixture.MatchWeek,
            HomeTeamName = fixture.HomeTeam.Name,
            HomeTeamLogoUrl = fixture.HomeTeam.LogoUrl,
            AwayTeamName = fixture.AwayTeam.Name,
            AwayTeamLogoUrl = fixture.AwayTeam.LogoUrl,
            KickoffUtc = fixture.KickoffUtc,
            VenueName = fixture.VenueName,
            Status = fixture.Status,
            HomeHalfTimeScore = fixture.HomeHalfTimeScore,
            AwayHalfTimeScore = fixture.AwayHalfTimeScore,
            HomeFullTimeScore = fixture.HomeFullTimeScore,
            AwayFullTimeScore = fixture.AwayFullTimeScore
        };
    }
}
