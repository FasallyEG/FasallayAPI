using Fasally.Contracts.Authentication;
using Fasally.Contracts.Users;
using Fasally.Entities;
using Mapster;

namespace Fasally.Mapping;

public class MappingConfigurations : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RegisterRequest, ApplicationUser>()
             .Map(dest => dest.UserName, src => src.Email);

        config.NewConfig<(ApplicationUser user, IEnumerable<string> roles), UserResponse>()
            .Map(dest => dest, src => src.user)
            .Map(dest => dest.Roles, src => src.roles);

        config.NewConfig<CreateUserRequest, ApplicationUser>()
            .Map(dest => dest.UserName, src => src.Email)
            .Map(dest => dest.EmailConfirmed, src => true);

        config.NewConfig<UpdateUserRequest, ApplicationUser>()
            .Map(dest => dest.UserName, src => src.Email)
            .Map(dest => dest.NormalizedEmail, src => src.Email.ToUpper());
    }
}