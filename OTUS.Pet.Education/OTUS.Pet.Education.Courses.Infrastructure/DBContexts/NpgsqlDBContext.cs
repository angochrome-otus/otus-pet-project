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
        }

        public NpgsqlDBContext(ConnectionConfig config, bool IsStarter)
        {
            _config = config;
            Database.EnsureDeleted();
            Task.Delay(1000);
            Database.EnsureCreated();
            Task.Delay(1000);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_config.ConnectionString);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is IEntity && 
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (IEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow; // Set creation date for new entities
                    continue;
                }

                entity.UpdatedAt = DateTime.UtcNow; // Update modified date for all changes
            }
            
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is IEntity && 
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (IEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow; // Set creation date for new entities
                    continue;
                }

                entity.UpdatedAt = DateTime.UtcNow; // Update modified date for all changes
            }

            return base.SaveChanges();
        }
    }
}