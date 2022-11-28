using Classroom.Api.Context.Configurations;
using Classroom.Api.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Task = Classroom.Api.Entities.Task;

namespace Classroom.Api.Context;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {

    }

    public DbSet<Course> Courses { get; set; }
    public DbSet<UserCourse> UserCourses { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<UserTask> UserTasks { get; set; }
    public DbSet<TaskComment> TaskComments { get; set; }
    public DbSet<LocalizedStringEntity> LocalizedStrings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Course>()
            .HasMany(c => c.Users2)
            .WithMany(u => u.Courses2);

        builder.Entity<LocalizedStringEntity>()
            .HasKey(l => l.Key);

        builder.Entity<LocalizedStringEntity>()
            .Property(l => l.Uz).IsRequired().HasDefaultValue("uzbekcha");

        TaskConfiguration.Sozla(builder.Entity<Task>());

        // new UserTaskConfiguration().Configure(builder.Entity<UserTask>());
        // builder.ApplyConfiguration(new UserTaskConfiguration());

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        builder.Entity<LocalizedStringEntity>().HasData(
            new List<LocalizedStringEntity>()
            {
                new LocalizedStringEntity()
                {
                    Key = "Required",
                    Uz = "{0} kiritilishi kerak",
                    Ru = "{0} ruscha",
                    En = "{0} field is required"
                }
            });
    }
}