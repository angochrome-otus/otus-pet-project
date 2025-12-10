using OTUS.Pet.Education.Notifications.Domain.Common;

namespace OTUS.Pet.Education.Notifications.Domain.Models;

public class Notification : Entity
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

    /// <summary>
    /// Канал отправки
    /// </summary>
    public Channel Channel { get; set; }
}
