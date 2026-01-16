using BackendOrar.Data;
using BackendOrar.Models;
using BackendOrar.Models.Filters;

namespace BackendOrar.Services
{
    public interface IProfessorService
    {
        Task<Professor[]> GetProfessors(ProfessorFilterModel? filter);
        Task<(int status, Professor? professor, string message)> AddProfessor(string accessToken, ProfessorRequestModel? req);
    }
}
