using Fasally.Contracts.Authentication;
using Fasally.Contracts.Tailors;
using Fasally.Contracts.Users;
using Fasally.Entities;
using Google.Apis.Auth;
using Mapster;

namespace Fasally.Mapping;

public class MappingConfigurations : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ── Auth ──────────────────────────────────────────────────────────
        config.NewConfig<RegisterRequest, ApplicationUser>()
            .Map(dest => dest.UserName,           src => src.Email)
            .Map(dest => dest.IsProfileCompleted, src => true)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.SecurityStamp);

        config.NewConfig<GoogleJsonWebSignature.Payload, ApplicationUser>()
            .Map(dest => dest.Email,              src => src.Email)
            .Map(dest => dest.UserName,           src => src.Email)
            .Map(dest => dest.FirstName,          src => src.GivenName ?? string.Empty)
            .Map(dest => dest.LastName,           src => src.FamilyName ?? string.Empty)
            .Map(dest => dest.EmailConfirmed,     src => true)
            .Map(dest => dest.ProfileImageUrl,    src => src.Picture)
            .Map(dest => dest.IsProfileCompleted, src => true)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.SecurityStamp);

        // ── Users ─────────────────────────────────────────────────────────
        config.NewConfig<CreateUserRequest, ApplicationUser>()
            .Map(dest => dest.UserName,       src => src.Email)
            .Map(dest => dest.EmailConfirmed, src => true);

        config.NewConfig<UpdateUserRequest, ApplicationUser>()
            .Map(dest => dest.UserName,        src => src.Email)
            .Map(dest => dest.NormalizedEmail, src => src.Email.ToUpper());

        config.NewConfig<ApplicationUser, UserProfileResponse>()
            .Map(dest => dest.NeedsProfileCompletion, src => !src.IsProfileCompleted)
            .Map(dest => dest.PendingProfileType,      src => src.PendingProfileType)
            .Map(dest => dest.TailorProfile, src => src.Tailor == null
                ? null
                : new TailorStatusResponse(
                    src.Tailor.Status,
                    src.Tailor.IsVerified,
                    src.Tailor.ExperienceYears));

        config.NewConfig<ApplicationUser, UserResponse>()
            .Map(dest => dest.TailorProfile, src => src.Tailor == null
                ? null
                : new TailorStatusResponse(
                    src.Tailor.Status,
                    src.Tailor.IsVerified,
                    src.Tailor.ExperienceYears));

        // ── Tailors ───────────────────────────────────────────────────────
        config.NewConfig<CreateTailorRequest, Tailor>()
            .Map(dest => dest.ExperienceYears,    src => src.ExperienceYears)
            .Map(dest => dest.Bio,                src => src.Bio)
            .Map(dest => dest.NationalIdImageUrl, src => src.NationalIdImageUrl)
            .Map(dest => dest.ShopImageUrl,       src => src.ShopImageUrl)
            .Ignore(dest => dest.Categories)
            .Ignore(dest => dest.ApplicationUserId)
            .Ignore(dest => dest.Status);

        config.NewConfig<UpdateTailorRequest, Tailor>()
            .Map(dest => dest.ExperienceYears, src => src.ExperienceYears)
            .Map(dest => dest.Bio,             src => src.Bio)
            .Ignore(dest => dest.Categories)
            .Ignore(dest => dest.ApplicationUserId)
            .Ignore(dest => dest.Status);

        config.NewConfig<CreatePortfolioItemRequest, PortfolioItem>()
            .Map(dest => dest.Title,       src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.ImageUrls,   src => src.ImageUrls)
            .Ignore(dest => dest.TailorId);

        config.NewConfig<Tailor, TailorListItemResponse>()
            .Map(dest => dest.Id,             src => src.ApplicationUserId)
            .Map(dest => dest.FullName,        src => src.User.FullName)
            .Map(dest => dest.ProfileImageUrl, src => src.User.ProfileImageUrl)
            .Map(dest => dest.Categories,      src => src.Categories.Select(c => c.Name));

        config.NewConfig<Tailor, TailorDetailsResponse>()
            .Map(dest => dest.Id,             src => src.ApplicationUserId)
            .Map(dest => dest.FullName,        src => src.User.FullName)
            .Map(dest => dest.ProfileImageUrl, src => src.User.ProfileImageUrl)
            .Map(dest => dest.Categories,      src => src.Categories.Select(c => c.Name))
            .Map(dest => dest.Portfolio,       src => src.PortfolioItems);

        config.NewConfig<PortfolioItem, PortfolioItemResponse>()
            .Map(dest => dest.Id,          src => src.Id)
            .Map(dest => dest.Title,       src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.ImageUrls,   src => src.ImageUrls);
    }
}
