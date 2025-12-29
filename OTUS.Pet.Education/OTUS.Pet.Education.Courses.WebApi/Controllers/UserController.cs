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
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private RepositoryFactory _repositoryFactory;
        public UserController(ILogger<UserController> logger, RepositoryFactory repositoryFactory)
        {
            _logger = logger;
            _repositoryFactory = repositoryFactory;
        }

        [HttpPost]
        [Route("AddUser")]
        public async Task AddUser(User user)
        {
            var userRepository = _repositoryFactory.CreateRepository<IUserRepository>();
            await userRepository.Add((Domain.Models.User)user);
        }

        [HttpPost]
        [Route("GetUsersByFirstMiddleLastName")]
        public async Task<User?> GetUsersByFMLName(User user)
        {
            var userRepository = _repositoryFactory.CreateRepository<IUserRepository>();
            var resultUser = await userRepository.GetByFMLName((Domain.Models.User)user);
            if (resultUser is null)
                return null;

            return (User)resultUser;
        }
    }
}