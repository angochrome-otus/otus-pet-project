using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.Domain.Interfaces
{
    public interface ILessonScheduler
    {
        /// <summary>
        /// Получить календарь
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <returns></returns>
        Task<Models.Calendar> GetCalendar(DateOnly dateFrom, DateOnly dateTo);
    }
}