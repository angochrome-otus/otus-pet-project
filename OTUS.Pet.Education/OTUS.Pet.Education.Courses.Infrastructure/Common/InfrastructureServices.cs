using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OTUS.Pet.Education.Courses.Infrastructure.DataLayers;
using OTUS.Pet.Education.Courses.Infrastructure.DataLayers.Model;
using OTUS.Pet.Education.Courses.Infrastructure.Interfaces;
using OTUS.Pet.Education.Courses.Infrastructure.Interfaces.Model;

namespace OTUS.Pet.Education.Courses.Infrastructure.Common
{
    public static class InfrastructureServices
    {
        public const string CONNECTION_STRING_ENVIRONMENT_KEY = "CONNECTION_STRING_ENVIRONMENT";
        /// <summary>
        /// Добавление сервисов для работы с слоем БД (PostgreSQL)
        /// </summary>
        /// <param name="services"></param>
        public static void AddInfrastructureServices(this IServiceCollection services)
        {
            var connectionString = Environment.GetEnvironmentVariable(CONNECTION_STRING_ENVIRONMENT_KEY);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new NullReferenceException("CONNECTION_STRING_ENVIRONMENT is null!");
            }
            // connectionString = "Host=localhost;Port=5432;Database=usersdb;Username=postgres;Password=пароль_от_postgres";
            var config = new ConnectionConfig(connectionString);
            try
            {
                using (var context = new NpgsqlDBContext(config, IsStarter: true))
                {
                    using (var subjectLayer = new SubjectLayer(context))
                    {
                        subjectLayer.Get(1).GetAwaiter().GetResult();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error test connect to repository!", ex);
            }
            services.AddSingleton(config);
            services.AddTransient<IDBContext, NpgsqlDBContext>();

            services.AddTransient<ICourseLayer, CourseLayer>();
            services.AddTransient<ILessonLayer, LessonLayer>();
            services.AddTransient<IRoleLayer, RoleLayer>();
            services.AddTransient<ISubjectLayer, SubjectLayer>();
            services.AddTransient<IUserLayer, UserLayer>();

            services.AddTransient<IDataLayer, DataLayer>();
        }
    }
}