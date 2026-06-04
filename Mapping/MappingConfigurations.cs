using Fasally.Contracts.Authentication;
using Fasally.Contracts.Addresses;
using Fasally.Contracts.Categories;
using Fasally.Contracts.Measurements;
using Fasally.Contracts.Products;
using Fasally.Contracts.Projects;
using Fasally.Contracts.Proposals;
using Fasally.Contracts.Roles;
using Fasally.Contracts.Sellers;
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
            .Ignore(dest => dest.SecurityStamp!);

        config.NewConfig<GoogleJsonWebSignature.Payload, ApplicationUser>()
            .Map(dest => dest.Email,              src => src.Email)
            .Map(dest => dest.UserName,           src => src.Email)
            .Map(dest => dest.FirstName,          src => src.GivenName ?? string.Empty)
            .Map(dest => dest.LastName,           src => src.FamilyName ?? string.Empty)
            .Map(dest => dest.EmailConfirmed,     src => true)
            .Map(dest => dest.ProfileImageUrl,    src => src.Picture)
            .Map(dest => dest.IsProfileCompleted, src => true)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.SecurityStamp!);

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

        // ── Sellers ──────────────────────────────────────────────────────
        config.NewConfig<CreateSellerProfileRequest, SellerProfile>()
            .Ignore(dest => dest.ApplicationUserId)
            .Ignore(dest => dest.Status);

        config.NewConfig<UpdateSellerProfileRequest, SellerProfile>()
            .Ignore(dest => dest.ApplicationUserId)
            .Ignore(dest => dest.Status);

        config.NewConfig<SellerProfile, SellerProfileResponse>()
            .Map(dest => dest.Id, src => src.ApplicationUserId);

        config.NewConfig<SellerProfile, PublicSellerProfileResponse>()
            .Map(dest => dest.Id, src => src.ApplicationUserId);

        // ── Products ─────────────────────────────────────────────────────
        config.NewConfig<CreateProductRequest, Product>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.SellerProfileId);

        config.NewConfig<UpdateProductRequest, Product>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.SellerProfileId);

        config.NewConfig<AddProductImageRequest, ProductImage>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.ProductId);

        config.NewConfig<ProductVariantRequest, ProductVariant>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.ProductId);

        config.NewConfig<Product, ProductResponse>()
            .Map(dest => dest.SellerId,        src => src.SellerProfileId)
            .Map(dest => dest.SellerStoreName, src => src.SellerProfile.StoreName)
            .Map(dest => dest.CategoryName,    src => src.Category == null ? null : src.Category.Name)
            .Map(dest => dest.Images,          src => src.Images.Where(i => !i.IsDeleted).OrderBy(i => i.SortOrder))
            .Map(dest => dest.Variants,        src => src.Variants.Where(v => !v.IsDeleted));

        // ── Categories ───────────────────────────────────────────────────
        config.NewConfig<CategoryRequest, Category>();

        // ── Addresses ────────────────────────────────────────────────────
        config.NewConfig<CreateAddressRequest, Address>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.UserId);

        config.NewConfig<UpdateAddressRequest, Address>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.UserId);

        config.NewConfig<Address, AddressResponse>();

        // ── Measurements ────────────────────────────────────────────────
        config.NewConfig<MeasurementDto, ClientMeasurement>()
            .Ignore(dest => dest.ApplicationUserId)
            .Ignore(dest => dest.UpdatedAt);

        config.NewConfig<ClientMeasurement, MeasurementDto>();

        config.NewConfig<MeasurementDto, ProjectMeasurement>()
            .Ignore(dest => dest.ProjectId);

        config.NewConfig<ProjectMeasurement, MeasurementDto>();

        // ── Proposals ───────────────────────────────────────────────────
        config.NewConfig<CreateProposalRequest, Proposal>()
            .Map(dest => dest.Images, src => src.ImageUrls.Select(imageUrl => new ProposalImage { ImageUrl = imageUrl }))
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.ClientId)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.CreatedAt);

        config.NewConfig<Proposal, ProposalListItemResponse>()
            .Map(dest => dest.ClientName, src => src.Client.FullName)
            .Map(dest => dest.TailorName, src => src.Tailor.User.FullName);

        config.NewConfig<Proposal, ProposalDetailsResponse>()
            .Map(dest => dest.ClientName, src => src.Client.FullName)
            .Map(dest => dest.TailorName, src => src.Tailor.User.FullName);

        config.NewConfig<ProposalImage, ProposalImageResponse>();

        config.NewConfig<ProposalProduct, ProposalProductResponse>()
            .Map(dest => dest.ProductName, src => src.Product.Name)
            .Map(dest => dest.UnitPrice, src => src.UnitPriceAtProposal)
            .Map(dest => dest.TotalPrice, src => src.UnitPriceAtProposal * src.Quantity);

        // ── Projects ────────────────────────────────────────────────────
        config.NewConfig<Project, ProjectListItemResponse>()
            .Map(dest => dest.ClientId, src => src.Proposal.ClientId)
            .Map(dest => dest.ClientName, src => src.Proposal.Client.FullName)
            .Map(dest => dest.TailorId, src => src.Proposal.TailorId)
            .Map(dest => dest.TailorName, src => src.Proposal.Tailor.User.FullName);

        config.NewConfig<Project, ProjectDetailsResponse>()
            .Map(dest => dest.ClientId, src => src.Proposal.ClientId)
            .Map(dest => dest.ClientName, src => src.Proposal.Client.FullName)
            .Map(dest => dest.TailorId, src => src.Proposal.TailorId)
            .Map(dest => dest.TailorName, src => src.Proposal.Tailor.User.FullName)
            .Map(dest => dest.ProposalDescription, src => src.Proposal.Description)
            .Map(dest => dest.TotalPrice, src => src.Proposal.TotalPrice)
            .Map(dest => dest.ProductionDeadline, src => src.Proposal.ProductionDeadline)
            .Map(dest => dest.Products, src => src.Proposal.Products)
            .Map(dest => dest.MeasurementSnapshot, src => src.MeasurementSnapshot);

        // ── Roles ────────────────────────────────────────────────────────
        config.NewConfig<RoleRequest, ApplicationRole>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.IsDefault)
            .Ignore(dest => dest.IsDeleted);
    }
}
