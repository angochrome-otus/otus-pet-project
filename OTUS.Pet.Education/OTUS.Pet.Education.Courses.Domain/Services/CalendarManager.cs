using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OTUS.Pet.Education.Courses.Domain.Interfaces;
using OTUS.Pet.Education.Courses.Domain.Interfaces.Repository;

namespace OTUS.Pet.Education.Courses.Domain.Services
{
    public class CalendarManager : ILessonScheduler
    {
        private readonly ILessonRepository _lessonRepository;

        public CalendarManager(ILessonRepository lessonRepository)
        {
            _lessonRepository = lessonRepository;
        }
        /// <inheritdoc/>
        public async Task<Models.Calendar> GetCalendar(DateOnly dateFrom, DateOnly dateTo)
        {
            var lessons = await _lessonRepository.GetLessonsByPeriod(dateFrom, dateTo);
            return new Models.Calendar(lessons);
        }
    }
}