using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OTUS.Pet.Education.Courses.Infrastructure.Entities;

namespace OTUS.Pet.Education.Courses.Infrastructure.Interfaces
{
    public interface IDBContext
    {
        DbSet<Course> Courses { get; }
        DbSet<Lesson> Lessons { get; }
        DbSet<Role> Roles { get; }
        DbSet<Subject> Subjects { get; }
        DbSet<User> Users { get; }

        DbContext _context { get; }
    }
}