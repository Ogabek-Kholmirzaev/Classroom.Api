﻿using Classroom.Api.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Classroom.Api.Filters;

public class CourseAdminFilterAttribute : ActionFilterAttribute
{
    public bool OnlyAdmin { get; set; }
    private readonly ApplicationDbContext _context;

    public CourseAdminFilterAttribute(ApplicationDbContext context, bool onlyAdmin)
    {
        _context = context;
        OnlyAdmin = onlyAdmin;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ActionArguments.ContainsKey("courseId"))
        {
            await next();
            return;
        }

        var userClaims = context.HttpContext.User;

        var userId = userClaims.FindFirst(userClaim => userClaim.Type == ClaimTypes.NameIdentifier)?.Value;

        var courseId = (Guid?)context.ActionArguments["courseId"];

        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

        if (course is null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        var userCourse = course?.Users?.FirstOrDefault(userCourse => userCourse.UserId.ToString() == userId);
        if (userCourse is null)
        {
            context.Result = new BadRequestResult();
            return;
        }

        if (OnlyAdmin)
        {
            if (!userCourse.IsAdmin)
            {
                context.Result = new BadRequestResult();
                return;
            }
        }

        await next();
    }
}