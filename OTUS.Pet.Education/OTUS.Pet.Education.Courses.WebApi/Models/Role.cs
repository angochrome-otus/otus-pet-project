using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.WebApi.Models
{
    public class Role
    {
        /// <summary>
        /// Имя роли
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание роли
        /// </summary>
        public string Description { get; set; } = string.Empty;

        public static explicit operator Role(Domain.Models.Role role) =>
        new Role
        {
            Name = role.Name,
            Description = role.Description
        };

        public static explicit operator Domain.Models.Role(Role role) =>
        new Domain.Models.Role
        {
            Name = role.Name,
            Description = role.Description
        };
    }
}