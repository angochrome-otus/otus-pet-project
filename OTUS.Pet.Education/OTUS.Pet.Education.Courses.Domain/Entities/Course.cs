using OTUS.Pet.Education.Courses.Domain.Common;

namespace OTUS.Pet.Education.Courses.Domain.Entities;

/// <summary>
/// Курс
/// </summary>
public class Course : Entity
{
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
}
