using Fasally.Abstractions;
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
    ApplicationDbContext context):IUserService
    {
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<UserResponse>> GetAllsAsync(CancellationToken cancellationToken = default) =>
        await _context.Users
            .Select(u => new UserResponse(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email!,
                u.IsDisabled
            ))
            .ToListAsync(cancellationToken);

    public async Task<Result<UserResponse>> GetAsync(string id)
        {
        if(await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure<UserResponse>(UserErrors.UserNotFound);

        var response = user.Adapt<UserResponse>();
        return Result.Success(response);
        }

    public async Task<Result<UserResponse>> AddAsync(CreateUserRequest request,CancellationToken cancellationToken = default)
        {
        var emailIsExists = await _userManager.Users
            .AnyAsync(u => u.Email == request.Email,cancellationToken);

        if(emailIsExists)
            return Result.Failure<UserResponse>(UserErrors.DuplicatedEmail);

        var user = request.Adapt<ApplicationUser>();
        user.UserName = request.Email;
        var result = await _userManager.CreateAsync(user,request.Password);

        if(result.Succeeded)
            {
            var response = user.Adapt<UserResponse>();
            return Result.Success(response);
            }

        var error = result.Errors.First();
        return Result.Failure<UserResponse>(
            new Error(error.Code,error.Description,StatusCodes.Status400BadRequest));
        }

    public async Task<Result> UpdateAsync(string id,UpdateUserRequest request,CancellationToken cancellationToken = default)
        {
        var emailIsExists = await _userManager.Users
            .AnyAsync(u => u.Email == request.Email && u.Id != id,cancellationToken);

        if(emailIsExists)
            return Result.Failure(UserErrors.DuplicatedEmail);

        if(await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure(UserErrors.UserNotFound);

        user = request.Adapt(user);

        var result = await _userManager.UpdateAsync(user);

        if(result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(
            new Error(error.Code,error.Description,StatusCodes.Status400BadRequest));
        }

    public async Task<Result<UserProfileResponse>> GetProfileAsync(string userId)
        {
        var user = await _userManager.Users
            .Where(u => u.Id == userId)
            .ProjectToType<UserProfileResponse>()
            .SingleAsync();

        return Result.Success(user);
        }

    public async Task<Result> UpdateProfileAsync(string userId,UpdateProfileRequest request)
        {
        await _userManager.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.FirstName,request.FirstName)
                .SetProperty(u => u.LastName,request.LastName));

        return Result.Success();
        }

    public async Task<Result> ChangePasswordAsync(string userId,ChangePasswordRequest request)
        {
        var user = await _userManager.FindByIdAsync(userId);

        var result = await _userManager.ChangePasswordAsync(
            user!,request.CurrentPassword,request.NewPassword);

        if(result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(
            new Error(error.Code,error.Description,StatusCodes.Status400BadRequest));
        }

    public async Task<Result> ToggleStatusAsync(string id)
        {
        if(await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure<UserResponse>(UserErrors.UserNotFound);

        user.IsDisabled = !user.IsDisabled;

        var result = await _userManager.UpdateAsync(user);

        if(result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(
            new Error(error.Code,error.Description,StatusCodes.Status400BadRequest));
        }

    public async Task<Result> UnlockUser(string id)
        {
        if(await _userManager.FindByIdAsync(id) is not { } user)
            return Result.Failure<UserResponse>(UserErrors.UserNotFound);

        var result = await _userManager.SetLockoutEndDateAsync(user,null);

        if(result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();
        return Result.Failure(
            new Error(error.Code,error.Description,StatusCodes.Status400BadRequest));
        }
        //  public async Task<Image> SaveSingleFileAsync(IFormFile file)
        // {
        //     if (file == null || file.Length == 0)
        //         throw new ArgumentException("No file uploaded");

        //     var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
        //     if (!Directory.Exists(uploadsFolder))
        //         Directory.CreateDirectory(uploadsFolder);

        //     var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        //     var filePath = Path.Combine(uploadsFolder, fileName);

        //     using var stream = new FileStream(filePath, FileMode.Create);
        //     await file.CopyToAsync(stream);

        //     var image = new Image
        //     {
        //         FileName = fileName,
        //         OriginalName = file.FileName,
        //         FilePath = "/uploads/" + fileName,
        //         Size = file.Length
        //     };

        //     _context.Images.Add(image);
        //     await _context.SaveChangesAsync();

        //     return image;
        // }

    }
