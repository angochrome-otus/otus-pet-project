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
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    /// <inheritdoc/>
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc/>
    public void Update(Subject arg)
    {
        if (Name != arg.Name)
            Name = arg.Name;
    }
}
