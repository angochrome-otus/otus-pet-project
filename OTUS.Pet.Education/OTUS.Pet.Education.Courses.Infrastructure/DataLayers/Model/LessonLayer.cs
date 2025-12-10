using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OTUS.Pet.Education.Courses.Infrastructure.Entities;
using OTUS.Pet.Education.Courses.Infrastructure.Interfaces;
using OTUS.Pet.Education.Courses.Infrastructure.Interfaces.Model;

namespace OTUS.Pet.Education.Courses.Infrastructure.DataLayers.Model
{
    public class LessonLayer : ILessonLayer
    {
        private readonly IDBContext _dbContext;
        public LessonLayer(IDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task AddMany(List<Lesson> arg)
        {
            await _dbContext.Lessons.AddRangeAsync(arg);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task AddSingle(Lesson arg)
        {
            await _dbContext.Lessons.AddAsync(arg);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task ChangeMany(List<Lesson> arg)
        {
            var map = arg.ToDictionary(key => key.Id, value => value);
            var lessons = _dbContext.Lessons.Where(c => map.Keys.Contains(c.Id)).ToList();
            var changedIds = new List<Guid>(map.Keys);
            foreach (var lesson in lessons)
            {
                if (!map.TryGetValue(lesson.Id, out var newlesson))
                {
                    throw new KeyNotFoundException("Couldn't get lesson by id from map!");
                }

                lesson.Update(newlesson);

                changedIds.Remove(lesson.Id);
            }
            _dbContext.Lessons.UpdateRange(lessons);
            // TODO: Если метод подразумевает добавление при отсутствии записей
            if (changedIds.Any())
            {
                await _dbContext.Lessons.AddRangeAsync(map.Where(kvp => changedIds.Contains(kvp.Key)).Select(kvp => kvp.Value));
            }
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task ChangeSingle(Lesson arg)
        {
            var lesson = await _dbContext.Lessons.FirstOrDefaultAsync(c => c.Id == arg.Id);
            if (lesson is null)
            {
                await _dbContext.Lessons.AddAsync(arg);
                await _dbContext._context.SaveChangesAsync();
                return;
            }
            lesson.Update(arg);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteMany(List<Guid> ids)
        {
            var lessons = _dbContext.Lessons.Where(c => ids.Contains(c.Id)).ToList();
            _dbContext.Lessons.RemoveRange(lessons);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteSingle(Guid id)
        {
            var lesson = await _dbContext.Lessons.FirstOrDefaultAsync(c => c.Id == id);
            if (lesson is null)
                return;

            _dbContext.Lessons.Remove(lesson);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<Dictionary<Guid, Lesson?>> GetMany(List<Guid> ids)
        {
            var result = new Dictionary<Guid, Lesson?>();
            var lessons = _dbContext.Lessons.AsNoTracking().Where(c => ids.Contains(c.Id)).ToDictionary(key => key.Id, value => value);
            foreach (var id in ids)
            {
                if (!lessons.TryGetValue(id, out var course))
                {
                    result.Add(id, null);
                    continue;
                }

                result.Add(id, course);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<Lesson?> GetSingle(Guid id)
        {
            var lesson = await _dbContext.Lessons.FirstOrDefaultAsync(c => c.Id == id);
            if (lesson is null)
                return null;

            return lesson;
        }
    }
}