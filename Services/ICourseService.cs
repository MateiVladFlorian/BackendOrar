using BackendOrar.Data;
using BackendOrar.Models;
using BackendOrar.Models.Filters;

namespace BackendOrar.Services
{
    public interface ICourseService
    {
        Task<Course[]> GetCourses(CourseFilterModel? model);
        Task<(int status, Course? course, string message)> AddCourse(string accessToken, CourseRequestModel? req);
    }
}
