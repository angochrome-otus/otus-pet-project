using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OTUS.Pet.Education.Courses.Domain.Models;

namespace OTUS.Pet.Education.Courses.Domain.Interfaces.Repository
{
    public interface ILessonRepository : IRepository
    {
        Task<Dictionary<DateOnly, List<Lesson>>> GetLessonsByPeriod(DateOnly dateFrom, DateOnly dateTo);
    }
}