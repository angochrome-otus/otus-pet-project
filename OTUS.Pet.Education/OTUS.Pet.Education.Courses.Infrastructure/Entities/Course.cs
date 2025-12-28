using OTUS.Pet.Education.Courses.Infrastructure.Interfaces;

namespace OTUS.Pet.Education.Courses.Infrastructure.Entities;

/// <summary>
/// Курс
/// </summary>
public class Course : IEntity, IEntityUpdater<Course>
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

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

    /// <inheritdoc/>
    public void Update(Course arg)
    {
        if (Name != arg.Name)
            Name = arg.Name;

        if (StartDate != arg.StartDate)
            StartDate = arg.StartDate;

        if (EndDate != arg.EndDate)
            EndDate = arg.EndDate;

        Students.Clear();
        Students.AddRange(arg.Students);

        if (Subject != arg.Subject)
            Subject = Subject;
    }

    public static explicit operator Course(Domain.Models.Course course) =>
    new Course
    {
        Id = course.Id,
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
        Id = course.Id,
        Name = course.Name,
        StartDate = course.StartDate,
        EndDate = course.EndDate,
        Lessons = course.Lessons.Select(x => (Domain.Models.Lesson)x).ToList(),
        Students = course.Students.Select(x => (Domain.Models.User)x).ToList(),
        Subject = (Domain.Models.Subject)course.Subject
    };
}
