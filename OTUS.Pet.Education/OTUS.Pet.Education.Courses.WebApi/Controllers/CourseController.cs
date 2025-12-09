using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OTUS.Pet.Education.Courses.Infrastructure.Interfaces.Model;
using OTUS.Pet.Education.Courses.WebApi.Models;

namespace OTUS.Pet.Education.Courses.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ILogger<CourseController> _logger;
        private ICourseLayer _courseLayer;
        private ISubjectLayer _subjectLayer;

        public CourseController(ILogger<CourseController> logger, ICourseLayer courseLayer, ISubjectLayer subjectLayer)
        {
            _logger = logger;
            _courseLayer = courseLayer;
            _subjectLayer = subjectLayer;
        }

        [HttpPost]
        [Route("AddCourse")]
        public async Task<ActionResult<string>> AddCourse(Course course)
        {
            if (course.Subject is null || string.IsNullOrWhiteSpace(course.Subject.Name))
            {
                return BadRequest("Course subject is incorrected!");
            }

            var subject = await _subjectLayer.GetByName(course.Subject.Name);
            if (subject is null)
            {
                return BadRequest("Course subject is not found!");
            }
            await _courseLayer.AddSingle(new Infrastructure.Entities.Course
            {
                Name = course.Name,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                Subject = new Infrastructure.Entities.Subject { Id = subject.Id }
            });
            return Ok("OK");
        }
    }
}