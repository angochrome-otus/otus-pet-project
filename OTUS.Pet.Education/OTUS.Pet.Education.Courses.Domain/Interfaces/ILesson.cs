using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.Domain.Interfaces
{
    /// <summary>
    /// Урок
    /// </summary>
    public interface ILesson
    {
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание урока
        /// </summary>
        public string Description { get; set; }

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