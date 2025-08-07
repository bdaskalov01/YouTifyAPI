namespace WebAPIProgram.Controllers;

public interface IUserService
{
    public Task getUserIdTest(string viewedUserId, string viewerId);
}