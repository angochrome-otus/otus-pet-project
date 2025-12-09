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
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private IUserLayer _userLayer;

        public UserController(ILogger<UserController> logger, IUserLayer userLayer)
        {
            _logger = logger;
            _userLayer = userLayer;
        }

        [HttpPost]
        [Route("AddUser")]
        public async Task AddUser(User user)
        {
            await _userLayer.AddSingle(new Infrastructure.Entities.User
            {
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                Roles = user.Roles is null ? new List<Infrastructure.Entities.Role>() : user.Roles.Select(e => new Infrastructure.Entities.Role { Name = e.Name, Description = e.Description }).ToList()
            });
        }

        [HttpPost]
        [Route("GetUsersByFirstMiddleLastName")]
        public async Task<User?> GetUsersByFMLName(User user)
        {
            var resultUser = await _userLayer.GetByFMLName(new Infrastructure.Entities.User
            {
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
            });
            if(resultUser is null)
                return null;

            return new User
            {
                FirstName = resultUser.FirstName,
                MiddleName = resultUser.MiddleName,
                LastName = resultUser.LastName,
            };
        }
    }
}