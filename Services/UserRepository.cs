using System.Data.Common;
using System.Threading.Channels;
using ElsaRegister.Models;
using MySqlConnector;
namespace ElsaRegister.Services;

public class UserRepository(MySqlDataSource database) : IUserRepository
{
    public async Task<User> GetUser(string email)
    {
        using var connection = await database.OpenConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT * FROM `user` WHERE `email` = @email;";
        command.Parameters.AddWithValue("@email", email);
        var result = await ReadAllAsync(await command.ExecuteReaderAsync());
        
        return result.FirstOrDefault();
    }

    public async Task InsertUser(User user)
    {
        using var connection = await database.OpenConnectionAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO `user` VALUES (@email, @name, @created);";
        BindParams(command, user);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<IReadOnlyList<User>> ReadAllAsync(DbDataReader reader)
    {
        var users = new List<User>();
        using (reader)
        {
            while (await reader.ReadAsync())
            {
                var post = new User
                {
                    Email = reader.GetString(0),
                    Name = reader.GetString(1),
                    Created = reader.GetDateTime(2),
                };
                users.Add(post);
            }
        }
        return users;
    }

    private static void BindParams(MySqlCommand cmd, User user)
    {
        cmd.Parameters.AddWithValue("@email", user.Email);
        cmd.Parameters.AddWithValue("@name", user.Name);
        cmd.Parameters.AddWithValue("@created", user.Created);
    }
}