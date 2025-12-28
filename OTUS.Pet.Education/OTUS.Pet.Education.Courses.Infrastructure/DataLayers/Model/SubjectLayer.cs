using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OTUS.Pet.Education.Courses.Domain.Interfaces.Repository;
using OTUS.Pet.Education.Courses.Infrastructure.Entities;
using OTUS.Pet.Education.Courses.Infrastructure.Interfaces;

namespace OTUS.Pet.Education.Courses.Infrastructure.DataLayers.Model
{
    public class SubjectLayer : ISubjectRepository, IDataCRUD<Subject>, IDisposable
    {
        private readonly IDBContext _dbContext;
        public SubjectLayer(IDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Dispose
        private bool disposed = false;

        // реализация интерфейса IDisposable.
        public void Dispose()
        {
            // освобождаем неуправляемые ресурсы
            Dispose(true);
            // подавляем финализацию
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                // Освобождаем управляемые ресурсы
            }
            // освобождаем неуправляемые объекты
            disposed = true;
        }

        // Деструктор
        ~SubjectLayer()
        {
            Dispose(false);
        }
        #endregion

        /// <inheritdoc/>
        public async Task AddMany(List<Subject> arg)
        {
            await _dbContext.Subjects.AddRangeAsync(arg);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task AddSingle(Subject arg)
        {
            await _dbContext.Subjects.AddAsync(arg);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task ChangeMany(List<Subject> arg)
        {
            var map = arg.ToDictionary(key => key.Id, value => value);
            var subjects = _dbContext.Subjects.Where(c => map.Keys.Contains(c.Id)).ToList();
            var changedIds = new List<Guid>(map.Keys);
            foreach (var subject in subjects)
            {
                if (!map.TryGetValue(subject.Id, out var newsubject))
                {
                    throw new KeyNotFoundException("Couldn't get subject by id from map!");
                }

                subject.Update(newsubject);

                changedIds.Remove(subject.Id);
            }
            _dbContext.Subjects.UpdateRange(subjects);
            // TODO: Если метод подразумевает добавление при отсутствии записей
            if (changedIds.Any())
            {
                await _dbContext.Subjects.AddRangeAsync(map.Where(kvp => changedIds.Contains(kvp.Key)).Select(kvp => kvp.Value));
            }
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task ChangeSingle(Subject arg)
        {
            var subject = await _dbContext.Subjects.FirstOrDefaultAsync(c => c.Id == arg.Id);
            if (subject is null)
            {
                await _dbContext.Subjects.AddAsync(arg);
                await _dbContext._context.SaveChangesAsync();
                return;
            }
            subject.Update(arg);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteMany(List<Guid> ids)
        {
            var subjects = _dbContext.Subjects.Where(c => ids.Contains(c.Id)).ToList();
            _dbContext.Subjects.RemoveRange(subjects);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteSingle(Guid id)
        {
            var subject = await _dbContext.Subjects.FirstOrDefaultAsync(c => c.Id == id);
            if (subject is null)
                return;

            _dbContext.Subjects.Remove(subject);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<Dictionary<Guid, Subject?>> GetMany(List<Guid> ids)
        {
            var result = new Dictionary<Guid, Subject?>();
            var subjects = _dbContext.Subjects.AsNoTracking().Where(c => ids.Contains(c.Id)).ToDictionary(key => key.Id, value => value);
            foreach (var id in ids)
            {
                if (!subjects.TryGetValue(id, out var subject))
                {
                    result.Add(id, null);
                    continue;
                }

                result.Add(id, subject);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<Subject?> GetSingle(Guid id)
        {
            var subject = await _dbContext.Subjects.FirstOrDefaultAsync(c => c.Id == id);
            if (subject is null)
                return null;

            return subject;
        }

        /// <inheritdoc/>
        public async Task<List<Subject>> Get(int limit)
        {
            return _dbContext.Subjects.Take(limit).ToList();
        }

        /// <inheritdoc/>
        public async Task<Courses.Domain.Models.Subject?> GetByName(string name)
        {
            var subject = _dbContext.Subjects.AsNoTracking().FirstOrDefault(c => c.Name.ToLower() == name.ToLower());
            if (subject is null)
                return null;
            return (Courses.Domain.Models.Subject)subject;
        }

        /// <inheritdoc/>
        public async Task Add(Domain.Models.Subject subject)
        {
            await AddSingle((Subject)subject);
        }
    }
}