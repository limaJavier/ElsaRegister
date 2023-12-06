using ElsaRegister.Models;

namespace ElsaRegister.Services;

public interface IUserRepository
{
    public Task InsertUser(User user);
    public Task<User> GetUser(string email);
}