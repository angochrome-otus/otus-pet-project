using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.Domain.Models
{
    /// <summary>
    /// Описание курса
    /// </summary>
    public class Subject
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Тематики
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}