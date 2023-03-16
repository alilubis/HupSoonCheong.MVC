using Microsoft.EntityFrameworkCore;

namespace HupSoonCheong.MVC.Models.ApplicationDbContext
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions option) : base(option) { }

        public DbSet<Container> Containers { get; set; }
        public DbSet<Photo> Photos { get; set; }
    }
}
