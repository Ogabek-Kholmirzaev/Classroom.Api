using System.ComponentModel.DataAnnotations.Schema;

namespace Classroom.Api.Entities;

public class UserCourse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    
    public virtual User? User { get; set; }

    public Guid CourseId { get; set; }
    [ForeignKey(nameof(CourseId))]
    public virtual Course? Course { get; set; }

    public bool IsAdmin { get; set; }
}