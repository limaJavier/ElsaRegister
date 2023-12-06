using AutoMapper;
using ElsaRegister.Models;

namespace ElsaRegister.Services;

public class MappingConfig : Profile
{
    public MappingConfig() => CreateMap<UserDTO, User>().AfterMap((dto, user) => user.Created = DateTime.UtcNow);
}