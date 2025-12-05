using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using PDFHub.API.Models.Domains;

namespace PDFHub.API.Data;

public class PDFHubDbContext : IdentityDbContext<IdentityUser>
{
    public PDFHubDbContext(DbContextOptions<PDFHubDbContext> options): base(options)
    {

    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.HasIndex(e => e.Token).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}