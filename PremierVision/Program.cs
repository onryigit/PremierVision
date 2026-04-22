using Microsoft.EntityFrameworkCore;
using PremierVision.Data;
using PremierVision.Options;
using PremierVision.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IStandingsService, StandingsService>();
builder.Services.Configure<ApiFootballOptions>(
    builder.Configuration.GetSection(ApiFootballOptions.SectionName));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedData.EnsureSeededAsync(dbContext);
    }
    catch (Exception exception)
    {
        Console.WriteLine($"Database initialization skipped: {exception.Message}");
    }
}


app.Run();
