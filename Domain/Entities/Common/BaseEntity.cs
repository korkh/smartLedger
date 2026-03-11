namespace Domain.Entities.Common;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}

public abstract class BaseEntity : ISoftDelete
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } // Можно заполнять через UserAccessor
    public DateTime? LastModifiedAt { get; set; }

    // Реализация Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
