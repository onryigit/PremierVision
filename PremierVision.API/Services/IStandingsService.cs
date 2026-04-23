using PremierVision.Contracts;

namespace PremierVision.Services;

public interface IStandingsService
{
    Task<IReadOnlyList<StandingRowDto>> CalculateAsync(CancellationToken cancellationToken = default);
}
