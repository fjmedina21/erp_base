using System.Globalization;
using System.Security.Claims;
using ErpBase.Api.Data;
using ErpBase.Api.DTOs.Auth;
using ErpBase.Api.Entities;
using ErpBase.Api.Options;
using ErpBase.Api.Services;
using ErpBase.Api.Services.AuthService;
using ErpBase.Api.Services.Email;
using ErpBase.Api.Services.JwtService;
using ErpBase.Api.Services.PasswordReset;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ErpBase.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    AppDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IPasswordResetTokenService passwordResetTokenService,
    IPasswordResetEmailSender passwordResetEmailSender,
    IOptions<PasswordResetOptions> passwordResetOptions,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var exists = await dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
        if (exists)
        {
            return Conflict(new { message = "Ya existe un usuario con este email." });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Activo = true
        };

        dbContext.Users.Add(user);
        dbContext.Auditorias.Add(new Auditoria
        {
            RegistroId = user.Id,
            Accion = "REGISTRO",
            Entidad = "usuarios"
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new AuthResponse(user.Id, user.Email, jwtTokenService.CreateToken(user, [], []), [], []));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                    .ThenInclude(x => x.RolePermissions)
                        .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.Email == email && x.DeletedAt == null, cancellationToken);

        if (user is null || !user.Activo || user.Bloqueado || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            dbContext.Auditorias.Add(new Auditoria
            {
                RegistroId = user?.Id,
                Accion = "LOGIN_FALLIDO",
                Entidad = "usuarios"
            });
            await dbContext.SaveChangesAsync(cancellationToken);
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        var roles = user.UserRoles
            .Where(x => x.Role.DeletedAt == null)
            .Select(x => x.Role.Nombre)
            .Distinct()
            .ToArray();

        var permissions = user.UserRoles
            .SelectMany(x => x.Role.RolePermissions)
            .Where(x => x.Permission.DeletedAt == null)
            .Select(x => x.Permission.Codigo)
            .Distinct()
            .ToArray();

        var token = jwtTokenService.CreateToken(user, roles, permissions);
        dbContext.Auditorias.Add(new Auditoria
        {
            RegistroId = user.Id,
            Accion = "LOGIN_EXITOSO",
            Entidad = "usuarios"
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new AuthResponse(user.Id, user.Email, token, roles, permissions));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(
            x => x.Email == email && x.DeletedAt == null && x.Activo,
            cancellationToken);

        var genericMessage = "Si el correo existe y está activo, se enviará un enlace de recuperación.";
        if (user is null)
        {
            dbContext.Auditorias.Add(new Auditoria
            {
                Accion = "RECUPERACION_PASSWORD_SOLICITADA",
                Entidad = "usuarios"
            });
            await dbContext.SaveChangesAsync(cancellationToken);
            return Ok(new ForgotPasswordResponse(genericMessage));
        }

        var options = passwordResetOptions.Value;
        var resetToken = passwordResetTokenService.GenerateToken();
        var expiresAt = DateTime.Now.AddMinutes(options.ExpirationMinutes);

        dbContext.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = passwordResetTokenService.HashToken(resetToken),
            ExpiresAt = expiresAt
        });
        dbContext.Auditorias.Add(new Auditoria
        {
            RegistroId = user.Id,
            Accion = "RECUPERACION_PASSWORD_SOLICITADA",
            Entidad = "usuarios"
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        var resetUrlBase = $"{Request.Scheme}://{Request.Host}/reset-password";
        var resetUrl = $"{resetUrlBase}?token={Uri.EscapeDataString(resetToken)}";
        await passwordResetEmailSender.SendPasswordResetAsync(user.Email, resetUrl, expiresAt, cancellationToken);

        if (environment.IsDevelopment())
        {
            return Ok(new ForgotPasswordResponse(genericMessage, resetToken, resetUrl, FormatLocalDateTime(expiresAt)));
        }

        return Ok(new ForgotPasswordResponse(genericMessage));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromQuery] string token, ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new { message = "El token de recuperación es requerido." });
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            return BadRequest(new { message = "La confirmación de contraseña no coincide." });
        }

        var tokenHash = passwordResetTokenService.HashToken(token);
        var resetToken = await dbContext.PasswordResetTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (resetToken is null ||
            resetToken.UsedAt is not null ||
            resetToken.ExpiresAt < DateTime.Now ||
            resetToken.User.DeletedAt is not null ||
            !resetToken.User.Activo ||
            resetToken.User.Bloqueado)
        {
            return BadRequest(new { message = "El token de recuperación es inválido o expiró." });
        }

        resetToken.User.PasswordHash = passwordHasher.Hash(request.NewPassword);
        resetToken.User.UpdatedAt = DateTime.Now;
        resetToken.UsedAt = DateTime.Now;
        dbContext.Auditorias.Add(new Auditoria
        {
            RegistroId = resetToken.UserId,
            Accion = "PASSWORD_RESTABLECIDO",
            Entidad = "usuarios"
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponse>> Me(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out var id))
        {
            return Unauthorized();
        }

        var user = await dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                    .ThenInclude(x => x.RolePermissions)
                        .ThenInclude(x => x.Permission)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        var roles = user.UserRoles.Select(x => x.Role.Nombre).Distinct().ToArray();
        var permissions = user.UserRoles
            .SelectMany(x => x.Role.RolePermissions)
            .Select(x => x.Permission.Codigo)
            .Distinct()
            .ToArray();

        return Ok(new UserProfileResponse(user.Id, user.Email, user.Activo, user.Bloqueado, roles, permissions));
    }

    private static string FormatLocalDateTime(DateTime value) =>
        value.ToLocalTime().ToString("dd/MM/yyyy hh:mm tt", CultureInfo.GetCultureInfo("es-DO"));
}

public sealed record UserProfileResponse(
    Guid Id,
    string Email,
    bool Activo,
    bool Bloqueado,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
