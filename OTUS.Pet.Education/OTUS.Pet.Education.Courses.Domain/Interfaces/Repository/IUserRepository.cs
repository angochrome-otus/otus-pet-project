using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OTUS.Pet.Education.Courses.Domain.Models;

namespace OTUS.Pet.Education.Courses.Domain.Interfaces.Repository
{
    public interface IUserRepository : IRepository
    {
        /// <summary>
        /// Получние пользователся по именам
        /// </summary>
        /// <returns></returns>
        Task<User?> GetByFMLName(User user);
    }
}