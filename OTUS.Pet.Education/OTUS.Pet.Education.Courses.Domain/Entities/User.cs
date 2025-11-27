using OTUS.Pet.Education.Courses.Domain.Common;

namespace OTUS.Pet.Education.Courses.Domain.Entities;

/// <summary>
/// Пользователь
/// </summary>
public class User : Entity
{
    /// <summary>
    /// Имя
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Фамилия
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Отчество
    /// </summary>
    public string MiddleName { get; set; } = string.Empty;

    /// <summary>
    /// Роли пользователя
    /// </summary>
    public List<Role> Roles { get; set; } = new List<Role>();
}
