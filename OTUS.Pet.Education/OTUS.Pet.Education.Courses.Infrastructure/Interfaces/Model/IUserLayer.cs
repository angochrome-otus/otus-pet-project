using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OTUS.Pet.Education.Courses.Infrastructure.Entities;

namespace OTUS.Pet.Education.Courses.Infrastructure.Interfaces.Model
{
    public interface IUserLayer : IDataCRUD<User>
    {
        /// <summary>
        /// Получние пользователся по именам
        /// </summary>
        /// <returns></returns>
        Task<User?> GetByFMLName(User user);
    }
}