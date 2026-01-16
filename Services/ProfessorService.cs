using BackendOrar.Data;
using BackendOrar.Definitions;
using BackendOrar.Models;
using BackendOrar.Models.Filters;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
#pragma warning disable

namespace BackendOrar.Services
{
    public class ProfessorService : IProfessorService
    {
        readonly OrarContext orarContext;
        readonly IUserService userService;

        public ProfessorService(OrarContext orarContext, IUserService userService)
        {
            this.orarContext = orarContext;
            this.userService = userService;
        }

        public async Task<Professor[]> GetProfessors(ProfessorFilterModel? filter)
        {
            var filteredProfessors = await orarContext.Professor
                .Where(p => (filter.id == null) || (filter.id != null && p.Id == filter.id) 
                && p.Email.CompareTo(filter.email) == 0
                && p.Department.CompareTo(filter.department) == 0
                && p.Position.CompareTo(filter.position) == 0)
                .ToListAsync();

            return filteredProfessors != null ? filteredProfessors.ToArray()
                : [];
        }

        public async Task<(int status, Professor? professor, string message)> AddProfessor(string accessToken, ProfessorRequestModel? req)
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
                var foundProfessor = await orarContext.Professor
                    .FirstOrDefaultAsync(p => p.Email.CompareTo(req.email) == 0
                    || (p.Name.CompareTo(req.name) == 0
                    && p.Department.CompareTo(req.department) == 0));

                if (foundProfessor != null) return (0, foundProfessor, "The professor is already registered.");
                else
                {
                    foundProfessor = new Professor
                    {
                        Name = req.name,
                        Email = req.email,
                        Department = req.department,
                        Position = req.position
                    };

                    try
                    {
                        await orarContext.Professor.AddAsync(foundProfessor);
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

                    await orarContext.Entry(foundProfessor).ReloadAsync();
                    return (1, foundProfessor, "Professor added successfully.");
                }
            }
        }
    }
}
