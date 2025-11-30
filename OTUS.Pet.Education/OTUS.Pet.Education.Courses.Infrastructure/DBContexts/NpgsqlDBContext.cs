using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OTUS.Pet.Education.Courses.Infrastructure.Common;
using OTUS.Pet.Education.Courses.Infrastructure.Entities;
using OTUS.Pet.Education.Courses.Infrastructure.Interfaces;

namespace OTUS.Pet.Education.Courses.Infrastructure.DataLayers
{
    public class NpgsqlDBContext : DbContext, IDBContext
    {
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Lesson> Lessons => Set<Lesson>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Subject> Subjects => Set<Subject>();
        public DbSet<User> Users => Set<User>();

        public DbContext _context => this;

        private readonly ConnectionConfig _config;

        public NpgsqlDBContext(ConnectionConfig config)
        {
            _config = config;
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_config.ConnectionString);
        }
    }
}