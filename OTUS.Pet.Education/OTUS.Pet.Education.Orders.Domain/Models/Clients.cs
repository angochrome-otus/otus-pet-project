using OTUS.Pet.Education.Orders.Domain.Common;

namespace OTUS.Pet.Education.Orders.Domain.Models;

public class Clients : Entity
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
}
