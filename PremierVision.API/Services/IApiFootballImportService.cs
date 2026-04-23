namespace PremierVision.Services;

public interface IApiFootballImportService
{
    Task<int> ImportTeamsAsync(CancellationToken cancellationToken = default);
    Task<int> ImportFixturesAsync(CancellationToken cancellationToken = default);
    Task<int> ImportLiveFixturesAsync(CancellationToken cancellationToken = default);
}
