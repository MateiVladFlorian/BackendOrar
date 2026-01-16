using BackendOrar.Data;
using BackendOrar.Definitions;
using BackendOrar.Models;
using BackendOrar.Models.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
#pragma warning disable

namespace BackendOrar.Services
{
    public class CourseService : ICourseService
    {
        readonly OrarContext orarContext;
        readonly IUserService userService;

        public CourseService(IUserService userService, OrarContext orarContext)
        { 
            this.orarContext = orarContext; 
            this.userService = userService;
        }

        public async Task<Course[]> GetCourses(CourseFilterModel? filter)
        {
            var filteredCourses = await orarContext.Course
                .Where(c => (filter.id == null) || (filter.id != null && c.Id == filter.id)
                    && c.Name.CompareTo(filter.name) == 0
                    && c.Type.CompareTo(filter.type) == 0
                    && c.Department.CompareTo(filter.department) == 0
                    && c.StudyCycle.CompareTo(filter.studyCycle) == 0)
                .ToListAsync();

            return filteredCourses != null ? filteredCourses.ToArray()
                : [];
        }

        public async Task<(int status, Course? course, string message)> AddCourse(string accessToken, CourseRequestModel? req)
        {
            var user = await userService.GetAccountFromAccessTokenAsync(accessToken);
            bool isValid = await userService.IsAccessTokenExpired(accessToken);

            if (user == null || (user != null && isValid)) 
                return (-3, null, "Invalid or expired token.");
            var role = (UserRole)user.UserRole;

            if (role != UserRole.Administrator) return (-3, null, "Insufficient permissions.");
            if (req == null) return (-2, null, "Request body cannot be null.");      
            else
            {
                var course = await orarContext.Course.
                    FirstOrDefaultAsync(c =>
                    c.Name.CompareTo(req.name) == 0
                    && c.Type.CompareTo(req.type) == 0
                    && c.Department.CompareTo(req.department) == 0
                    && c.StudyCycle.CompareTo(req.studyCycle) == 0);

                if (course != null) return (0, course, "The course is already registered.");
                else
                {
                    course = new Course
                    {
                        Name = req.name,
                        Type = req.type,
                        Department = req.department,
                        StudyCycle = req.studyCycle
                    };

                    try
                    {
                        await orarContext.Course.AddAsync(course);
                        await orarContext.SaveChangesAsync();
                    }
                    catch (DbUpdateException ex)
                    {
                        return (-1, null, $"Database error: {ex.InnerException?.Message ?? ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        return (-1, null, $"Unexpected error: {ex.Message}");
                    }

                    await orarContext.Entry(course).ReloadAsync();
                    return (1, course, "Course created successfully.");
                }
            }
        }
    }
}
