namespace PremierVision.Services;

public interface IDemoFixtureScenarioService
{
    Task<object> EnsureScenarioAsync(CancellationToken cancellationToken = default);
}
