using System.Security.Cryptography;
using System.Text;
using Fasally.Abstractions;
using Fasally.Authentication;
using Fasally.Contracts.Authentication;
using Fasally.Entities;
using Fasally.Errors;
using Fasally.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Fasally.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJWTProvider jwtProvider,
    ILogger<AuthService> logger,
    IHttpContextAccessor httpContextAccessor,
    ApplicationDbContext context) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IJWTProvider _jwtProvider = jwtProvider;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ApplicationDbContext _context = context;
    private readonly int _refreshTokenExpiryDays = 14;

    public async Task<Result<AuthResponse>> GetTokenAsync(
         string email,
         string password,
         CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return Result.Failure<AuthResponse>(UserErrors.InvalidCredintials);
        if (user.IsDisabled) return Result.Failure<AuthResponse>(UserErrors.DisabledUser);

        var signInResult = await _signInManager.PasswordSignInAsync(user, password, false, true);

        if (!signInResult.Succeeded)
        {
            var error = signInResult.IsNotAllowed
                ? UserErrors.EmailNotConfirmed
                : signInResult.IsLockedOut
                    ? UserErrors.LockedUser
                    : UserErrors.InvalidCredintials;
            return Result.Failure<AuthResponse>(error);
        }

        var (token, expiresIn) = _jwtProvider.GenerateToken(user);

        var activeRefreshToken = await _context.RefreshTokens
       .Where(t => t.UserId == user.Id && t.IsActive)
       .FirstOrDefaultAsync(cancellationToken);

        string refreshToken;
        DateTime refreshTokenExpiry;

        if (activeRefreshToken != null)
        {
            refreshToken = activeRefreshToken.Token;
            refreshTokenExpiry = activeRefreshToken.ExpiresOn;
        }
        else
        {
            refreshToken =  GenerateRefreshToken();
            refreshTokenExpiry = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresOn = refreshTokenExpiry,
                UserId = user.Id
            });

            await _context.SaveChangesAsync(cancellationToken);
        }
        return Result.Success(new AuthResponse(
            user.Id, user.Email, user.FirstName, user.LastName,
            token, expiresIn, refreshToken, refreshTokenExpiry));
    }

    public async Task<Result<AuthResponse>> GetRefreshTokenAsync(
       string token,
       string refreshToken,
       CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(token);
        if (userId is null) return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return Result.Failure<AuthResponse>(UserErrors.InvalidJwtToken);
        if (user.IsDisabled) return Result.Failure<AuthResponse>(UserErrors.DisabledUser);
        if (user.LockoutEnd > DateTime.UtcNow) return Result.Failure<AuthResponse>(UserErrors.LockedUser);

        var activeRefreshToken = await _context.RefreshTokens
            .Where(rt => rt.Token == refreshToken && rt.UserId == user.Id && rt.RevokedOn == null && rt.ExpiresOn > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeRefreshToken is null)
            return Result.Failure<AuthResponse>(UserErrors.InvalidRefreshToken);

        activeRefreshToken.RevokedOn = DateTime.UtcNow;

        var (newToken, expiresIn) = _jwtProvider.GenerateToken(user);
        var newRefreshToken = GenerateRefreshToken();
        var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            ExpiresOn = newRefreshTokenExpiry,
            UserId = user.Id
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new AuthResponse(
            user.Id, user.Email, user.FirstName, user.LastName,
            newToken, expiresIn, newRefreshToken, newRefreshTokenExpiry));
    }

    public async Task<Result> RevokeRefreshTokenAsync(
        string token,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(token);
        if (userId is null) return Result.Failure(UserErrors.InvalidJwtToken);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return Result.Failure(UserErrors.InvalidJwtToken);

        var activeToken = await _context.RefreshTokens
            .Where(rt => rt.Token == refreshToken && rt.UserId == user.Id && rt.RevokedOn == null && rt.ExpiresOn > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeToken is null) return Result.Failure(UserErrors.InvalidRefreshToken);

        activeToken.RevokedOn = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var emailIsExists = await _userManager.Users.AnyAsync(x => x.Email == request.Email, cancellationToken);

        if (emailIsExists)
            return Result.Failure(UserErrors.DuplicatedEmail);

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure(new Error(
                error.Code,
                error.Description,
                StatusCodes.Status400BadRequest));
        }

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        _logger.LogInformation("Email confirmation code: {Code}", code);

        // TODO: Send confirmation email

        return Result.Success();
    }

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request)
    {
        if (await _userManager.FindByIdAsync(request.UserId) is not { } user)
            return Result.Failure(UserErrors.InvalidCode);

        if (user.EmailConfirmed)
            return Result.Failure(UserErrors.DuplicatedConfirmation);

        var code = request.Code;

        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return Result.Failure(UserErrors.InvalidCode);
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
            return Result.Success();

        var error = result.Errors.First();

        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status400BadRequest));
    }
    public async Task<Result> ResendConfirmationEmailAsync(
        ResendConfirmationEmailRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not { } user)
            return Result.Success();

        if (user.EmailConfirmed)
            return Result.Failure(UserErrors.DuplicatedConfirmation);

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        _logger.LogInformation("Resend confirmation code: {Code}", code);

        // TODO: Send confirmation email

        return Result.Success();
    }

    public async Task<Result> SendResetPasswordCodeAsync(string email)
    {
        if (await _userManager.FindByEmailAsync(email) is not { } user)
            return Result.Success(); // misleading

        if (!user.EmailConfirmed)
            return Result.Failure(UserErrors.EmailNotConfirmed with { StatusCode = StatusCodes.Status400BadRequest });

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        _logger.LogInformation("Reset password code: {Code}", code);

        // TODO: Send reset password email

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !user.EmailConfirmed)
            return Result.Failure(UserErrors.InvalidCode);
        IdentityResult identityResult;

        try
        {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
            identityResult = await _userManager.ResetPasswordAsync(user, code, request.NewPassword);
        }
        catch (FormatException)
        {
            identityResult = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
        }

        if (identityResult.Succeeded)
            return Result.Success();

        var error = identityResult.Errors.First();
        return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status401Unauthorized));
    }

    private static string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}

