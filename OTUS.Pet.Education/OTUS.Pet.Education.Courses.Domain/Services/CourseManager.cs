using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OTUS.Pet.Education.Courses.Domain.Interfaces;
using OTUS.Pet.Education.Courses.Domain.Interfaces.Repository;
using OTUS.Pet.Education.Courses.Domain.Models;

namespace OTUS.Pet.Education.Courses.Domain.Services
{
    public class CourseManager : IProductProvider
    {
        private readonly ICourseRepository _courseRepository;
        public CourseManager(ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
        }

        /// <inheritdoc/>
        public void ProvideLesson(Guid id, User user)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public void SubscribeCourse(Guid id, User user)
        {
            var course = _courseRepository.GetById(id);
            if (course is null)
            {

            }

        }
    }
}