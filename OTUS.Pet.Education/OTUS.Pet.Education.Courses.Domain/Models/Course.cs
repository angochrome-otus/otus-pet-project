using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.Domain.Models
{
    /// <summary>
    /// Курс
    /// </summary>
    public class Course
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Наименование курса
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Дата начала курса
        /// </summary>
        public DateOnly StartDate { get; set; }

        /// <summary>
        /// Дата завершения курса
        /// </summary>
        public DateOnly EndDate { get; set; }

        /// <summary>
        /// Уроки курса
        /// </summary>
        public List<Lesson> Lessons { get; set; } = new List<Lesson>();

        //  Lessons(list)
        public List<User> Students { get; set; } = new List<User>();

        /// <summary>
        /// Описание курса
        /// </summary>
        public Subject Subject { get; set; } = null!;
    }
}