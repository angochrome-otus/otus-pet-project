using OTUS.Pet.Education.Courses.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OTUS.Pet.Education.Courses.Infrastructure.Entities;

/// <summary>
/// Описание курса
/// </summary>
public class Subject : IEntity, IEntityUpdater<Subject>
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    /// <summary>
    /// Тематики
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc/>
    public void Update(Subject arg)
    {
        if (Name != arg.Name)
            Name = arg.Name;
    }

    public static explicit operator Subject(Domain.Models.Subject subject) =>
    new Subject
    {
        Id = subject.Id,
        Name = subject.Name,
    };

    public static explicit operator Domain.Models.Subject(Subject subject) =>
    new Domain.Models.Subject
    {
        Id = subject.Id,
        Name = subject.Name,
    };
}
