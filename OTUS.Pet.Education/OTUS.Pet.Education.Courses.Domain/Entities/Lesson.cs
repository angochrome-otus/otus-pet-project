using OTUS.Pet.Education.Courses.Domain.Common;

namespace OTUS.Pet.Education.Courses.Domain.Entities;


/// <summary>
/// Урок
/// </summary>
public class Lesson : Entity
{
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

}
