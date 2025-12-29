using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OTUS.Pet.Education.Courses.Domain.Models;

namespace OTUS.Pet.Education.Courses.Domain.Interfaces.Repository
{
    public interface ICourseRepository : IRepository
    {
        /// <summary>
        /// Добавить
        /// </summary>
        /// <returns></returns>
        Task Add(Course course);
        /// <summary>
        /// Получить количество
        /// </summary>
        /// <returns></returns>
        Task<List<Course>> GetLimit(int limit);
        /// <summary>
        /// Получить Id
        /// </summary>
        /// <returns></returns>
        Task<Course?> GetById(Guid id);
        /// <summary>
        /// Добавление студента
        /// </summary>
        /// <param name="course"></param>
        /// <param name="student"></param>
        /// <returns></returns>
        Task AddStudent(Course course, User student);
    }
}