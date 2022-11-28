using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Classroom.Api.Entities;

[Table("courses")]
public class Course
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 10)]
    public string? Name { get; set; }

    [Required]
    [StringLength(32, MinimumLength = 32)]
    public string? Key { get; set; }

    public virtual List<UserCourse>? Users { get; set; }
    public virtual List<User>? Users2 { get; set; }
    public virtual List<Task>? Tasks { get; set; }
}