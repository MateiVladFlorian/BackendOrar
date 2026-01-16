using BackendOrar.Core;
using BackendOrar.Data;
using BackendOrar.Definitions;
using BackendOrar.Models;
using BackendOrar.Models.Filters;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Group = BackendOrar.Data.Group;
#pragma warning disable

namespace BackendOrar.Services
{
    public class TimetableService : ITimetableService
    {
        readonly OrarContext orarContext;
        readonly IUserService userService;

        public TimetableService(OrarContext orarContext, IUserService userService)
        {
            this.orarContext = orarContext;
            this.userService = userService;
        }

        public async Task<TimetableEntry[]> GetTimetableEntries(int? id)
        {
            if(id != null)
            {
                var entry = await orarContext.Timetable.FirstOrDefaultAsync(x => x.Id == id.Value);
                
                if(entry != null)
                {
                    var course = await orarContext.Course
                        .FirstOrDefaultAsync(c => c.Id == entry.CourseId);

                    var professor = await orarContext.Professor
                        .FirstOrDefaultAsync(p => p.Id == entry.ProfessorId);

                    var classroom = await orarContext.Classroom
                        .FirstOrDefaultAsync(c => c.Id == entry.ClassroomId);

                    var group = await orarContext.Group
                        .FirstOrDefaultAsync(g => g.Id == entry.GroupId);

                    var tentry = new TimetableEntry
                    {
                        id = entry.Id,
                        academicYear = entry.AcademicYear,
                        studyCycle = course.StudyCycle,
                        studyYear = group.StudyYear,
                        buildingName = classroom.Building,
                        classroomCapacity = classroom.Capacity,
                        courseName = course.Name,
                        courseType = course.Type,
                        groupCode = group.GroupCode,
                        department = course.Department,
                        professorEmail = professor.Email,
                        professorName = professor.Name,
                        professorPosition = professor.Position,
                        roomName = classroom.RoomName,
                        dayOfWeek = entry.DayOfWeek,
                        range = entry.Range
                    };

                    return [tentry];
                }

                return [];
            }

            var result = new List<TimetableEntry>();
            var entries = await orarContext.Timetable.ToListAsync();

            foreach(var entry in entries)
            {
                var course = await orarContext.Course
                        .FirstOrDefaultAsync(c => c.Id == entry.CourseId);

                var professor = await orarContext.Professor
                    .FirstOrDefaultAsync(p => p.Id == entry.ProfessorId);

                var classroom = await orarContext.Classroom
                    .FirstOrDefaultAsync(c => c.Id == entry.ClassroomId);

                var group = await orarContext.Group
                    .FirstOrDefaultAsync(g => g.Id == entry.GroupId);

                var table_entry = new TimetableEntry
                {
                    id = entry.Id,
                    academicYear = entry.AcademicYear,
                    studyCycle = course.StudyCycle,
                    studyYear = group.StudyYear,
                    buildingName = classroom.Building,
                    classroomCapacity = classroom.Capacity,
                    courseName = course.Name,
                    courseType = course.Type,
                    groupCode = group.GroupCode,
                    department = course.Department,
                    professorEmail = professor.Email,
                    professorName = professor.Name,
                    professorPosition = professor.Position,
                    roomName = classroom.RoomName,
                    dayOfWeek = entry.DayOfWeek,
                    range = entry.Range
                };

                result.Add(table_entry);
            }

            return result.ToArray();
        }

