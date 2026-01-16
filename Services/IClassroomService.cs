using BackendOrar.Data;
using BackendOrar.Models;
using BackendOrar.Models.Filters;

namespace BackendOrar.Services
{
    public interface IClassroomService
    {
        Task<Classroom[]> GetClassrooms(ClassroomFilterModel? filter);
        Task<(int status, Classroom? room, string message)> AddClassroom(string accessToken, ClassroomRequestModel? req);
    }
}
