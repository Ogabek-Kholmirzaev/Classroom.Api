using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Classroom.Api.Models;

public class CreateCourseDto
{
    [Required(ErrorMessage = "Required")]
    [StringLength(100)]
    public string? Name { get; set; }

    public CreateCourseDto()
    {
        var culture = CultureInfo.CurrentCulture;
    }
}