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
    public DbSet<PdfFiles> PdfFiles {get; set;}
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => e.Token).IsUnique();
        });

        builder.Entity<PdfFiles>(entity =>
        {
            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);
        });
    }
}