using Fasally.Abstractions;
using Fasally.Abstractions.Consts;
using Fasally.Contracts.Users;
using Fasally.Entities;
using Fasally.Errors;
using Fasally.Persistence;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fasally.Services;

public class UserService(
    UserManager<ApplicationUser> userManager,
    IRoleService roleService,
    ApplicationDbContext context) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IRoleService _roleService = roleService;
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await (from u in _context.Users
               join ur in _context.UserRoles on u.Id equals ur.UserId
               join r in _context.Roles on ur.RoleId equals r.Id into roles
               where !roles.Any(x => x.Name == DefaultRoles.Member)
               select new
               {
                   u.Id, u.FirstName, u.LastName, u.Email, u.IsDisabled,
                   Roles = roles.Select(x => x.Name!).ToList()
               })
               .GroupBy(u => new { u.Id, u.FirstName, u.LastName, u.Email, u.IsDisabled })
               .Select(g => new UserResponse(
                   g.Key.Id, g.Key.FirstName, g.Key.LastName,
                   g.Key.Email!, g.Key.IsDisabled,
                   g.SelectMany(x => x.Roles),
                   null))
               .ToListAsync(cancellationToken);

    public async Task<Result<UserResponse>> GetAsync(string id)
    {
        if (await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure<UserResponse>(UserErrors.UserNotFound);

        var roles = await _userManager.GetRolesAsync(user);

        var tailor = await _context.Tailors
            .Where(t => t.ApplicationUserId == id)
            .Select(t => new TailorStatusResponse(t.Status, t.IsVerified, t.ExperienceYears))
            .FirstOrDefaultAsync();

        var response = user.Adapt<UserResponse>() with { Roles = roles, TailorProfile = tailor };

        return Result.Success(response);
    }

    public async Task<Result<UserResponse>> AddAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var emailIsExists = await _userManager.Users
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailIsExists)
            return Result.Failure<UserResponse>(UserErrors.DuplicatedEmail);

        var allowedRoles = await _roleService.GetAllAsync(cancellationToken: cancellationToken);

        if (request.Roles.Except(allowedRoles.Select(r => r.Name)).Any())
            return Result.Failure<UserResponse>(UserErrors.InvalidRoles);

        var user = request.Adapt<ApplicationUser>();

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRolesAsync(user, request.Roles);
            var response = user.Adapt<UserResponse>() with { Roles = request.Roles, TailorProfile = null };
            return Result.Success(response);
        }

        var error = result.Errors.First();
        return Result.Failure<UserResponse>(
            new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> UpdateAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var emailIsExists = await _userManager.Users
            .AnyAsync(u => u.Email == request.Email && u.Id != id, cancellationToken);

        if (emailIsExists)
            return Result.Failure(UserErrors.DuplicatedEmail);

        var allowedRoles = await _roleService.GetAllAsync(cancellationToken: cancellationToken);

        if (request.Roles.Except(allowedRoles.Select(r => r.Name)).Any())
            return Result.Failure(UserErrors.InvalidRoles);

        if (await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);

        user = request.Adapt(user);

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .ExecuteDeleteAsync(cancellationToken);

            await _userManager.AddToRolesAsync(user, request.Roles);

            return Result.Success();
        }

        var error = result.Errors.First();
        return Result.Failure(
            new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result<UserProfileResponse>> GetProfileAsync(string userId)
    {
        var user = await _userManager.Users
            .Where(u => u.Id == userId)
            .ProjectToType<UserProfileResponse>()
            .SingleAsync();

        return Result.Success(user);
    }

    public async Task<Result> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        await _userManager.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.FirstName, request.FirstName)
                .SetProperty(u => u.LastName, request.LastName));

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);

        var result = await _userManager.ChangePasswordAsync(
            user!, request.CurrentPassword, request.NewPassword);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(
            new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> RequestUpgradeAsync(string userId, RequestUpgradeRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        if (user.PendingProfileType is not null)
            return Result.Failure(UserErrors.UpgradeAlreadyRequested);

        user.PendingProfileType = request.ProfileType;
        user.IsProfileCompleted = false;

        await _userManager.UpdateAsync(user);

        return Result.Success();
    }

    public async Task<Result> ToggleStatusAsync(string id)
    {
        if (await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);

        user.IsDisabled = !user.IsDisabled;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(
            new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }

    public async Task<Result> UnlockUser(string id)
    {
        if (await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);

        var result = await _userManager.SetLockoutEndDateAsync(user, null);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(
            new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }
}
