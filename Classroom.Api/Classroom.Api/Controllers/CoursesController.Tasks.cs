using Classroom.Api.Filters;
using Classroom.Api.Mappers;
using Classroom.Api.Models;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Api.Controllers;

public partial class CoursesController
{
    [HttpPost("{courseId}/tasks")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddTask(Guid courseId, [FromBody] CreateTaskDto createTaskDto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

        if (course is null)
            return NotFound();

        var user = await _userManager.GetUserAsync(User);

        if (course.Users?.Any(uc => uc.UserId == user.Id && uc.IsAdmin) != true)
            return BadRequest();

        var task = createTaskDto.Adapt<Classroom.Api.Entities.Task>();

        task.CreatedDate = DateTime.Now;
        task.CourseId = courseId;

        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();

        return Ok(task.Adapt<TaskDto>());
    }

    [HttpGet("{courseId}/tasks")]
    [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks(Guid courseId)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

        if (course is null)
            return NotFound();

        var tasks = course.Tasks?.Select(task => task.Adapt<TaskDto>()).ToList();

        return Ok(tasks ?? new List<TaskDto>());
    }

    [HttpGet("{courseId}/tasks/{taskId}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [IsCourseUserOrAdmin]
    public async Task<IActionResult> GetTaskById(Guid courseId, Guid taskId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.CourseId == courseId);
        return Ok(task?.Adapt<TaskDto>());
    }

    [HttpPut("{courseId}/tasks/{taskId}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [IsCourseUserOrAdmin(true)]
    public async Task<IActionResult> UpdateTask(Guid courseId, Guid taskId, [FromBody] UpdateTaskDto updateTaskDto)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.CourseId == courseId);

        if (task is null)
            return NotFound();

        task.SetValues(updateTaskDto);

        await _context.SaveChangesAsync();

        return Ok(task.Adapt<TaskDto>());
    }

    [HttpDelete("{courseId}/tasks/{taskId}")]
    public async Task<IActionResult> DeleteTask(Guid courseId, Guid taskId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.CourseId == courseId);
        
        if (task is null)
            return NotFound();

        _context.Remove(task);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("{courseId}/tasks/{taskId}/results")]
    [ProducesResponseType(typeof(UserTaskResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaskResults(Guid courseId, Guid taskId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.CourseId == courseId);
        
        if (task is null)
            return NotFound();

        var taskDto = task.Adapt<UsersTaskResultsDto>();

        if (task.UserTasks is null) return Ok(taskDto);

        foreach (var result in task.UserTasks)
        {
            taskDto.UsersResult ??= new List<UsersTaskResult>();
            taskDto.UsersResult.Add(result.Adapt<UsersTaskResult>());
        }

        return Ok(taskDto);
    }

    [HttpPut("{courseId}/tasks/{taskId}/results/{resultId}")]
    [ProducesResponseType(typeof(UsersTaskResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateUserResult(Guid courseId, Guid taskId, Guid resultId, CreateUserTaskResultDto resultDto)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.CourseId == courseId);
        if (task is null)
            return NotFound();

        var result = task.UserTasks?.FirstOrDefault(userTask => userTask.Id == resultId);
        
        if (result is null)
            return NotFound();

        result.Status = resultDto.Status;
        result.Description = resultDto.Description;

        await _context.SaveChangesAsync();

        return Ok(result.Adapt<UsersTaskResult>());
    }
}