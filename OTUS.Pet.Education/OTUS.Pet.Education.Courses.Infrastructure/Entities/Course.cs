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

    //  Lessons(list)
    public List<User> Students { get; set; } = new List<User>();

    /// <summary>
    /// Описание курса
    /// </summary>
    public Subject Subject { get; set; } = null!;

    /// <inheritdoc/>
    public void Update(Course arg)
    {
        if(Name != arg.Name)
            Name = arg.Name;

        if(StartDate != arg.StartDate)
            StartDate = arg.StartDate;

        if(EndDate != arg.EndDate)
            EndDate = arg.EndDate;

        Students.Clear();
        Students.AddRange(arg.Students);

        if(Subject != arg.Subject)
            Subject = Subject;
    }
}
