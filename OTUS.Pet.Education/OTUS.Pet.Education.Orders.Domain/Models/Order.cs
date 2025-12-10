using OTUS.Pet.Education.Orders.Domain.Common;

namespace OTUS.Pet.Education.Orders.Domain.Models;

public class Order : Entity
{
    /// <summary>
    /// Наименование
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Тело оповещения
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время оповещения
    /// </summary>
    public DateTime NotificationDateTime { get; set; }

    /// <summary>
    /// Время и дата фактической отправки
    /// </summary>
    public DateTime? SentDate { get; set; }
}
