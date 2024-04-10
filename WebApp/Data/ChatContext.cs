
namespace Messenjoor.Data
{
    public class ChatContext : DbContext
    {
        public ChatContext(DbContextOptions<ChatContext> options) : base(options)
        {
        }

        public DbSet<Entities.User> Users { get; set; }
        public DbSet<Entities.Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Entities.Message>(e =>
            {
                e.HasOne(m=> m.ToUser).WithMany().OnDelete(DeleteBehavior.NoAction);
                e.HasOne(m=> m.FromUser).WithMany().OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
