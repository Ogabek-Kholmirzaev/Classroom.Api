using System.ComponentModel.DataAnnotations;

namespace Classroom.Api.Models;

public class UpdateCourseDto
{
    [Required]
    public string? Name { get; set; }
}