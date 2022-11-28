using Classroom.Api.Entities;

namespace Classroom.Api.Models;

public class CreateUserTaskResultDto
{
    public string? Description { get; set; }
    public EUserTaskStatus Status { get; set; }
}