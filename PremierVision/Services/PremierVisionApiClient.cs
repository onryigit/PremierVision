using System.Net;
using System.Net.Http.Json;
using PremierVision.Models.ViewModels;
using PremierVision.Models.ViewModels.Admin;

namespace PremierVision.Services;

public class PremierVisionApiClient(HttpClient httpClient) : IPremierVisionApiClient
{
    public Task<HomePageViewModel> GetHomePageAsync(int? week, CancellationToken cancellationToken = default) =>
        GetAsync<HomePageViewModel>(week.HasValue ? $"api/home?week={week.Value}" : "api/home", cancellationToken);

    public Task<FixtureListViewModel> GetFixturesAsync(int? week, CancellationToken cancellationToken = default) =>
        GetAsync<FixtureListViewModel>(week.HasValue ? $"api/fixtures?week={week.Value}" : "api/fixtures", cancellationToken);

    public async Task<IReadOnlyList<StandingRowViewModel>> GetStandingsAsync(CancellationToken cancellationToken = default)
    {
        var rows = await GetAsync<List<StandingRowViewModel>>("api/standings", cancellationToken);
        return rows;
    }

    public async Task<MatchDetailViewModel?> GetMatchDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync($"api/matches/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return await DeserializeAsync<MatchDetailViewModel>(response, cancellationToken);
    }

    public Task<AdminPanelViewModel> GetAdminOptionsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<AdminPanelViewModel>("api/admin/options", cancellationToken);

    public Task AddFixtureAsync(CreateFixtureInputModel request, CancellationToken cancellationToken = default)
    {
        var kickoffUtc = request.KickoffUtc.Kind switch
        {
            DateTimeKind.Utc => request.KickoffUtc,
            DateTimeKind.Local => request.KickoffUtc.ToUniversalTime(),
            _ => DateTime.SpecifyKind(request.KickoffUtc, DateTimeKind.Local).ToUniversalTime()
        };

        var payload = new CreateFixtureInputModel
        {
            MatchWeek = request.MatchWeek,
            HomeTeamId = request.HomeTeamId,
            AwayTeamId = request.AwayTeamId,
            KickoffUtc = kickoffUtc,
            VenueName = request.VenueName,
            ImageUrl = request.ImageUrl,
            Status = request.Status,
            HomeHalfTimeScore = request.HomeHalfTimeScore,
            AwayHalfTimeScore = request.AwayHalfTimeScore,
            HomeFullTimeScore = request.HomeFullTimeScore,
            AwayFullTimeScore = request.AwayFullTimeScore
        };

        return PostAsync("api/admin/fixtures", payload, cancellationToken);
    }

    public Task AddEventAsync(CreateMatchEventInputModel request, CancellationToken cancellationToken = default) =>
        PostAsync("api/admin/events", request, cancellationToken);

    public Task AddStatisticAsync(CreateMatchStatisticInputModel request, CancellationToken cancellationToken = default) =>
        PostAsync("api/admin/statistics", request, cancellationToken);

    private async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await DeserializeAsync<T>(response, cancellationToken);
    }

    private async Task PostAsync<T>(string path, T payload, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(path, payload, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task<T> DeserializeAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var model = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        return model ?? throw new InvalidOperationException("API bos yanit dondu.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = string.IsNullOrWhiteSpace(body)
            ? $"API istegi basarisiz oldu: {(int)response.StatusCode} {response.ReasonPhrase}"
            : $"API istegi basarisiz oldu: {(int)response.StatusCode} {body}";

        throw new InvalidOperationException(message);
    }
}
