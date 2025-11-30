using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.Infrastructure.Interfaces
{
    public interface IEntity
    {
        Guid Id { get; set; }

        DateTime CreatedAt { get; set; }

        DateTime? UpdatedAt { get; set; }

        DateTime? DeletedAt { get; set; }
    }
}