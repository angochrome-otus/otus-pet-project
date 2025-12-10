using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.WebApi.Models
{
    public class Course
    {
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

        //  Lessons(list)
        public List<User> Students { get; set; } = new List<User>();

        /// <summary>
        /// Описание курса
        /// </summary>
        public Subject Subject { get; set; } = null!;
    }
}