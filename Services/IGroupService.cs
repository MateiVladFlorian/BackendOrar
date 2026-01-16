using BackendOrar.Data;
using BackendOrar.Models;
using BackendOrar.Models.Filters;

namespace BackendOrar.Services
{
    public interface IGroupService
    {
        Task<Group[]> GetGroups(GroupFilterModel? filter);
        Task<(int status, Group? room, string message)> AddGroup(string accessToken, GroupRequestModel? req);
    }
}
