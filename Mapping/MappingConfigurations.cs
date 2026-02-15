using Fasally.Contracts.Authentication;
using Fasally.Contracts.Users;
using Fasally.Entities;
using Mapster;

namespace Fasally.Mapping;

public class MappingConfigurations:IRegister
    {
    public void Register(TypeAdapterConfig config)
        {
        config.NewConfig<RegisterRequest,ApplicationUser>()
             .Map(dest => dest.UserName,src => src.Email);

        config.NewConfig<ApplicationUser,UserResponse>();

        config.NewConfig<CreateUserRequest,ApplicationUser>()
            .Map(dest => dest.UserName,src => src.Email)
            .Map(dest => dest.EmailConfirmed,src => true);

        config.NewConfig<UpdateUserRequest,ApplicationUser>()
            .Map(dest => dest.UserName,src => src.Email)
            .Map(dest => dest.NormalizedEmail,src => src.Email.ToUpper());
        }
    }