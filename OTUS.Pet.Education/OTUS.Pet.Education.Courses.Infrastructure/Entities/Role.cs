using OTUS.Pet.Education.Courses.Infrastructure.Interfaces;

namespace OTUS.Pet.Education.Courses.Infrastructure.Entities;


/// <summary>
/// Роль пользователя
/// </summary>
public class Role : IEntity, IEntityUpdater<Role>
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Имя роли
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание роли
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <inheritdoc/>
    public void Update(Role arg)
    {
        if (Name != arg.Name)
            Name = arg.Name;

        if (Description != arg.Description)
            Description = arg.Description;
    }
}
