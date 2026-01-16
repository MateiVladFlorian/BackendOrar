using BackendOrar.Data;
using BackendOrar.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendOrar.Services
{
    public class MetadataService : IMetadataService
    {
        readonly OrarContext orarContext;
        public MetadataService(OrarContext orarContext) 
        { this.orarContext = orarContext; }

        public MetadataModel[] GetMetadataModels()
        {
            /* get all distinct academic years from Timetable */
            var academicYears = orarContext.Timetable
                .Select(t => t.AcademicYear)
                .Distinct()
                .ToList();

            var result = new List<MetadataModel>();

            foreach (var academicYear in academicYears)
            {
                /* get groups for this academic year */
                var groupsInYear = orarContext.Timetable
                    .Where(t => t.AcademicYear == academicYear)
                    .Select(t => t.Group)
                    .Distinct()
                    .ToList();

                /* group by study cycle */
                var cycles = groupsInYear
                    .GroupBy(g => g.StudyCycle)
                    .Select(cycleGroup => new MetaGroupModel
                    {
                        studyCycle = cycleGroup.Key,
                        years = cycleGroup
                            .GroupBy(g => g.StudyYear.ToString())
                            .Select(yearGroup => new MetaYearModel
                            {
                                studyYear = yearGroup.Key,
                                groups = yearGroup
                                    .Select(g => g.GroupCode)
                                    .OrderBy(code => code)
                                    .ToArray()
                            })
                            .OrderBy(y => y.studyYear)
                            .ToArray()
                    })
                    .OrderBy(c => c.studyCycle)
                    .ToArray();

                result.Add(new MetadataModel
                {
                    academicYear = academicYear,
                    cycles = cycles
                });
            }

            return result.OrderBy(m => m.academicYear)
                .ToArray();
        }
    }
}
