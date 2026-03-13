namespace Domain.Interfaces
{
    public interface IUserAccessor
    {
        string GetUserName();
        bool IsAdmin();
        int GetUserId();
        bool IsSeniorAccountant();
    }
}
