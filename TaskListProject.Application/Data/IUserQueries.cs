namespace TaskListProject.Application.Data;

public interface IUserQueries
{
    string? GetUserPassword(string userLogin, string password);
}
