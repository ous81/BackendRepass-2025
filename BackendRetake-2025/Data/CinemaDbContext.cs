using Microsoft.EntityFrameworkCore;
using BackendRetake_2025.Models;

namespace BackendRetake_2025.Data
{
    public class CinemaDbContext : DbContext
    {
        public CinemaDbContext(DbContextOptions<CinemaDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Poster> Posters { get; set; }

        public DbSet<Review> Reviews { get; set; }
        public DbSet<Series> Series { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).HasConversion<string>();
            });

            modelBuilder.Entity<Movie>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Director).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Genre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.BoxOffice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.AverageRating).HasDefaultValue(0.0);
            });

            modelBuilder.Entity<Poster>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Url).IsRequired();
                entity.Property(e => e.MimeType).IsRequired().HasMaxLength(100);

                entity.HasOne(e => e.Movie)
                      .WithMany(m => m.Posters)
                      .HasForeignKey(e => e.MovieId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Series)
                      .WithMany(s => s.Posters)
                      .HasForeignKey(e => e.SeriesId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.ToTable(t => t.HasCheckConstraint("CK_Poster_MovieOrSeries",
                    "(MovieId IS NOT NULL AND SeriesId IS NULL) OR (MovieId IS NULL AND SeriesId IS NOT NULL)"));
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.Rating).IsRequired();

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Movie)
                      .WithMany(m => m.Reviews)
                      .HasForeignKey(e => e.MovieId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Series)
                      .WithMany(s => s.Reviews)
                      .HasForeignKey(e => e.SeriesId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.ToTable(t => t.HasCheckConstraint("CK_Review_MovieOrSeries",
                    "(MovieId IS NOT NULL AND SeriesId IS NULL) OR (MovieId IS NULL AND SeriesId IS NOT NULL)"));

                entity.HasIndex(e => new { e.UserId, e.MovieId, e.SeriesId })
                      .IsUnique()
                      .HasFilter("(MovieId IS NOT NULL AND SeriesId IS NULL) OR (MovieId IS NULL AND SeriesId IS NOT NULL)");
            });

            modelBuilder.Entity<Series>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Genre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.AverageRating).HasDefaultValue(0.0);
            });
        }


    }
}
