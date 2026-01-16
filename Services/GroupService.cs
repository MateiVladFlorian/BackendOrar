using BackendOrar.Data;
using BackendOrar.Definitions;
using BackendOrar.Models;
using BackendOrar.Models.Filters;
using Microsoft.EntityFrameworkCore;
#pragma warning disable

namespace BackendOrar.Services
{
    public class GroupService : IGroupService
    {
        readonly OrarContext orarContext;
        readonly IUserService userService;

        public GroupService(OrarContext orarContext, IUserService userService)
        {
            this.orarContext = orarContext;
            this.userService = userService;
        }

        public async Task<Group[]> GetGroups(GroupFilterModel? filter)
        {
            var filteredRooms = await orarContext.Group
                .Where(p => (filter.id == null) || (filter.id != null && p.Id == filter.id.Value)
                && p.StudyCycle.CompareTo(filter.studyCycle) == 0
                && (filter.studyYear == null) || (filter.studyYear != null && p.StudyYear == filter.studyYear.Value)
                && p.StudyCycle.CompareTo(filter.studyCycle) == 0)
                .ToListAsync();

            return filteredRooms != null ? filteredRooms.ToArray()
                : [];
        }

        public async Task<(int status, Group? room, string message)> AddGroup(string accessToken, GroupRequestModel? req)
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
                var foundGroup = await orarContext.Group
                    .FirstOrDefaultAsync(p => p.StudyCycle.CompareTo(req.studyCycle) == 0
                    && p.GroupCode.CompareTo(req.groupCode) == 0
                    && p.StudyYear == req.studyYear);

                if (foundGroup != null) return (0, foundGroup, "The group is already registered.");
                else
                {
                    foundGroup = new Group
                    {
                        StudyCycle = req.studyCycle,
                        GroupCode = req.groupCode,
                        StudyYear = req.studyYear
                    };

                    try
                    {
                        await orarContext.Group.AddAsync(foundGroup);
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

                    await orarContext.Entry(foundGroup).ReloadAsync();
                    return (1, foundGroup, "Group added successfully.");
                }
            }
        }
    }
}
