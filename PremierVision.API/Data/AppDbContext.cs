using Microsoft.EntityFrameworkCore;
using PremierVision.Models;

namespace PremierVision.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Fixture> Fixtures => Set<Fixture>();
    public DbSet<MatchEvent> MatchEvents => Set<MatchEvent>();
    public DbSet<MatchStatistic> MatchStatistics => Set<MatchStatistic>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Team>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(120);
            entity.Property(x => x.ShortName).HasMaxLength(40);
            entity.Property(x => x.Code).HasMaxLength(12);
            entity.Property(x => x.LogoUrl).HasMaxLength(500);
            entity.Property(x => x.StadiumName).HasMaxLength(150);
            entity.Property(x => x.StadiumCity).HasMaxLength(120);
        });

        modelBuilder.Entity<Fixture>(entity =>
        {
            entity.Property(x => x.VenueName).HasMaxLength(150);
            entity.Property(x => x.ImageUrl).HasMaxLength(500);
            entity.HasOne(x => x.HomeTeam)
                .WithMany(x => x.HomeFixtures)
                .HasForeignKey(x => x.HomeTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AwayTeam)
                .WithMany(x => x.AwayFixtures)
                .HasForeignKey(x => x.AwayTeamId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MatchEvent>(entity =>
        {
            entity.Property(x => x.PlayerName).HasMaxLength(120);
            entity.Property(x => x.Description).HasMaxLength(250);

            entity.HasOne(x => x.Fixture)
                .WithMany(x => x.Events)
                .HasForeignKey(x => x.FixtureId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Team)
                .WithMany()
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MatchStatistic>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(80);
            entity.Property(x => x.Value).HasMaxLength(50);

            entity.HasOne(x => x.Fixture)
                .WithMany(x => x.Statistics)
                .HasForeignKey(x => x.FixtureId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Team)
                .WithMany()
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
