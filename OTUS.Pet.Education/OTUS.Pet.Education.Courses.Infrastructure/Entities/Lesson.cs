using OTUS.Pet.Education.Courses.Infrastructure.Interfaces;

namespace OTUS.Pet.Education.Courses.Infrastructure.Entities;


/// <summary>
/// Урок
/// </summary>
public class Lesson : IEntity, IEntityUpdater<Lesson>
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Наименование
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание урока
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время начала урока
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Дата и время завершения урока
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <inheritdoc/>
    public void Update(Lesson arg)
    {
        if (Name != arg.Name)
            Name = arg.Name;

        if (Description != arg.Description)
            Description = arg.Description;

        if (StartDate != arg.StartDate)
            StartDate = arg.StartDate;

        if (EndDate != arg.EndDate)
            EndDate = arg.EndDate;
    }

    public static explicit operator Lesson(Domain.Models.Lesson lesson) =>
    new Lesson
    {
        Id = lesson.Id,
        Name = lesson.Name,
        Description = lesson.Description,
        StartDate = lesson.StartDate,
        EndDate = lesson.EndDate
    };

    public static explicit operator Domain.Models.Lesson(Lesson lesson) =>
    new Domain.Models.Lesson
    {
        Id = lesson.Id,
        Name = lesson.Name,
        Description = lesson.Description,
        StartDate = lesson.StartDate,
        EndDate = lesson.EndDate
    };
}
