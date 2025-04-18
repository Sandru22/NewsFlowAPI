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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
       => optionsBuilder.UseSqlServer("Data Source=(localdb)\\NewsFlowDB;Initial Catalog=NewsFlowDB;Integrated Security=True;Encrypt=True");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Apelează configurarea din IdentityDbContext

            // Configurarea tabelei News
            modelBuilder.Entity<NewsItem>(entity =>
            {
                entity.HasKey(e => e.NewsId).HasName("PK__News__954EBDF30EAEB42D");

                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Content).HasColumnType("text");
                entity.Property(e => e.ImageUrl).HasMaxLength(255);
                entity.Property(e => e.PublishedAt).HasColumnType("datetime");
                entity.Property(e => e.Source).HasMaxLength(255);
                entity.Property(e => e.Title).HasMaxLength(500);
                entity.Property(e => e.Url).HasMaxLength(500);

                // Relația one-to-many între NewsItem și NewsLike
                entity.HasMany(n => n.NewsLikes)
                      .WithOne(nl => nl.NewsItem)
                      .HasForeignKey(nl => nl.NewsId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relația one-to-many între NewsItem și NewsShares
                entity.HasMany(n => n.NewsShares)
                      .WithOne(ns => ns.NewsItem)
                      .HasForeignKey(ns => ns.NewsId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configurarea tabelei NewsLike
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

            // Configurarea tabelei NewsShares
            modelBuilder.Entity<NewsShare>(entity =>
            {
                entity.HasKey(ns => ns.NewsId);

                entity.Property(ns => ns.SharedAt)
                      .IsRequired()
                      .HasColumnType("datetime");

                // Relația many-to-one între NewsShare și NewsItem
                entity.HasOne(ns => ns.NewsItem)
                      .WithMany(n => n.NewsShares)
                      .HasForeignKey(ns => ns.NewsId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relația many-to-one între NewsShare și User
                entity.HasOne(ns => ns.User)
                      .WithMany()
                      .HasForeignKey(ns => ns.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserInteraction>(entity =>
            {
                entity.HasKey(ui => ui.InteractionId); // Cheia primară

                entity.Property(ui => ui.InteractionDate)
                      .IsRequired()
                      .HasColumnType("datetime");

                // Relația many-to-one cu NewsItem
                entity.HasOne(ui => ui.NewsItem)
                      .WithMany()
                      .HasForeignKey(ui => ui.NewsId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relația many-to-one cu User
                entity.HasOne(ui => ui.User)
                      .WithMany()
                      .HasForeignKey(ui => ui.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

    }
}
