using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.Domain.Models
{
    public class Calendar
    {
        private readonly Dictionary<DateOnly, List<Lesson>> _lessons;
        /// <summary>
        /// Уроки по календарю
        /// </summary>
        public Dictionary<DateOnly, List<Lesson>> Lessons => _lessons;

        public Calendar(Dictionary<DateOnly, List<Lesson>> lessons)
        {
            _lessons = lessons;
        }
    }
}