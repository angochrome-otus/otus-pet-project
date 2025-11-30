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
    public class RoleLayer : IRoleLayer
    {
        private readonly IDBContext _dbContext;
        public RoleLayer(IDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task AddMany(List<Role> arg)
        {
            await _dbContext.Roles.AddRangeAsync(arg);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task AddSingle(Role arg)
        {
            await _dbContext.Roles.AddAsync(arg);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task ChangeMany(List<Role> arg)
        {
            var map = arg.ToDictionary(key => key.Id, value => value);
            var roles = _dbContext.Roles.Where(c => map.Keys.Contains(c.Id)).ToList();
            var changedIds = new List<Guid>(map.Keys);
            foreach (var role in roles)
            {
                if (!map.TryGetValue(role.Id, out var newrole))
                {
                    throw new KeyNotFoundException("Couldn't get role by id from map!");
                }

                role.Update(newrole);

                changedIds.Remove(role.Id);
            }
            _dbContext.Roles.UpdateRange(roles);
            // TODO: Если метод подразумевает добавление при отсутствии записей
            if (changedIds.Any())
            {
                await _dbContext.Roles.AddRangeAsync(map.Where(kvp => changedIds.Contains(kvp.Key)).Select(kvp => kvp.Value));
            }
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task ChangeSingle(Role arg)
        {
            var role = await _dbContext.Roles.FirstOrDefaultAsync(c => c.Id == arg.Id);
            if (role is null)
            {
                await _dbContext.Roles.AddAsync(arg);
                await _dbContext._context.SaveChangesAsync();
                return;
            }
            role.Update(arg);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteMany(List<Guid> ids)
        {
            var roles = _dbContext.Roles.Where(c => ids.Contains(c.Id)).ToList();
            _dbContext.Roles.RemoveRange(roles);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteSingle(Guid id)
        {
            var role = await _dbContext.Roles.FirstOrDefaultAsync(c => c.Id == id);
            if (role is null)
                return;

            _dbContext.Roles.Remove(role);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<Dictionary<Guid, Role?>> GetMany(List<Guid> ids)
        {
            var result = new Dictionary<Guid, Role?>();
            var roles = _dbContext.Roles.AsNoTracking().Where(c => ids.Contains(c.Id)).ToDictionary(key => key.Id, value => value);
            foreach (var id in ids)
            {
                if (!roles.TryGetValue(id, out var role))
                {
                    result.Add(id, null);
                    continue;
                }

                result.Add(id, role);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<Role?> GetSingle(Guid id)
        {
            var role = await _dbContext.Roles.FirstOrDefaultAsync(c => c.Id == id);
            if (role is null)
                return null;

            return role;
        }
    }
}