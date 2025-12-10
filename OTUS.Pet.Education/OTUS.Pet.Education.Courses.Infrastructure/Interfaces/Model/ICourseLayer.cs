using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OTUS.Pet.Education.Courses.Infrastructure.Entities;

namespace OTUS.Pet.Education.Courses.Infrastructure.Interfaces.Model
{
    public interface ICourseLayer : IDataCRUD<Course>
    {
        Task<List<Course>> Get(int limit);
    }
}