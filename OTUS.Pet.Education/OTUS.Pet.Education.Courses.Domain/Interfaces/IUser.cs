using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.Domain.Interfaces
{
    /// <summary>
    /// Пользователь
    /// </summary>
    public interface IUser
    {
        /// <summary>
        /// Имя
        /// </summary>
        string FirstName { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        string LastName { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        string MiddleName { get; set; }

        /// <summary>
        /// Роли пользователя
        /// </summary>
        List<IRole> Roles { get; set; }
    }
}