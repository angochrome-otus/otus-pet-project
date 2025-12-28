using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OTUS.Pet.Education.Courses.Domain.Models;

namespace OTUS.Pet.Education.Courses.Domain.Interfaces
{
    public interface IProductProvider
    {
        /// <summary>
        /// Подписать пользователя на курс
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        void SubscribeCourse(Guid id, User user);
        /// <summary>
        /// Открыть урок для пользователя
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        void ProvideLesson(Guid id, User user);
    }
}