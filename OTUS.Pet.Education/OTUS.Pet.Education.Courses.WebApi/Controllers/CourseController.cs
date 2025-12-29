using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OTUS.Pet.Education.Courses.Domain.Interfaces.Repository;
using OTUS.Pet.Education.Courses.Infrastructure.Services;
using OTUS.Pet.Education.Courses.WebApi.Models;

namespace OTUS.Pet.Education.Courses.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ILogger<CourseController> _logger;
        private RepositoryFactory _repositoryFactory;

        public CourseController(ILogger<CourseController> logger, RepositoryFactory repositoryFactory)
        {
            _logger = logger;
            _repositoryFactory = repositoryFactory;
        }

        [HttpPost]
        [Route("AddCourse")]
        public async Task<ActionResult<string>> AddCourse(Course course)
        {
            if (course.Subject is null || string.IsNullOrWhiteSpace(course.Subject.Name))
            {
                return BadRequest("Course subject is incorrected!");
            }
            var subjectRepository = _repositoryFactory.CreateRepository<ISubjectRepository>();

            var subject = await subjectRepository.GetByName(course.Subject.Name);
            if (subject is null)
            {
                return BadRequest("Course subject is not found!");
            }
            var courseRepository = _repositoryFactory.CreateRepository<ICourseRepository>();
            await courseRepository.Add((Domain.Models.Course)course);
            return Ok("OK");
        }

        [HttpPost]
        [Route("GetCourses")]
        public async Task<ActionResult<List<Course>>> GetCourses()
        {
            var courseRepository = _repositoryFactory.CreateRepository<ICourseRepository>();
            var courses = await courseRepository.GetLimit(100);
            return courses.Select(c => new Course { Name = c.Name, StartDate = c.StartDate, EndDate = c.EndDate, Subject = new Subject { Name = c.Subject.Name }}).ToList();
        }
    }
}