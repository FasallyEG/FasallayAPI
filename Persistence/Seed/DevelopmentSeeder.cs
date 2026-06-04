using Bogus;
using Fasally.Abstractions.Consts;
using Fasally.Entities;
using Fasally.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fasally.Persistence.Seed;

public sealed class DevelopmentSeeder(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ILogger<DevelopmentSeeder> logger)
{
    private const string Password = "Dev@12345";
    private const int ClientCount = 100;
    private const int TailorCount = 30;
    private const int SellerCount = 15;
    private const int ProductCount = 500;
    private const int ProposalCount = 100;
    private const int ProjectCount = 50;

    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly ILogger<DevelopmentSeeder> _logger = logger;
    private readonly Faker _faker = new("en");

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        Randomizer.Seed = new Random(1234);

        await EnsureRolesAsync();

        var categories = await SeedCategoriesAsync(cancellationToken);
        var clients = await SeedClientsAsync(cancellationToken);
        var tailors = await SeedTailorsAsync(categories, cancellationToken);
        var sellers = await SeedSellersAsync(cancellationToken);
        var products = await SeedProductsAsync(sellers, categories, cancellationToken);
        await SeedAddressesAsync(clients, cancellationToken);
        await SeedProposalsAndProjectsAsync(clients, tailors, products, cancellationToken);

