using OTUS.Pet.Education.Courses.Infrastructure.Interfaces;

namespace OTUS.Pet.Education.Courses.Infrastructure.Entities;

/// <summary>
/// Пользователь
/// </summary>
public class User : IEntity, IEntityUpdater<User>
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

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

    /// <inheritdoc/>
    public void Update(User arg)
    {
        if (FirstName != arg.FirstName)
            FirstName = arg.FirstName;

        if (LastName != arg.LastName)
            LastName = arg.LastName;

        if (MiddleName != arg.MiddleName)
            MiddleName = arg.MiddleName;

        Roles.Clear();
        Roles.AddRange(arg.Roles);
    }

    public static explicit operator User(Domain.Models.User user) =>
    new User
    {
        Id = user.Id,
        FirstName = user.FirstName,
        MiddleName = user.MiddleName,
        LastName = user.LastName,
        Roles = user.Roles.Select(x => (Role)x).ToList()
    };

    public static explicit operator Domain.Models.User(User user) =>
    new Domain.Models.User
    {
        Id = user.Id,
        FirstName = user.FirstName,
        MiddleName = user.MiddleName,
        LastName = user.LastName,
        Roles = user.Roles.Select(x => (Domain.Models.Role)x).ToList()
    };
}
