using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.Domain.Models
{
    /// <summary>
    /// Урок
    /// </summary>
    public class Lesson
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Описание урока
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Дата и время начала урока
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Дата и время завершения урока
        /// </summary>
        public DateTime EndDate { get; set; }
    }
}