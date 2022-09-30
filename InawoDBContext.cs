using Inawo.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace Inawo
{
    public class InawoDBContext: DbContext
    {
        public InawoDBContext(DbContextOptions<InawoDBContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Income> Incomes { get; set; }
        public DbSet<Expense> Expenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
               .HasMany(l => l.Incomes)
               .WithOne(u => u.Account);
            modelBuilder.Entity<User>()
               .HasMany(l => l.Expenses)
               .WithOne(u => u.Account);
        }
    }
}
