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
    public class SubjectController : ControllerBase
    {
        private readonly ILogger<SubjectController> _logger;
        private RepositoryFactory _repositoryFactory;

        public SubjectController(ILogger<SubjectController> logger, RepositoryFactory repositoryFactory)
        {
            _logger = logger;
            _repositoryFactory = repositoryFactory;
        }

        [HttpPost]
        [Route("AddSubject")]
        public async Task AddSubject(Subject subject)
        {
            var layer = _repositoryFactory.CreateRepository<ISubjectRepository>();
            await layer.Add((Domain.Models.Subject)subject);
        }
    }
}