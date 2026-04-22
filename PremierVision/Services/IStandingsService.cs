using PremierVision.Models.ViewModels;

namespace PremierVision.Services;

public interface IStandingsService
{
    Task<IReadOnlyList<StandingRowViewModel>> CalculateAsync(CancellationToken cancellationToken = default);
}
