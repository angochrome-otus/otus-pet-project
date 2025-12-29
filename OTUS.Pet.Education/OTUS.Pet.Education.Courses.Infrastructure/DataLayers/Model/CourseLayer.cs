using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OTUS.Pet.Education.Courses.Domain.Interfaces.Repository;
using OTUS.Pet.Education.Courses.Infrastructure.Entities;
using OTUS.Pet.Education.Courses.Infrastructure.Interfaces;

namespace OTUS.Pet.Education.Courses.Infrastructure.DataLayers.Model
{
    public class CourseLayer : ICourseRepository, IDataCRUD<Course>
    {
        private readonly IDBContext _dbContext;
        public CourseLayer(IDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task AddMany(List<Course> arg)
        {
            await _dbContext.Courses.AddRangeAsync(arg);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task AddSingle(Course arg)
        {
            if (arg.Subject.Id != default)
            {
                _dbContext.Subjects.Attach(arg.Subject);
            }
            await _dbContext.Courses.AddAsync(arg);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task ChangeMany(List<Course> arg)
        {
            var map = arg.ToDictionary(key => key.Id, value => value);
            var courses = _dbContext.Courses.Where(c => map.Keys.Contains(c.Id)).ToList();
            var changedIds = new List<Guid>(map.Keys);
            foreach (var course in courses)
            {
                if (!map.TryGetValue(course.Id, out var newcourse))
                {
                    throw new KeyNotFoundException("Couldn't get course by id from map!");
                }

                course.Update(newcourse);

                changedIds.Remove(course.Id);
            }
            _dbContext.Courses.UpdateRange(courses);
            // TODO: Если метод подразумевает добавление при отсутствии записей
            if (changedIds.Any())
            {
                await _dbContext.Courses.AddRangeAsync(map.Where(kvp => changedIds.Contains(kvp.Key)).Select(kvp => kvp.Value));
            }
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task ChangeSingle(Course arg)
        {
            var course = await _dbContext.Courses.FirstOrDefaultAsync(c => c.Id == arg.Id);
            if (course is null)
            {
                await _dbContext.Courses.AddAsync(arg);
                await _dbContext._context.SaveChangesAsync();
                return;
            }
            course.Update(arg);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteMany(List<Guid> ids)
        {
            var courses = _dbContext.Courses.Where(c => ids.Contains(c.Id)).ToList();
            _dbContext.Courses.RemoveRange(courses);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteSingle(Guid id)
        {
            var course = await _dbContext.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course is null)
                return;

            _dbContext.Courses.Remove(course);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<Dictionary<Guid, Course?>> GetMany(List<Guid> ids)
        {
            var result = new Dictionary<Guid, Course?>();
            var courses = _dbContext.Courses.AsNoTracking().Where(c => ids.Contains(c.Id)).ToDictionary(key => key.Id, value => value);
            foreach (var id in ids)
            {
                if (!courses.TryGetValue(id, out var course))
                {
                    result.Add(id, null);
                    continue;
                }

                result.Add(id, course);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<Course?> GetSingle(Guid id)
        {
            var course = await _dbContext.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course is null)
                return null;

            return course;
        }

        /// <inheritdoc/>
        public async Task<List<Course>> Get(int limit)
        {
            return _dbContext.Courses.Take(limit).Include(s => s.Subject).Include(s => s.Students).ToList();
        }

        /// <inheritdoc/>
        public async Task<Domain.Models.Course?> GetById(Guid id)
        {
            var course = await GetSingle(id);
            if (course is null)
                return null;
            return (Domain.Models.Course)course;
        }

        /// <inheritdoc/>
        public async Task AddStudent(Domain.Models.Course course, Domain.Models.User student)
        {
            var dbCourse = await _dbContext.Courses.FirstOrDefaultAsync((c) => c.Id == course.Id);
            var dbStudent = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == student.Id);
            if (dbCourse is null)
                return;
            if (dbStudent is null)
                return;
            dbCourse.Students.Add(dbStudent);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task Add(Domain.Models.Course course)
        {
            await AddSingle((Course)course);
        }

        /// <inheritdoc/>
        async Task<List<Domain.Models.Course>> ICourseRepository.GetLimit(int limit)
        {
            var courses = await Get(limit);
            if (courses.Any())
                return new List<Domain.Models.Course>();

            return courses.Select(c => (Domain.Models.Course)c).ToList();
        }
    }
}