        public async Task<TimetableEntry[]> GetFilteredTimetableEntries(TimetableFilterModel? model)
        {
            if(model == null)
            {
                var list = await GetTimetableEntries(null);
                return list;
            }

            var result = (from e in await orarContext.Timetable.ToListAsync()
                          join c in await orarContext.Course.ToListAsync() on e.CourseId equals c.Id
                          join r in await orarContext.Classroom.ToListAsync() on e.ClassroomId equals r.Id
                          join g in await orarContext.Group.ToListAsync() on e.GroupId equals g.Id
                          join p in await orarContext.Professor.ToListAsync() on e.ProfessorId equals p.Id
                          where ((model.cycle == null) || (model.cycle != null && c.StudyCycle.ToLower().CompareTo(model.cycle.ToLower()) == 0))
                                && ((model.year == null) || (model.year != null && g.StudyYear == model.year.Value))
                                && ((model.groupCode == null) || (model.groupCode != null && model.groupCode.ToLower().CompareTo(g.GroupCode.ToLower()) == 0))
                                && ((model.cname == null) || (!string.IsNullOrEmpty(model.cname) && model.cname.ToLower().CompareTo(c.Name.ToLower()) == 0))
                                && ((model.pname == null) || (!string.IsNullOrEmpty(model.pname) && model.pname.ToLower().CompareTo(p.Name.ToLower()) == 0))
                                && ((model.rname == null) || (!string.IsNullOrEmpty(model.rname) && model.rname.ToLower().CompareTo(r.RoomName.ToLower()) == 0))
                          select new TimetableEntry
                          {
                              id = e.Id,
                              courseName = c.Name,
                              classroomCapacity = r.Capacity,
                              courseType = c.Type,
                              groupCode = g.GroupCode,
                              studyCycle = c.StudyCycle,
                              academicYear = e.AcademicYear,
                              buildingName = r.Building,
                              dayOfWeek = e.DayOfWeek,
                              department = c.Department,
                              professorEmail = p.Email,
                              professorName = p.Name,
                              professorPosition = p.Position,
                              range = e.Range,
                              roomName = r.RoomName,
                              studyYear = g.StudyYear
                          }
                          ).ToArray();

            return result;
        }

