using Microsoft.AspNetCore.Identity;

namespace Classroom.Api.Entities;

public class User : IdentityUser<Guid>
{
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }

    public virtual List<UserCourse>? Courses { get; set; }
    public virtual List<Course>? Courses2 { get; set; }
    public virtual List<UserTask>? UserTasks { get; set; }
}