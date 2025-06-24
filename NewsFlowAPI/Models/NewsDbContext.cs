using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace NewsFlowAPI.Models
{
    public class NewsDbContext : IdentityDbContext<User>
    {
        public NewsDbContext(DbContextOptions<NewsDbContext> options) : base(options) { }


        public virtual DbSet<NewsItem> News { get; set; }

        public virtual DbSet<NewsLike> NewsLikes { get; set; }

        public DbSet<NewsShare> NewsShares { get; set; }

        public DbSet<UserInteraction> UserInteractions { get; set; }

        public DbSet<Subscriptions> Subscriptions { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            
            modelBuilder.Entity<NewsItem>(entity =>
            {
                entity.HasKey(e => e.NewsId).HasName("PK__News__954EBDF30EAEB42D");

                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Content).HasColumnType("nvarchar(max)");
                entity.Property(e => e.ImageUrl).HasMaxLength(255);
                entity.Property(e => e.PublishedAt).HasColumnType("datetime");
                entity.Property(e => e.Source).HasMaxLength(255);
                entity.Property(e => e.Title).HasMaxLength(500);
                entity.Property(e => e.Url).HasMaxLength(500);

               
                entity.HasMany(n => n.NewsLikes)
                      .WithOne(nl => nl.NewsItem)
                      .HasForeignKey(nl => nl.NewsId)
                      .OnDelete(DeleteBehavior.Cascade);

                
                entity.HasMany(n => n.NewsShares)
                      .WithOne(ns => ns.NewsItem)
                      .HasForeignKey(ns => ns.NewsId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

           
            modelBuilder.Entity<NewsLike>(entity =>
            {
                entity.HasKey(nl => new { nl.NewsId, nl.UserId });
                entity.Property(nl => nl.LikedAt).IsRequired();

                entity.HasOne(nl => nl.NewsItem)
                      .WithMany(n => n.NewsLikes)
                      .HasForeignKey(nl => nl.NewsId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(nl => nl.User)
                      .WithMany()
                      .HasForeignKey(nl => nl.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<NewsShare>(entity =>
            {
                entity.HasKey(ns => ns.NewsId);

                entity.Property(ns => ns.SharedAt)
                      .IsRequired()
                      .HasColumnType("datetime");

                entity.HasOne(ns => ns.NewsItem)
                      .WithMany(n => n.NewsShares)
                      .HasForeignKey(ns => ns.NewsId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ns => ns.User)
                      .WithMany()
                      .HasForeignKey(ns => ns.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserInteraction>(entity =>
            {
                entity.HasKey(ui => ui.InteractionId); 

                entity.Property(ui => ui.InteractionDate)
                      .IsRequired()
                      .HasColumnType("datetime");


                entity.HasOne(ui => ui.NewsItem)
                      .WithMany()
                      .HasForeignKey(ui => ui.NewsId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ui => ui.User)
                      .WithMany()
                      .HasForeignKey(ui => ui.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Subscriptions>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.Source)
                      .IsRequired()
                      .HasMaxLength(255);


                entity.HasIndex(s => new { s.userId, s.Source })
                      .IsUnique();

                entity.HasOne(s => s.User)
                      .WithMany()
                      .HasForeignKey(s => s.userId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

    }
}
