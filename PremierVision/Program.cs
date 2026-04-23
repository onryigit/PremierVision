using PremierVision.Options;
using PremierVision.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.Configure<PremierVisionApiOptions>(
    builder.Configuration.GetSection(PremierVisionApiOptions.SectionName));
builder.Services.AddHttpClient<IPremierVisionApiClient, PremierVisionApiClient>((serviceProvider, client) =>
{
    var apiOptions = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<PremierVisionApiOptions>>()
        .Value;
    client.BaseAddress = new Uri(apiOptions.BaseUrl.TrimEnd('/') + "/");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "match-detail",
    pattern: "match-detail.html/{id?}",
    defaults: new { controller = "Matches", action = "Detail" });

app.MapControllerRoute(
    name: "fixtures",
    pattern: "fixtures.html",
    defaults: new { controller = "Fixtures", action = "Index" });

app.MapControllerRoute(
    name: "standings",
    pattern: "standings.html",
    defaults: new { controller = "Standings", action = "Index" });

app.MapControllerRoute(
    name: "index-html",
    pattern: "index.html",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
