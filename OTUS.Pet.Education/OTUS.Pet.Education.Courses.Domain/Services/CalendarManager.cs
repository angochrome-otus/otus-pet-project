using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OTUS.Pet.Education.Courses.Domain.Interfaces;
using OTUS.Pet.Education.Courses.Domain.Interfaces.Repository;

namespace OTUS.Pet.Education.Courses.Domain.Services
{
    public class CalendarManager : ILessonScheduler
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ILogger<CalendarManager> _logger;

        public CalendarManager(ILogger<CalendarManager> logger, ILessonRepository lessonRepository)
        {
            _lessonRepository = lessonRepository;
            _logger = logger;
        }
        /// <inheritdoc/>
        public async Task<Models.Calendar> GetCalendar(DateOnly dateFrom, DateOnly dateTo)
        {
            var lessons = await _lessonRepository.GetLessonsByPeriod(dateFrom, dateTo);
            return new Models.Calendar(lessons);
        }
    }
}