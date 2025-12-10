using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.Domain.Interfaces
{
    /// <summary>
    /// Курс
    /// </summary>
    public interface ICourse
    {
        /// <summary>
        /// Наименование курса
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Дата начала курса
        /// </summary>
        public DateOnly StartDate { get; set; }
        
        /// <summary>
        /// Дата завершения курса
        /// </summary>
        public DateOnly EndDate { get; set; }

        //  Lessons(list)
        public List<IUser> Students { get; set; }

        /// <summary>
        /// Описание курса
        /// </summary>
        public ISubject Subject { get; set; }
    }
}