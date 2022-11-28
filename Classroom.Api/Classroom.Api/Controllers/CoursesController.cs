using Classroom.Api.Context;
using Classroom.Api.Entities;
using Classroom.Api.Filters;
using Classroom.Api.Mappers;
using Classroom.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[TypeFilter(typeof(IsCourseExistsActionFilterAttribute))]
[TypeFilter(typeof(IsTaskExistsActionFilterAttribute))]
public partial class CoursesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public CoursesController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<CourseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCourses()
    {
        var courses = await _context.Courses.ToListAsync();
        List<CourseDto> coursesDto = courses.Select(c => c.ToDto()).ToList();
        return Ok(coursesDto);
    }

    [HttpGet("{courseId}")]
    //[TypeFilter(typeof(CourseAdminFilterAttribute), Arguments = new object[] { false })]
    [IsCourseUserOrAdmin(true)]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCourseById(Guid courseId)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

        return Ok(course?.ToDto());
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto createCourseDto)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var user = await _userManager.GetUserAsync(User);

        var course = new Course()
        {
            Name = createCourseDto.Name,
            Key = Guid.NewGuid().ToString("N"),
            Users = new List<UserCourse>()
            {
                new UserCourse()
                {
                    UserId = user.Id,
                    IsAdmin = true
                }
            }
        };

        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();

        course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == course.Id);

        return Ok(course?.ToDto());
    }

    [HttpPut("{courseId}")]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCourse(Guid courseId, [FromBody] UpdateCourseDto updateCourseDto)
    {
        if (!await _context.Courses.AnyAsync(c => c.Id == courseId))
            return NotFound();

        if (!ModelState.IsValid)
            return BadRequest();

        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

        if (course is null)
            return NotFound();

        var user = await _userManager.GetUserAsync(User);

        if (course.Users?.Any(uc => uc.UserId == user.Id && uc.IsAdmin) != true)
            return BadRequest();

        course.Name = updateCourseDto.Name;

        await _context.SaveChangesAsync();

        return Ok(course?.ToDto());
    }

    [HttpDelete("{courseId}")]
    public async Task<IActionResult> DeleteCourse(Guid courseId)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

        if (course is null)
            return NotFound();

        var user = await _userManager.GetUserAsync(User);

        if (course.Users?.Any(uc => uc.UserId == user.Id && uc.IsAdmin) != true)
            return Forbid();

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("{courseId}/join")]
    public async Task<IActionResult> JoinCourse(Guid courseId, [FromBody] JoinCourseDto joinCourseDto)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

        if (course is null)
            return NotFound();

        var user = await _userManager.GetUserAsync(User);

        if (course.Users?.Any(uc => uc.UserId == user.Id) == true)
            return BadRequest();

        _context.UserCourses.Add(new UserCourse()
        {
            UserId = user.Id,
            CourseId = course.Id,
            IsAdmin = false
        });

        await _context.SaveChangesAsync();

        return Ok();
    }
}