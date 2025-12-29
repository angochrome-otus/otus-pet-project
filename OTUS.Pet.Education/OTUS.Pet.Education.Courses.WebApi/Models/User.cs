using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.WebApi.Models
{
    public class User
    {
        /// <summary>
        /// Имя
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Фамилия
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Отчество
        /// </summary>
        public string MiddleName { get; set; } = string.Empty;

        /// <summary>
        /// Роли пользователя
        /// </summary>
        public List<Role> Roles { get; set; } = new List<Role>();

        public static explicit operator User(Domain.Models.User user) =>
        new User
        {
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            LastName = user.LastName,
            Roles = user.Roles.Select(x => (Role)x).ToList()
        };

        public static explicit operator Domain.Models.User(User user) =>
        new Domain.Models.User
        {
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            LastName = user.LastName,
            Roles = user.Roles.Select(x => (Domain.Models.Role)x).ToList()
        };
    }
}