using System.ComponentModel.DataAnnotations;

namespace ElsaRegister.Models;

public class User
{
    public string Email { get; set; }
    public string Name { get; set; }
    public DateTime Created { get; set; }
}