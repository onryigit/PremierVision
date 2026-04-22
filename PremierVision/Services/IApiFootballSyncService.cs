namespace PremierVision.Services;

public interface IApiFootballSyncService
{
    Task<int> SyncTeamsAsync(CancellationToken cancellationToken = default);
    Task<int> SyncFixturesAsync(CancellationToken cancellationToken = default);
}
