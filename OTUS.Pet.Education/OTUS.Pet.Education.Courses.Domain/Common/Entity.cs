namespace OTUS.Pet.Education.Courses.Domain.Common;

public abstract class Entity
{
    public long Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}