        _logger.LogInformation(
            "Development seed complete: {Categories} categories, {Clients} clients, {Tailors} tailors, {Sellers} sellers, {Products} products, {Proposals} proposals, {Projects} projects.",
            categories.Count,
            clients.Count,
            tailors.Count,
            sellers.Count,
            products.Count,
            ProposalCount,
            ProjectCount);
    }

    private async Task EnsureRolesAsync()
    {
        foreach (var roleName in new[] { DefaultRoles.Admin, DefaultRoles.Member, DefaultRoles.Tailor, DefaultRoles.Seller })
        {
            if (await _roleManager.RoleExistsAsync(roleName))
                continue;

            await _roleManager.CreateAsync(new ApplicationRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                IsDefault = roleName == DefaultRoles.Member
            });
        }
    }

    private async Task<List<Category>> SeedCategoriesAsync(CancellationToken cancellationToken)
    {
        string[] names =
        [
            "Men's Suits",
            "Women's Dresses",
            "Abayas",
            "Shirts",
            "Uniforms",
            "Kids Clothing",
            "Wedding Dresses",
            "Evening Dresses",
            "Traditional Wear",
            "Sportswear",
            "Casual Wear",
            "Formal Wear",
            "Outerwear",
            "Alterations",
            "Embroidery",
            "Hijabs",
            "Jackets",
            "Trousers",
            "Skirts",
            "Blouses",
            "Kaftans",
            "School Uniforms",
            "Corporate Uniforms",
            "Costumes"
        ];

        var existingNames = await _context.Categories
            .Where(c => names.Contains(c.Name))
            .Select(c => c.Name)
            .ToListAsync(cancellationToken);

        foreach (var name in names.Except(existingNames))
            _context.Categories.Add(new Category { Name = name });

        await _context.SaveChangesAsync(cancellationToken);

        return await _context.Categories
            .Where(c => names.Contains(c.Name))
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<ApplicationUser>> SeedClientsAsync(CancellationToken cancellationToken)
    {
        var users = new List<ApplicationUser>();

        for (var i = 1; i <= ClientCount; i++)
        {
            var email = DevEmail("client", i);
            var user = await EnsureUserAsync(email, DefaultRoles.Member, i, ProfileType: null);
            users.Add(user);

            var hasMeasurement = await _context.ClientMeasurements
                .AnyAsync(m => m.ApplicationUserId == user.Id, cancellationToken);

            if (!hasMeasurement)
                _context.ClientMeasurements.Add(CreateMeasurement(user.Id, i));
        }

        await _context.SaveChangesAsync(cancellationToken);
        return users;
    }

    private async Task<List<Tailor>> SeedTailorsAsync(
        IReadOnlyList<Category> categories,
        CancellationToken cancellationToken)
    {
        for (var i = 1; i <= TailorCount; i++)
        {
            var email = DevEmail("tailor", i);
            var user = await EnsureUserAsync(email, DefaultRoles.Tailor, i, ProfileType.Tailor);

            var tailor = await _context.Tailors
                .Include(t => t.Categories)
                .Include(t => t.PortfolioItems)
                .FirstOrDefaultAsync(t => t.ApplicationUserId == user.Id, cancellationToken);

            if (tailor is null)
            {
                tailor = new Tailor
                {
                    ApplicationUserId = user.Id,
                    ExperienceYears = _faker.Random.Int(2, 25),
                    Bio = $"{_faker.Company.CompanyName()} specialist in custom tailoring, fittings, and premium finishing.",
                    NationalIdImageUrl = ImageUrl("tailor-national-id", i),
                    ShopImageUrl = ImageUrl("tailor-shop", i),
                    Status = ProfileStatus.Approved,
                    ResponseRate = Math.Round(_faker.Random.Double(82, 99), 1),
                    AverageRating = Math.Round(_faker.Random.Double(3.8, 5.0), 1),
                    TotalReviews = _faker.Random.Int(12, 240)
                };

                foreach (var category in PickCategories(categories, i, 2, 5))
                    tailor.Categories.Add(category);

                _context.Tailors.Add(tailor);
                user.IsProfileCompleted = true;
            }

            var expectedPortfolioCount = 5 + i % 6;
            for (var item = 1; item <= expectedPortfolioCount; item++)
            {
                var title = $"DEV Portfolio {i:000}-{item:00}";
                if (tailor.PortfolioItems.Any(p => p.Title == title))
                    continue;

                tailor.PortfolioItems.Add(new PortfolioItem
                {
                    Title = title,
                    Description = $"Custom {_faker.Commerce.ProductAdjective()} {PortfolioType(item)} with hand-finished details.",
                    ImageUrls =
                    [
                        ImageUrl("portfolio", i * 10 + item),
                        ImageUrl("portfolio-detail", i * 10 + item)
                    ],
                    TailorId = user.Id
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await _context.Tailors
            .Include(t => t.User)
            .Include(t => t.Categories)
            .Where(t => t.User.Email != null && t.User.Email.StartsWith("dev-tailor-"))
            .OrderBy(t => t.User.Email)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<SellerProfile>> SeedSellersAsync(CancellationToken cancellationToken)
    {
        for (var i = 1; i <= SellerCount; i++)
        {
            var email = DevEmail("seller", i);
            var user = await EnsureUserAsync(email, DefaultRoles.Seller, i, ProfileType.Seller);

            var exists = await _context.SellerProfiles
                .AnyAsync(s => s.ApplicationUserId == user.Id, cancellationToken);

            if (exists)
                continue;

            _context.SellerProfiles.Add(new SellerProfile
            {
                ApplicationUserId = user.Id,
                StoreName = $"DEV {FabricStoreName(i)}",
                Description = $"Supplier of {_faker.Commerce.ProductAdjective().ToLowerInvariant()} fabrics, linings, trims, and tailoring essentials.",
                BusinessPhone = $"010{_faker.Random.Number(10000000, 99999999)}",
                BusinessEmail = $"store-{i:000}@dev-fasally.test",
                ShopImageUrl = ImageUrl("fabric-store", i),
                Status = ProfileStatus.Approved,
                AverageRating = Math.Round(_faker.Random.Double(3.7, 5.0), 1),
                TotalReviews = _faker.Random.Int(8, 180)
            });

            user.IsProfileCompleted = true;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await _context.SellerProfiles
            .Include(s => s.User)
            .Where(s => s.User.Email != null && s.User.Email.StartsWith("dev-seller-"))
            .OrderBy(s => s.User.Email)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<Product>> SeedProductsAsync(
        IReadOnlyList<SellerProfile> sellers,
        IReadOnlyList<Category> categories,
        CancellationToken cancellationToken)
    {
        var existingNames = await _context.Products
            .Where(p => p.Name.StartsWith("DEV Fabric "))
            .Select(p => p.Name)
            .ToListAsync(cancellationToken);

        var existingNameSet = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (var i = 1; i <= ProductCount; i++)
        {
            var name = $"DEV Fabric {i:000} - {FabricName(i)}";
            if (existingNameSet.Contains(name))
                continue;

            var product = new Product
            {
                SellerProfileId = sellers[(i - 1) % sellers.Count].ApplicationUserId,
                CategoryId = categories[(i - 1) % categories.Count].Id,
                Name = name,
                Description = $"{FabricName(i)} suitable for {categories[(i - 1) % categories.Count].Name.ToLowerInvariant()}, available by the meter.",
                Price = _faker.Random.Decimal(85m, 950m),
                Stock = _faker.Random.Int(5, 250),
                Status = i % 17 == 0 ? ProductStatus.Inactive : ProductStatus.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 180))
            };

            product.Images.Add(new ProductImage
            {
                ImageUrl = ImageUrl("fabric", i),
                AltText = product.Name,
                SortOrder = 1
            });

            product.Images.Add(new ProductImage
            {
                ImageUrl = ImageUrl("fabric-texture", i),
                AltText = $"{product.Name} texture",
                SortOrder = 2
            });

            product.Variants.Add(new ProductVariant { Type = "Color", Value = FabricColor(i) });
            product.Variants.Add(new ProductVariant { Type = "Material", Value = FabricMaterial(i) });

            product.InventoryLogs.Add(new InventoryLog
            {
                OldStock = 0,
                NewStock = product.Stock,
                ChangeAmount = product.Stock,
                Reason = "Initial development stock"
            });

            _context.Products.Add(product);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await _context.Products
            .Where(p => p.Name.StartsWith("DEV Fabric "))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    private async Task SeedAddressesAsync(
        IReadOnlyList<ApplicationUser> clients,
        CancellationToken cancellationToken)
    {
        string[] cities = ["Cairo", "Giza", "Alexandria", "Mansoura", "Tanta", "Zagazig", "Aswan", "Luxor"];
        string[] areas = ["Nasr City", "Maadi", "Heliopolis", "Dokki", "Smouha", "Garden City", "New Cairo", "Mohandessin"];

        for (var i = 0; i < clients.Count; i++)
        {
            var user = clients[i];
            var targetCount = 1 + i % 3;

            for (var addressIndex = 1; addressIndex <= targetCount; addressIndex++)
            {
                var name = $"DEV Address {addressIndex}";
                var exists = await _context.Addresses
                    .AnyAsync(a => a.UserId == user.Id && a.Name == name, cancellationToken);

                if (exists)
                    continue;

                _context.Addresses.Add(new Address
                {
                    UserId = user.Id,
                    Name = name,
                    City = cities[(i + addressIndex) % cities.Length],
                    Area = areas[(i + addressIndex * 2) % areas.Length],
                    Street = $"{_faker.Address.StreetName()} Street",
                    BuildingNumber = _faker.Random.Int(1, 180).ToString(),
                    Floor = _faker.Random.Int(1, 12).ToString(),
                    ApartmentNumber = _faker.Random.Int(1, 40).ToString(),
                    Notes = addressIndex == 1 ? "Primary development address" : "Secondary delivery address"
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedProposalsAndProjectsAsync(
        IReadOnlyList<ApplicationUser> clients,
        IReadOnlyList<Tailor> tailors,
        IReadOnlyList<Product> products,
        CancellationToken cancellationToken)
    {
        var activeProducts = products.Where(p => p.Status == ProductStatus.Active).ToList();

        for (var i = 1; i <= ProposalCount; i++)
        {
            var description = $"DEV-PROP-{i:000}: {ProposalDescription(i)}";
            var proposal = await _context.Proposals
                .Include(p => p.Images)
                .Include(p => p.Products)
                .Include(p => p.Project)
                .FirstOrDefaultAsync(p => p.Description.StartsWith($"DEV-PROP-{i:000}:"), cancellationToken);

            if (proposal is null)
            {
                var status = ProposalStatusFor(i);
                var selectedProducts = SelectProducts(activeProducts, i, status is ProposalStatus.Pending or ProposalStatus.Rejected ? 0 : _faker.Random.Int(1, 4));

                proposal = new Proposal
                {
                    Id = Guid.NewGuid(),
                    ClientId = clients[(i - 1) % clients.Count].Id,
                    TailorId = tailors[(i - 1) % tailors.Count].ApplicationUserId,
                    Description = description,
                    ResponseDeadline = DateTime.UtcNow.AddDays(_faker.Random.Int(2, 14)),
                    Status = status,
                    CreatedAt = DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 90)),
                    Images =
                    [
                        new ProposalImage { ImageUrl = ImageUrl("proposal-reference", i) },
                        new ProposalImage { ImageUrl = ImageUrl("proposal-inspiration", i) }
                    ]
                };

                if (status is ProposalStatus.AwaitingClientApproval or ProposalStatus.Approved)
                {
                    proposal.ProductionDeadline = DateTime.UtcNow.AddDays(_faker.Random.Int(14, 60));
                    proposal.Products = selectedProducts
                        .Select(product => new ProposalProduct
                        {
                            ProposalId = proposal.Id,
                            ProductId = product.Id,
                            Quantity = _faker.Random.Int(1, 5),
                            UnitPriceAtProposal = product.Price
                        })
                        .ToList();
                    proposal.TotalPrice = proposal.Products.Sum(p => p.UnitPriceAtProposal * p.Quantity) + _faker.Random.Decimal(350m, 2500m);
                }

                _context.Proposals.Add(proposal);
                await _context.SaveChangesAsync(cancellationToken);
            }

            if (i <= ProjectCount)
                await EnsureProjectAsync(proposal, clients[(i - 1) % clients.Count].Id, i, cancellationToken);
        }
    }

    private async Task EnsureProjectAsync(
        Proposal proposal,
        string clientId,
        int index,
        CancellationToken cancellationToken)
    {
        if (proposal.Status != ProposalStatus.Approved)
        {
            var products = await _context.Products
                .Where(p => p.Name.StartsWith("DEV Fabric ") && p.Status == ProductStatus.Active)
                .Take(2)
                .ToListAsync(cancellationToken);

            proposal.Status = ProposalStatus.Approved;
            proposal.ProductionDeadline ??= DateTime.UtcNow.AddDays(30 + index % 30);
            proposal.TotalPrice ??= _faker.Random.Decimal(900m, 6000m);

            if (!proposal.Products.Any())
            {
                foreach (var product in products)
                {
                    proposal.Products.Add(new ProposalProduct
                    {
                        ProposalId = proposal.Id,
                        ProductId = product.Id,
                        Quantity = _faker.Random.Int(1, 4),
                        UnitPriceAtProposal = product.Price
                    });
                }
            }
        }

        var existingProject = await _context.Projects
            .AnyAsync(p => p.ProposalId == proposal.Id, cancellationToken);

        if (existingProject)
        {
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        var status = ProjectStatusFor(index);
        var createdAt = DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 60));
        var startedAt = status is ProjectStatus.InProgress or ProjectStatus.Completed
            ? createdAt.AddDays(_faker.Random.Int(1, 5))
            : (DateTime?)null;

        var completedAt = status == ProjectStatus.Completed
            ? startedAt!.Value.AddDays(_faker.Random.Int(7, 30))
            : (DateTime?)null;

        var measurement = await _context.ClientMeasurements
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ApplicationUserId == clientId, cancellationToken);

        _context.Projects.Add(new Project
        {
            Id = Guid.NewGuid(),
            ProposalId = proposal.Id,
            Status = status,
            CreatedAt = createdAt,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            MeasurementSnapshot = new ProjectMeasurement
            {
                Chest = measurement?.Chest ?? _faker.Random.Double(34, 48),
                Waist = measurement?.Waist ?? _faker.Random.Double(28, 42),
                Hip = measurement?.Hip ?? _faker.Random.Double(34, 50),
                Length = measurement?.Length ?? _faker.Random.Double(55, 78),
                Sleeve = measurement?.Sleeve ?? _faker.Random.Double(50, 68),
                AdditionalMeasurements = measurement?.AdditionalMeasurements ?? "Development project measurement snapshot"
            }
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<ApplicationUser> EnsureUserAsync(
        string email,
        string role,
        int index,
        ProfileType? ProfileType)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            var gender = index % 2 == 0 ? Bogus.DataSets.Name.Gender.Female : Bogus.DataSets.Name.Gender.Male;
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = _faker.Name.FirstName(gender),
                LastName = _faker.Name.LastName(gender),
                ProfileImageUrl = ImageUrl("avatar", index),
                IsProfileCompleted = true,
                PendingProfileType = null
            };

            var result = await _userManager.CreateAsync(user, Password);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Could not create development user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
        else
        {
            user.EmailConfirmed = true;
            user.IsDisabled = false;
            user.PendingProfileType = null;
            user.IsProfileCompleted = true;
            await _userManager.UpdateAsync(user);
        }

        if (!await _userManager.IsInRoleAsync(user, role))
            await _userManager.AddToRoleAsync(user, role);

        return user;
    }

    private ClientMeasurement CreateMeasurement(string userId, int index) =>
        new()
        {
            ApplicationUserId = userId,
            Chest = Math.Round(_faker.Random.Double(34, 48), 1),
            Waist = Math.Round(_faker.Random.Double(28, 42), 1),
            Hip = Math.Round(_faker.Random.Double(34, 50), 1),
            Length = Math.Round(_faker.Random.Double(55, 78), 1),
            Sleeve = Math.Round(_faker.Random.Double(50, 68), 1),
            AdditionalMeasurements = $"DEV measurement profile {index:000}: prefers comfortable fit.",
            UpdatedAt = DateTime.UtcNow.AddDays(-_faker.Random.Int(1, 30))
        };

    private static string DevEmail(string type, int index) =>
        $"dev-{type}-{index:000}@fasally.test";

    private static string ImageUrl(string collection, int index) =>
        $"https://images.unsplash.com/{collection}-{index:000}?auto=format&fit=crop&w=900&q=80";

    private static IReadOnlyList<Category> PickCategories(
        IReadOnlyList<Category> categories,
        int seed,
        int min,
        int max)
    {
        var count = min + seed % (max - min + 1);
        return categories
            .Skip(seed % Math.Max(1, categories.Count - count))
            .Take(count)
            .ToList();
    }

    private static ProposalStatus ProposalStatusFor(int index) =>
        (index % 4) switch
        {
            0 => ProposalStatus.Pending,
            1 => ProposalStatus.Rejected,
            2 => ProposalStatus.AwaitingClientApproval,
            _ => ProposalStatus.Approved
        };

    private static ProjectStatus ProjectStatusFor(int index) =>
        (index % 3) switch
        {
            0 => ProjectStatus.Approved,
            1 => ProjectStatus.InProgress,
            _ => ProjectStatus.Completed
        };

    private static IReadOnlyList<Product> SelectProducts(IReadOnlyList<Product> products, int seed, int count) =>
        products
            .Skip(seed % Math.Max(1, products.Count - count))
            .Take(count)
            .ToList();

    private static string FabricName(int index)
    {
        string[] names =
        [
            "Premium Cotton Fabric",
            "Linen Fabric",
            "Wool Fabric",
            "Silk Fabric",
            "Velvet Fabric",
            "Satin Fabric",
            "Crepe Fabric",
            "Chiffon Fabric",
            "Denim Fabric",
            "Tweed Fabric",
            "Gabardine Fabric",
            "Jersey Knit Fabric"
        ];

        return names[(index - 1) % names.Length];
    }

    private static string FabricMaterial(int index)
    {
        string[] materials = ["Cotton", "Linen", "Wool", "Silk", "Polyester", "Viscose", "Rayon", "Denim", "Crepe"];
        return materials[(index - 1) % materials.Length];
    }

    private static string FabricColor(int index)
    {
        string[] colors = ["Navy", "Ivory", "Black", "Emerald", "Burgundy", "Champagne", "Charcoal", "Rose", "Olive", "Beige"];
        return colors[(index - 1) % colors.Length];
    }

    private static string FabricStoreName(int index)
    {
        string[] names =
        [
            "Thread House",
            "Cairo Fabric Market",
            "Needle & Yard",
            "Silk Road Textiles",
            "Cotton Corner",
            "The Linen Shelf",
            "Velvet Room",
            "Pattern Supply",
            "Golden Spool",
            "Fabric Studio"
        ];

        return names[(index - 1) % names.Length];
    }

    private static string PortfolioType(int index)
    {
        string[] types = ["suit", "dress", "abaya", "uniform", "evening gown", "jacket", "traditional outfit", "shirt"];
        return types[(index - 1) % types.Length];
    }

    private static string ProposalDescription(int index)
    {
        string[] descriptions =
        [
            "Client requested a tailored formal suit with clean lines and premium lining.",
            "Client requested a modest evening dress with soft drape and detailed finishing.",
            "Client requested a custom abaya with embroidery and lightweight fabric.",
            "Client requested uniform pieces with durable fabric and consistent sizing.",
            "Client requested a wedding outfit with structured fitting and fabric consultation."
        ];

        return descriptions[(index - 1) % descriptions.Length];
    }
}
