using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PremierVision.Data;
using PremierVision.Models;
using PremierVision.Models.ViewModels;
using PremierVision.Services;
using System.Diagnostics;

namespace PremierVision.Controllers
{
    public class HomeController(AppDbContext context, IStandingsService standingsService) : Controller
    {
        public async Task<IActionResult> Index(int? week, CancellationToken cancellationToken)
        {
            var availableWeeks = await context.Fixtures
                .AsNoTracking()
                .Select(x => x.MatchWeek)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);

            var currentWeek = await context.Fixtures
                .AsNoTracking()
                .Where(x => x.Status == FixtureStatus.Live || x.Status == FixtureStatus.Completed)
                .OrderByDescending(x => x.KickoffUtc)
                .Select(x => x.MatchWeek)
                .FirstOrDefaultAsync(cancellationToken);

            var selectedWeek = week
                ?? (currentWeek != 0 ? currentWeek : availableWeeks.LastOrDefault());

            var weeklyFixtures = await context.Fixtures
                .AsNoTracking()
                .Include(x => x.HomeTeam)
                .Include(x => x.AwayTeam)
                .Where(x => x.MatchWeek == selectedWeek)
                .OrderBy(x => x.KickoffUtc)
                .Select(x => new FixtureCardViewModel
                {
                    Id = x.Id,
                    MatchWeek = x.MatchWeek,
                    HomeTeamName = x.HomeTeam.Name,
                    HomeTeamLogoUrl = x.HomeTeam.LogoUrl,
                    AwayTeamName = x.AwayTeam.Name,
                    AwayTeamLogoUrl = x.AwayTeam.LogoUrl,
                    KickoffUtc = x.KickoffUtc,
                    VenueName = x.VenueName,
                    Status = x.Status,
                    HomeHalfTimeScore = x.HomeHalfTimeScore,
                    AwayHalfTimeScore = x.AwayHalfTimeScore,
                    HomeFullTimeScore = x.HomeFullTimeScore,
                    AwayFullTimeScore = x.AwayFullTimeScore
                })
                .ToListAsync(cancellationToken);

            var latestResults = await context.Fixtures
                .AsNoTracking()
                .Include(x => x.HomeTeam)
                .Include(x => x.AwayTeam)
                .Where(x => x.Status == FixtureStatus.Completed)
                .OrderByDescending(x => x.KickoffUtc)
                .Take(5)
                .Select(x => new FixtureCardViewModel
                {
                    Id = x.Id,
                    MatchWeek = x.MatchWeek,
                    HomeTeamName = x.HomeTeam.Name,
                    HomeTeamLogoUrl = x.HomeTeam.LogoUrl,
                    AwayTeamName = x.AwayTeam.Name,
                    AwayTeamLogoUrl = x.AwayTeam.LogoUrl,
                    KickoffUtc = x.KickoffUtc,
                    VenueName = x.VenueName,
                    Status = x.Status,
                    HomeHalfTimeScore = x.HomeHalfTimeScore,
                    AwayHalfTimeScore = x.AwayHalfTimeScore,
                    HomeFullTimeScore = x.HomeFullTimeScore,
                    AwayFullTimeScore = x.AwayFullTimeScore
                })
                .ToListAsync(cancellationToken);

            var upcomingFixtures = await context.Fixtures
                .AsNoTracking()
                .Include(x => x.HomeTeam)
                .Include(x => x.AwayTeam)
                .Where(x => x.Status != FixtureStatus.Completed)
                .OrderBy(x => x.KickoffUtc)
                .Take(5)
                .Select(x => new FixtureCardViewModel
                {
                    Id = x.Id,
                    MatchWeek = x.MatchWeek,
                    HomeTeamName = x.HomeTeam.Name,
                    HomeTeamLogoUrl = x.HomeTeam.LogoUrl,
                    AwayTeamName = x.AwayTeam.Name,
                    AwayTeamLogoUrl = x.AwayTeam.LogoUrl,
                    KickoffUtc = x.KickoffUtc,
                    VenueName = x.VenueName,
                    Status = x.Status,
                    HomeHalfTimeScore = x.HomeHalfTimeScore,
                    AwayHalfTimeScore = x.AwayHalfTimeScore,
                    HomeFullTimeScore = x.HomeFullTimeScore,
                    AwayFullTimeScore = x.AwayFullTimeScore
                })
                .ToListAsync(cancellationToken);

            var standings = await standingsService.CalculateAsync(cancellationToken);

            return View(new HomePageViewModel
            {
                CurrentWeek = selectedWeek,
                AvailableWeeks = availableWeeks,
                FeaturedMatch = weeklyFixtures
                    .Where(x => x.Status == FixtureStatus.Completed)
                    .OrderByDescending(x => (x.HomeFullTimeScore ?? 0) + (x.AwayFullTimeScore ?? 0))
                    .ThenByDescending(x => x.KickoffUtc)
                    .FirstOrDefault() ?? weeklyFixtures.FirstOrDefault(),
                WeeklyFixtures = weeklyFixtures,
                LatestResults = latestResults,
                UpcomingFixtures = upcomingFixtures,
                StandingsPreview = standings.Take(5).ToList()
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
