using Microsoft.EntityFrameworkCore;
using GradeBook.Storage.Entities;

namespace GradeBook.Storage
{
    public interface IGradeBookContext : IDisposable, IAsyncDisposable
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Group> Groups { get; set; }
        
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<TeacherSubjectGroup> TeacherSubjectGroupRelations { get; set; }
    
        public DbSet<User> Users { get; set; }
    }
    public class GradeBookContext : DbContext, IGradeBookContext
    {
        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;

        public DbSet<Grade> Grades { get; set; } = null!;
        public DbSet<Subject> Subjects { get; set; } = null!;
        public DbSet<Teacher> Teachers { get; set; } = null!;
        public DbSet<TeacherSubjectGroup> TeacherSubjectGroupRelations { get; set; } = null!;

        public DbSet<User> Users { get; set; } = null!;

        public GradeBookContext()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Database=GradeBook;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            optionsBuilder.UseLazyLoadingProxies();


            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            
            base.OnModelCreating(modelBuilder);
        }

    }
}
