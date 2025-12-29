using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OTUS.Pet.Education.Courses.Domain.Interfaces;
using OTUS.Pet.Education.Courses.Domain.Interfaces.Repository;
using OTUS.Pet.Education.Courses.Domain.Models;

namespace OTUS.Pet.Education.Courses.Domain.Services
{
    public class CourseManager : IProductProvider
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<CourseManager> _logger;
        public CourseManager(ILogger<CourseManager> logger, ICourseRepository courseRepository)
        {
            _courseRepository = courseRepository;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task ProvideLesson(Guid id, User user)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public async Task SubscribeCourse(Guid id, User user)
        {
            var course = await _courseRepository.GetById(id);
            if (course is null)
            {
                _logger.LogInformation($"Is not found course by Id:\"{id}\"!");
                return;
            }
            await _courseRepository.AddStudent(course, user);
        }
    }
}