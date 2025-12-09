using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.Infrastructure.Interfaces
{
    public interface IEntityUpdater<T>
    {
        /// <summary>
        /// Обновление данных
        /// </summary>
        /// <param name="arg"></param>
        void Update(T arg);
    }
}