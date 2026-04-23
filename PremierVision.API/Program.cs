using Microsoft.EntityFrameworkCore;
using PremierVision.Data;
using PremierVision.Options;
using PremierVision.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IStandingsService, StandingsService>();
builder.Services.AddHttpClient<IApiFootballImportService, ApiFootballImportService>((serviceProvider, client) =>
{
    var apiOptions = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<ApiFootballOptions>>()
        .Value;
    client.BaseAddress = new Uri(apiOptions.BaseUrl.TrimEnd('/') + "/");
});
builder.Services.Configure<ApiFootballOptions>(
    builder.Configuration.GetSection(ApiFootballOptions.SectionName));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
