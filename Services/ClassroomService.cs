using BackendOrar.Data;
using BackendOrar.Definitions;
using BackendOrar.Models;
using BackendOrar.Models.Filters;
using Microsoft.EntityFrameworkCore;
#pragma warning disable

namespace BackendOrar.Services
{
    public class ClassroomService : IClassroomService
    {
        readonly OrarContext orarContext;
        readonly IUserService userService;

        public ClassroomService(OrarContext orarContext, IUserService userService)
        {
            this.orarContext = orarContext;
            this.userService = userService;
        }

        public async Task<Classroom[]> GetClassrooms(ClassroomFilterModel? filter)
        {
            var filteredRooms = await orarContext.Classroom
                .Where(p => (filter.id == null) || (filter.id != null && p.Id == filter.id.Value)
                && p.Building.CompareTo(filter.building) == 0
                && (filter.capacity == null) || (filter.capacity != null && p.Capacity == filter.capacity.Value)
                && p.RoomName.CompareTo(filter.roomName) == 0)
                .ToListAsync();

            return filteredRooms != null ? filteredRooms.ToArray()
                : [];
        }

        public async Task<(int status, Classroom? room, string message)> AddClassroom(string accessToken, ClassroomRequestModel? req)
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
                var foundRoom = await orarContext.Classroom
                    .FirstOrDefaultAsync(p => p.Building.CompareTo(req.building) == 0
                && (req.capacity == null) || (req.capacity != null && p.Capacity == req.capacity)
                && p.RoomName.CompareTo(req.roomName) == 0);

                if (foundRoom != null) return (0, foundRoom, "The classroom is already registered.");
                else
                {
                    foundRoom = new Classroom
                    {
                        Building = req.building,
                        Capacity = req.capacity,
                        RoomName = req.roomName
                    };

                    try
                    {
                        await orarContext.Classroom.AddAsync(foundRoom);
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

                    await orarContext.Entry(foundRoom).ReloadAsync();
                    return (1, foundRoom, "Classroom added successfully.");
                }
            }
        }
    }
}
