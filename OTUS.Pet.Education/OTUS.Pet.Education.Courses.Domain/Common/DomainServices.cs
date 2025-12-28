using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OTUS.Pet.Education.Courses.Domain.Interfaces;
using OTUS.Pet.Education.Courses.Domain.Services;

namespace OTUS.Pet.Education.Courses.Domain.Common
{
    public static class DomainServices
    {
        public static void AddDomainServices(this IServiceCollection services)
        {
            services.AddScoped<ILessonScheduler, CalendarManager>();
            services.AddScoped<IProductProvider, CourseManager>();
        }
    }
}