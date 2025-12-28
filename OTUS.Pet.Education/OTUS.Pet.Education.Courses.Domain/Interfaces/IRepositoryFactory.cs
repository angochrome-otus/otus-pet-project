using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.Domain.Interfaces
{
    public interface IRepositoryFactory
    {
        TRepository CreateRepository<TRepository>() where TRepository : notnull, IRepository;
    }
}