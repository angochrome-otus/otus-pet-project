using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OTUS.Pet.Education.Courses.Domain.Models;

namespace OTUS.Pet.Education.Courses.Domain.Interfaces.Repository
{
    public interface ISubjectRepository : IRepository
    {
        /// <summary>
        /// Получение по имени
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<Subject?> GetByName(string name);
        /// <summary>
        /// Добавление
        /// </summary>
        /// <param name="subject"></param>
        /// <returns></returns>
        Task Add(Subject subject);
    }
}