using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.WebApi.Models
{
    public class Course
    {
        /// <summary>
        /// Наименование курса
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Дата начала курса
        /// </summary>
        public DateOnly StartDate { get; set; }
        
        /// <summary>
        /// Дата завершения курса
        /// </summary>
        public DateOnly EndDate { get; set; }

        /// <summary>
        /// Уроки курса
        /// </summary>
        public List<Lesson> Lessons { get; set; } = new List<Lesson>();

        //  Lessons(list)
        public List<User> Students { get; set; } = new List<User>();

        /// <summary>
        /// Описание курса
        /// </summary>
        public Subject Subject { get; set; } = null!;

        public static explicit operator Course(Domain.Models.Course course) =>
        new Course
        {
            Name = course.Name,
            StartDate = course.StartDate,
            EndDate = course.EndDate,
            Lessons = course.Lessons.Select(x => (Lesson)x).ToList(),
            Students = course.Students.Select(x => (User)x).ToList(),
            Subject = (Subject)course.Subject
        };

        public static explicit operator Domain.Models.Course(Course course) =>
        new Domain.Models.Course
        {
            Name = course.Name,
            StartDate = course.StartDate,
            EndDate = course.EndDate,
            Lessons = course.Lessons.Select(x => (Domain.Models.Lesson)x).ToList(),
            Students = course.Students.Select(x => (Domain.Models.User)x).ToList(),
            Subject = (Domain.Models.Subject)course.Subject
        };
    }
}