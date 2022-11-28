using Classroom.Api.Entities;
using Classroom.Api.Models;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Api.Controllers;

public partial class CoursesController
{
    [HttpGet("{courseId}/tasks/{taskId}/comments")]
    [ProducesResponseType(typeof(List<TaskCommentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaskComments(Guid courseId, Guid taskId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.CourseId == courseId);

        if (task is null)
            return NotFound();

        var comments = new List<TaskCommentDto>();

        if (task.Comments is null)
            return Ok(comments);

        var mainComments = task.Comments.Where(tc => tc.ParentId == null).ToList();

        foreach (var comment in mainComments)
        {
            comments.Add(ToTaskCommentDto(comment));
        }

        return Ok(comments);
    }

    private TaskCommentDto ToTaskCommentDto(TaskComment comment)
    {
        var commentDto = new TaskCommentDto()
        {
            Id = comment.Id,
            Comment = comment.Comment,
            CreatedDate = comment.CreatedDate,
            User = comment.User?.Adapt<UserDto>(),
        };

        if (comment.Children is null)
            return commentDto;

        foreach (var child in comment.Children)
        {
            commentDto.Children ??= new List<TaskCommentDto>();
            commentDto.Children.Add(ToTaskCommentDto(child));
        }

        return commentDto;
    }

    [HttpPost("{courseId}/tasks/{taskId}/comments")]
    public async Task<IActionResult> AddTaskComments(Guid courseId, Guid taskId, [FromBody] CreateTaskCommentDto taskCommentDto)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.CourseId == courseId);

        if (task is null)
            return NotFound();

        var user = await _userManager.GetUserAsync(User);

        task.Comments ??= new List<TaskComment>();

        task.Comments.Add(new TaskComment()
        {
            TaskId = taskId,
            UserId = user.Id,
            Comment = taskCommentDto.Comment,
            ParentId = taskCommentDto.ParentId
        });

        await _context.SaveChangesAsync();

        return Ok();
    }
}