        public async Task<(int status, Timetable? timetable, string message)> AddTimetableEntry(string accessToken, TimetableRequestModel? model)
        {
            var user = await userService.GetAccountFromAccessTokenAsync(accessToken);
            bool isValid = await userService.IsAccessTokenExpired(accessToken);

            if (user == null || (user != null && isValid))
                return (-3, null, "Invalid or expired token.");
            var role = (UserRole)user.UserRole;

            if (role != UserRole.Administrator && role != UserRole.Editor) 
                return (-3, null, "Insufficient permissions.");

            if (!TimetableValidator.IsValidTimetableEntry(model)) 
                return (-2, null, "Request body cannot be null.");

            var course = await orarContext.Course.
                FirstOrDefaultAsync(c =>
                c.Name.CompareTo(model.courseName) == 0
                && c.Type.CompareTo(model.courseType) == 0
                && c.Department.CompareTo(model.department) == 0
                && c.StudyCycle.CompareTo(model.studyCycle) == 0);

            int courseId = -1;
            int professorId = -1;
            int classroomId = -1;
            int groupId = -1;


            /* not found, insert it first */
            if (course == null)
            {
                course = new Course
                {
                    Name = model.courseName,
                    Type = model.courseType,
                    Department = model.department,
                    StudyCycle = model.studyCycle
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
            }

            var foundRoom = await orarContext.Classroom
                .FirstOrDefaultAsync(p => p.Building.CompareTo(model.buildingName) == 0
            && (model.classroomCapacity == null) || (model.classroomCapacity != null && p.Capacity == model.classroomCapacity)
            && p.RoomName.CompareTo(model.roomName) == 0);

            /* check for existing classroom */
            if (foundRoom == null)
            {
                foundRoom = new Classroom
                {
                    Building = model.buildingName,
                    Capacity = model.classroomCapacity.Value,
                    RoomName = model.roomName
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
            }

            var foundProfessor = await orarContext.Professor
                .FirstOrDefaultAsync(p => p.Email.CompareTo(model.professorEmail) == 0
                || (p.Name.CompareTo(model.professorName) == 0
                && p.Department.CompareTo(model.department) == 0));

            if(foundProfessor == null)
            {
                foundProfessor = new Professor
                {
                    Name = model.professorName,
                    Email = model.professorEmail,
                    Department = model.department,
                    Position = model.professorPosition
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
            }

            var foundGroup = await orarContext.Group
                .FirstOrDefaultAsync(p => p.StudyCycle.CompareTo(model.studyCycle) == 0
                && p.GroupCode.CompareTo(model.groupCode) == 0
                && p.StudyYear == model.studyYear);

            if (foundGroup == null)
            {
                foundGroup = new Group
                {
                    StudyCycle = model.studyCycle,
                    GroupCode = model.groupCode,
                    StudyYear = model.studyYear.Value
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
            }

            courseId = course.Id;
            classroomId = foundRoom.Id;

            professorId = foundProfessor.Id;
            groupId = foundGroup.Id;

            var entries = await orarContext.Timetable
                .Where(entry => entry.GroupId == groupId
                    && entry.DayOfWeek.CompareTo(model.dayOfWeek) == 0
                    && entry.AcademicYear.CompareTo(model.academicYear) == 0)
                .ToListAsync();

            bool conflict = await IsConflict(entries, model);
            if (conflict) return (0, null, "Timetable entry are overlapping.");

            var entry = new Timetable
            {
                CourseId = courseId,
                ClassroomId = classroomId,
                ProfessorId = professorId,
                AcademicYear = model.academicYear,
                DayOfWeek = model.dayOfWeek,
                Range = model.range,
                GroupId = groupId
            };

            await orarContext.Timetable.AddAsync(entry);
            await orarContext.SaveChangesAsync();

            await orarContext.Entry(entry).ReloadAsync();
            return (1, entry, "Timetable entry created successfully.");
        }

        public async Task<(int status, string message)> UpdateTimetableEntry(int id, string accessToken, TimetableUpdateModel? model)
        {
            var user = await userService.GetAccountFromAccessTokenAsync(accessToken);
            bool isValid = await userService.IsAccessTokenExpired(accessToken);

            if (user == null || (user != null && isValid))
                return (-3, "Invalid or expired token.");
            var role = (UserRole)user.UserRole;

            if (role != UserRole.Administrator && role != UserRole.Editor)
                return (-3, "Insufficient permissions.");

            if (!TimetableValidator.IsValidUpdateBody(model))
                return (-2, "Timetable update body cannot be null or malformed.");

            var entry = await orarContext.Timetable
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entry == null)
                return (0, "Timetable entry does not exists.");

            var course = await orarContext.Course
                .FirstOrDefaultAsync(c => c.Id == model.course_id.Value);

            var professor = await orarContext.Professor
                .FirstOrDefaultAsync(p => p.Id == model.professor_id.Value);

            var classroom = await orarContext.Classroom
                .FirstOrDefaultAsync(cr => cr.Id == model.classroom_id.Value);

            var group = await orarContext.Group
                .FirstOrDefaultAsync(g => g.Id == model.group_id.Value);

            if(course != null &&  professor != null 
                &&  classroom != null &&  group != null)
            {
                entry.CourseId = course.Id;
                entry.ProfessorId = professor.Id;

                entry.GroupId = group.Id;
                entry.ClassroomId = classroom.Id;

                entry.AcademicYear = model.academicYear;
                entry.DayOfWeek = model.dayOfWeek;
                entry.Range = model.range;

                var entries = await orarContext.Timetable
                    .Where(entry => entry.GroupId == model.group_id
                        && entry.DayOfWeek.CompareTo(model.dayOfWeek) == 0
                        && entry.AcademicYear.CompareTo(model.academicYear) == 0)
                    .ToListAsync();

                bool conflict = await IsConflict(id, entries, model);
                if (conflict) return (-1, "Timetable entry are overlapping.");

                orarContext.Timetable.Update(entry);
                await orarContext.SaveChangesAsync();
                return (1, "Timetable entry has been updated successfully.");
            }

            return (-2, "Timetable update body is malformed.");
        }

        public async Task<(int status, string message)> DeleteTimetableEntry(string accessToken, int id)
        {
            var user = await userService.GetAccountFromAccessTokenAsync(accessToken);
            bool isValid = await userService.IsAccessTokenExpired(accessToken);

            if (user == null || (user != null && isValid))
                return (-2, "Invalid or expired token.");

            var role = (UserRole)user.UserRole;
            if (role != UserRole.Administrator && role != UserRole.Editor)
                return (-2, "Insufficient permissions.");

            /* use a transaction to ensure all operations succeed or fail together */
            using var transaction = await orarContext.Database.BeginTransactionAsync();

            try
            {
                var existingEntry = await orarContext.Timetable
                    .FirstOrDefaultAsync(entry => entry.Id == id);

                if (existingEntry == null)
                    return (-1, "The corresponding timetable entry does not exists.");

                var course = await orarContext.Course
                    .FirstOrDefaultAsync(c => c.Id == existingEntry.CourseId);

                var professor = await orarContext.Professor
                    .FirstOrDefaultAsync(p => p.Id == existingEntry.ProfessorId);

                var classroom = await orarContext.Classroom
                    .FirstOrDefaultAsync(cr => cr.Id == existingEntry.ClassroomId);

                var group = await orarContext.Group
                    .FirstOrDefaultAsync(g => g.Id == existingEntry.GroupId);

                if (course == null || group == null || professor == null || classroom == null)
                    return (0, "Internal state error occured due of database errors.");

                /* first, remove the timetable entry */
                orarContext.Timetable.Remove(existingEntry);
                await orarContext.SaveChangesAsync();

                /* check if related entities are still referenced by other timetable entries */
                var hasOtherGroupEntries = await orarContext.Timetable
                    .AnyAsync(entry => entry.GroupId == group.Id);

                if (!hasOtherGroupEntries)
                    orarContext.Group.Remove(group);

                var hasOtherCourseEntries = await orarContext.Timetable
                    .AnyAsync(entry => entry.CourseId == course.Id);

                if (!hasOtherCourseEntries)
                    orarContext.Course.Remove(course);

                var hasOtherClassroomEntries = await orarContext.Timetable
                    .AnyAsync(entry => entry.ClassroomId == classroom.Id);

                if (!hasOtherClassroomEntries)
                    orarContext.Classroom.Remove(classroom);

                var hasOtherProfessorEntries = await orarContext.Timetable
                    .AnyAsync(entry => entry.ProfessorId == professor.Id);

                if (!hasOtherProfessorEntries)
                    orarContext.Professor.Remove(professor);

                /* save all entity removals at once */
                await orarContext.SaveChangesAsync();

                /* commit the transaction */
                await transaction.CommitAsync();
                return (1, "Timetable entry was removed successfully.");
            }
            catch (Exception ex)
            {
                /* rollback the transaction on error */
                await transaction.RollbackAsync();
                return (0, $"An error occurred while deleting the timetable entry: {ex.Message}");
            }
        }

        private async Task<bool> IsConflict(List<Timetable>? entries, TimetableRequestModel? model)
        {
            if ((entries == null || (entries != null && entries.Count == 0)) || model == null) 
                return false;

            var sameProfessor = await orarContext.Professor.FirstOrDefaultAsync(prof =>
              prof.Email.CompareTo(model.professorEmail) == 0
              && prof.Name.CompareTo(model.professorName) == 0
              && prof.Department.CompareTo(model.department) == 0);

            var sameRoom = await orarContext.Classroom.FirstOrDefaultAsync(room => 
                room.Building.CompareTo(model.buildingName) == 0
                && room.RoomName.CompareTo(model.roomName) == 0
                && room.Capacity == model.classroomCapacity.Value);

            foreach (var entry in entries)
            {
                /* same classroom or professor, cannot be in same time, need to check exact day and time */
                if(entry.ProfessorId == sameProfessor.Id || entry.ClassroomId == sameRoom.Id)
                {
                    var entryDay = TimetableValidator.GetDayOfWeek(entry.DayOfWeek);
                    var mainDay = TimetableValidator.GetDayOfWeek(model.dayOfWeek);
                    
                    /* same day detected */
                    if(entryDay == mainDay)
                    {
                        var entryRanger = TimeRangeParser.Parse(entry.Range);
                        var mainRanger = TimeRangeParser.Parse(model.range);

                        if (!entryRanger.AreIntervalsDisjoint(mainRanger))
                            return true;
                    }
                }
            }

            return false;
        }

        private async Task<bool> IsConflict(int id, List<Timetable>? entries, TimetableUpdateModel model)
        {
            if ((entries == null || (entries != null && entries.Count == 0)) || model == null)
                return false;

            var sameProfessor = await orarContext.Professor
                .FirstOrDefaultAsync(prof => prof.Id == model.professor_id.Value);

            var sameRoom = await orarContext.Classroom
                .FirstOrDefaultAsync(room => room.Id == model.classroom_id.Value);

            foreach (var entry in entries)
            {
                /* same classroom or professor, cannot be in same time, need to check exact day and time */
                if (entry.Id != id && (entry.ProfessorId == sameProfessor.Id || entry.ClassroomId == sameRoom.Id))
                {
                    var entryDay = TimetableValidator.GetDayOfWeek(entry.DayOfWeek);
                    var mainDay = TimetableValidator.GetDayOfWeek(model.dayOfWeek);

                    /* same day detected */
                    if (entryDay == mainDay)
                    {
                        var entryRanger = TimeRangeParser.Parse(entry.Range);
                        var mainRanger = TimeRangeParser.Parse(model.range);

                        if (!entryRanger.AreIntervalsDisjoint(mainRanger))
                            return true;
                    }
                }
            }

            return false;
        }
    }
}
