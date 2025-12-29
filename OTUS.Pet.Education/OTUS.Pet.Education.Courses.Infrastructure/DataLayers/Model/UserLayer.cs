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
    public class UserLayer : IUserRepository, IDataCRUD<User>
    {
        private readonly IDBContext _dbContext;
        public UserLayer(IDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task AddMany(List<User> arg)
        {
            await _dbContext.Users.AddRangeAsync(arg);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task AddSingle(User arg)
        {
            // Попытка устранения дублей ролей. Проблема наличия для каждого пользователя свой записи роли.
            // if (arg.Roles is not null && arg.Roles.Count > 0)
            // {
            //     try
            //     {
            //         var mapValues = arg.Roles.ToDictionary(key => key.Name, value => value);
            //         var roles = _dbContext.Roles.Where(r => mapValues.Keys.Contains(r.Name));
            //         foreach (var role in roles)
            //         {
            //             if (!mapValues.TryGetValue(role.Name, out var userRole))
            //             {
            //                 // Придумать exception
            //                 throw new Exception("");
            //             }

            //             arg.Roles.Remove(userRole);
            //             arg.Roles.Add(role);
            //         }
            //     }
            //     catch (Exception ex)
            //     {
            //         throw new Exception("Dublicate user roles!", ex);
            //     }
            // }
            await _dbContext.Users.AddAsync(arg);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task ChangeMany(List<User> arg)
        {
            var map = arg.ToDictionary(key => key.Id, value => value);
            var users = _dbContext.Users.Where(c => map.Keys.Contains(c.Id)).ToList();
            var changedIds = new List<Guid>(map.Keys);
            foreach (var user in users)
            {
                if (!map.TryGetValue(user.Id, out var newuser))
                {
                    throw new KeyNotFoundException("Couldn't get user by id from map!");
                }

                user.Update(newuser);

                changedIds.Remove(user.Id);
            }
            _dbContext.Users.UpdateRange(users);
            // TODO: Если метод подразумевает добавление при отсутствии записей
            if (changedIds.Any())
            {
                await _dbContext.Users.AddRangeAsync(map.Where(kvp => changedIds.Contains(kvp.Key)).Select(kvp => kvp.Value));
            }
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task ChangeSingle(User arg)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(c => c.Id == arg.Id);
            if (user is null)
            {
                await _dbContext.Users.AddAsync(arg);
                await _dbContext._context.SaveChangesAsync();
                return;
            }
            user.Update(arg);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteMany(List<Guid> ids)
        {
            var users = _dbContext.Users.Where(c => ids.Contains(c.Id)).ToList();
            _dbContext.Users.RemoveRange(users);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteSingle(Guid id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(c => c.Id == id);
            if (user is null)
                return;

            _dbContext.Users.Remove(user);
            await _dbContext._context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<Dictionary<Guid, User?>> GetMany(List<Guid> ids)
        {
            var result = new Dictionary<Guid, User?>();
            var users = _dbContext.Users.AsNoTracking().Where(c => ids.Contains(c.Id)).ToDictionary(key => key.Id, value => value);
            foreach (var id in ids)
            {
                if (!users.TryGetValue(id, out var user))
                {
                    result.Add(id, null);
                    continue;
                }

                result.Add(id, user);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<User?> GetSingle(Guid id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(c => c.Id == id);
            if (user is null)
                return null;

            return user;
        }

        /// <inheritdoc/>
        public async Task<List<User>> Get(int limit)
        {
            return _dbContext.Users.Take(limit).ToList();
        }

        /// <inheritdoc/>
        public async Task<Domain.Models.User?> GetByFMLName(Domain.Models.User user)
        {
            var users = _dbContext.Users.Where(c => c.FirstName.ToLower() == user.FirstName.ToLower() && c.MiddleName.ToLower() == user.MiddleName.ToLower() && c.LastName == user.LastName.ToLower());
            var firstUser = users.FirstOrDefault();
            if (firstUser is null)
                return null;
            return (Courses.Domain.Models.User)firstUser;
        }
        
        /// <inheritdoc/>
        public async Task Add(Domain.Models.User user)
        {
            await AddSingle((User)user);
        }
    }
}