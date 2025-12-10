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
    public class SubjectController : ControllerBase
    {
        private readonly ILogger<SubjectController> _logger;
        private ISubjectLayer _subjectLayer;

        public SubjectController(ILogger<SubjectController> logger, ISubjectLayer subjectLayer)
        {
            _logger = logger;
            _subjectLayer = subjectLayer;
        }

        [HttpPost]
        [Route("AddSubject")]
        public async Task AddSubject(Subject subject)
        {
            await _subjectLayer.AddSingle(new Infrastructure.Entities.Subject
            {
                Name = subject.Name,
            });
        }
    }
}