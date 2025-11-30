using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.Domain.Interfaces
{
    /// <summary>
    /// Роль пользователя
    /// </summary>
    public interface IRole
    {
        /// <summary>
        /// Имя роли
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание роли
        /// </summary>
        public string Description { get; set; }
    }
}