using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

namespace BackendOrar.Data
{
    public class OrarContext : DbContext
    {
        public OrarContext(DbContextOptions<OrarContext> options) : base(options)
        { }

        public DbSet<User> User { get; set; }
        public DbSet<TokenPair> TokenPair { get; set; }
        public DbSet<Classroom> Classroom { get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<Course> Course { get; set; }
        public DbSet<Professor> Professor { get; set; }
        public DbSet<Timetable> Timetable { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /* register all entities */
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<TokenPair>().ToTable("TokenPair");
            modelBuilder.Entity<Classroom>().ToTable("Classroom");

            modelBuilder.Entity<Group>().ToTable("Group");
            modelBuilder.Entity<Course>().ToTable("Course");
            modelBuilder.Entity<Professor>().ToTable("Professor");
            modelBuilder.Entity<Timetable>().ToTable("Timetable");
        }
    }
}
