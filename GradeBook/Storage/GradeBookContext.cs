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

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        public int SaveChanges();
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

        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Database=GradeBook;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            optionsBuilder.UseLazyLoadingProxies();


            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.Login).IsUnique(true);
            modelBuilder.Entity<TeacherSubjectGroup>().HasIndex(r => new { r.TeacherId, r.SubjectId, r.GroupId }).IsUnique(true);

            base.OnModelCreating(modelBuilder);
        }

    }
}
