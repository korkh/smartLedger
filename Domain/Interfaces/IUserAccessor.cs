namespace Domain.Interfaces
{
    public interface IUserAccessor
    {
        string GetUserName();
        bool IsAdmin();
    }
}
