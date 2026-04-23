using PremierVision.Models.ViewModels;
using PremierVision.Models.ViewModels.Admin;

namespace PremierVision.Services;

public interface IPremierVisionApiClient
{
    Task<HomePageViewModel> GetHomePageAsync(int? week, CancellationToken cancellationToken = default);
    Task<FixtureListViewModel> GetFixturesAsync(int? week, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StandingRowViewModel>> GetStandingsAsync(CancellationToken cancellationToken = default);
    Task<MatchDetailViewModel?> GetMatchDetailAsync(int id, CancellationToken cancellationToken = default);
    Task<AdminPanelViewModel> GetAdminOptionsAsync(CancellationToken cancellationToken = default);
    Task ImportTeamsAsync(CancellationToken cancellationToken = default);
    Task ImportFixturesAsync(CancellationToken cancellationToken = default);
    Task ImportLiveFixturesAsync(CancellationToken cancellationToken = default);
    Task EnsureDemoScenarioAsync(CancellationToken cancellationToken = default);
    Task AddFixtureAsync(CreateFixtureInputModel request, CancellationToken cancellationToken = default);
    Task AddEventAsync(CreateMatchEventInputModel request, CancellationToken cancellationToken = default);
    Task AddStatisticAsync(CreateMatchStatisticInputModel request, CancellationToken cancellationToken = default);
}
