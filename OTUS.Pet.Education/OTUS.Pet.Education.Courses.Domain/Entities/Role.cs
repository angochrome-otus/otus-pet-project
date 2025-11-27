using OTUS.Pet.Education.Courses.Domain.Common;

namespace OTUS.Pet.Education.Courses.Domain.Entities;


/// <summary>
/// Роль пользователя
/// </summary>
public class Role : Entity
{
    /// <summary>
    /// Имя роли
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание роли
    /// </summary>
    public string Description { get; set; } = string.Empty;

}
