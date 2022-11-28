namespace Classroom.Api.Models;

public class UserDto
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
}