using BackendOrar.Data;
using BackendOrar.Models;
using BackendOrar.Models.Filters;

namespace BackendOrar.Services
{
    public interface ITimetableService
    {
        Task<TimetableEntry[]> GetTimetableEntries(int? id);
        Task<TimetableEntry[]> GetFilteredTimetableEntries(TimetableFilterModel? model);
        Task<(int status, Timetable? timetable, string message)> AddTimetableEntry(string accessToken, TimetableRequestModel? model);
        Task<(int status, string message)> UpdateTimetableEntry(int id, string accessToken, TimetableUpdateModel? model);
        Task<(int status, string message)> DeleteTimetableEntry(string accessToken, int id);
    }
}
