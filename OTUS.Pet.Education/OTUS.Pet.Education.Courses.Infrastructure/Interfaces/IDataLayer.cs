using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OTUS.Pet.Education.Courses.Infrastructure.Interfaces.Model;

namespace OTUS.Pet.Education.Courses.Infrastructure.Interfaces
{
    public interface IDataLayer
    {
        /// <summary>
        /// Слой взаимодействия с курсами
        /// </summary>
        ICourseLayer CourseLayer { get; }
        /// <summary>
        /// Слой взаимодействия с уроками
        /// </summary>
        ILessonLayer LessonLayer { get; }
        /// <summary>
        /// Слой взаимодействия с ролями
        /// </summary>
        IRoleLayer RoleLayer { get; }
        /// <summary>
        /// Слой взаимодействия с тематикой курсов
        /// </summary>
        ISubjectLayer SubjectLayer { get; }
        /// <summary>
        /// Слой взаимодействия с пользователями
        /// </summary>
        IUserLayer UserLayer { get; }
    }
}