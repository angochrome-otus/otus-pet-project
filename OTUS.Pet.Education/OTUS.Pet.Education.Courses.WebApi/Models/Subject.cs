using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.WebApi.Models
{
    public class Subject
    {
        public string Name { get; set; } = string.Empty;

        public static explicit operator Subject(Domain.Models.Subject subject) =>
        new Subject
        {
            Name = subject.Name,
        };

        public static explicit operator Domain.Models.Subject(Subject subject) =>
        new Domain.Models.Subject
        {
            Name = subject.Name,
        };
    }
}