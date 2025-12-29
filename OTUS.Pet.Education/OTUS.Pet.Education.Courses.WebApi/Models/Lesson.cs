using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.WebApi.Models
{
    public class Lesson
    {
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

        public static explicit operator Lesson(Domain.Models.Lesson lesson) =>
        new Lesson
        {
            Name = lesson.Name,
            Description = lesson.Description,
            StartDate = lesson.StartDate,
            EndDate = lesson.EndDate
        };

        public static explicit operator Domain.Models.Lesson(Lesson lesson) =>
        new Domain.Models.Lesson
        {
            Name = lesson.Name,
            Description = lesson.Description,
            StartDate = lesson.StartDate,
            EndDate = lesson.EndDate
        };
    }
}