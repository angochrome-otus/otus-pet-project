using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OTUS.Pet.Education.Courses.Domain.Interfaces;

namespace OTUS.Pet.Education.Courses.Infrastructure.Services
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly ILogger<RepositoryFactory> _logger;
        private readonly IServiceProvider _serviceProvider;
        public RepositoryFactory(ILogger<RepositoryFactory> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public TRepository CreateRepository<TRepository>() where TRepository : notnull, IRepository
        {
            return _serviceProvider.GetRequiredService<TRepository>();
        }
    }
}