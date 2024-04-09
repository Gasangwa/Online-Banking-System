using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using ThesisProject.Areas.Identity.Data;
using ThesisProject.Models;

namespace ThesisProject.Data;

public class ThesisProjectContext : IdentityDbContext<ThesisProjectUser>
{
    public ThesisProjectContext(DbContextOptions<ThesisProjectContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<ThesisProjectUser>()
               .HasMany(user => user.Account)
               .WithOne(account => account.ThesisProjectUser)
               .HasForeignKey(account => account.userId);
        builder.Entity<ThesisProjectUser>()
               .HasMany(user => user.Card)
               .WithOne(card => card.ThesisProjectUser)
               .HasForeignKey(card => card.userId);
        builder.Entity<ThesisProjectUser>()
               .HasMany(user => user.Transaction)
               .WithOne(transaction => transaction.ThesisProjectUser)
               .HasForeignKey(transaction => transaction.userId);
		base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }

public DbSet<ThesisProject.Models.Account> Account { get; set; } = default!;
    public DbSet<ThesisProject.Models.Card> Card { get; set; }
    public DbSet<ThesisProject.Models.Transaction> Transaction { get; set; }
}
