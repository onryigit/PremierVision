using PremierVision.Contracts;
using PremierVision.Models;

namespace PremierVision.API.Controllers;

internal static class ApiMappings
{
    public static FixtureCardDto ToFixtureCardDto(this Fixture fixture)
    {
        int? currentMinute = fixture.Status == FixtureStatus.Live
            ? GetCurrentMinute(fixture)
            : null;

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
            CurrentMinute = currentMinute,
            HomeHalfTimeScore = fixture.HomeHalfTimeScore,
            AwayHalfTimeScore = fixture.AwayHalfTimeScore,
            HomeFullTimeScore = fixture.HomeFullTimeScore,
            AwayFullTimeScore = fixture.AwayFullTimeScore
        };
    }

    private static int GetCurrentMinute(Fixture fixture)
    {
        if (fixture.Events.Count > 0)
        {
            return Math.Clamp(fixture.Events.Max(x => x.Minute), 1, 90);
        }

        var elapsedMinutes = (int)Math.Floor((DateTime.UtcNow - fixture.KickoffUtc).TotalMinutes);
        return Math.Clamp(elapsedMinutes > 0 ? elapsedMinutes : 1, 1, 90);
    }
}
