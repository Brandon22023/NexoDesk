using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Aplicacion.Abstracciones;
using Aplicacion.DTOs.Autenticacion;
using Infraestructura.Persistencia;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infraestructura.Autenticacion;

public sealed class AutenticacionService(
    AppDbContext db,
    IOptions<JwtOptions> options) : IAutenticacionService
{
    private readonly JwtOptions jwtOptions = options.Value;

    public async Task<LoginResponse?> IniciarSesionAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        var usuario = await db.Usuarios
            .Include(item => item.Tenant)
            .SingleOrDefaultAsync(item => item.Email == email, cancellationToken);

        if (usuario is null || !usuario.Activo || !usuario.Tenant.Activo)
        {
            return null;
        }

        var passwordHasher = new PasswordHasher<Dominio.Entidades.Usuario>();
        var passwordResult = passwordHasher.VerifyHashedPassword(
            usuario,
            usuario.PasswordHash,
            request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.ActualizarPasswordHash(
                passwordHasher.HashPassword(usuario, request.Password));
            await db.SaveChangesAsync(cancellationToken);
        }

        var usuarioDto = ToDto(usuario);
        var expiration = TimeSpan.FromHours(jwtOptions.ExpirationHours);
        var token = CreateToken(usuarioDto, expiration);

        return new LoginResponse(
            token,
            (int)expiration.TotalSeconds,
            usuarioDto);
    }

    public async Task<UsuarioSesionDto?> ObtenerUsuarioAsync(
        Guid usuarioId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await db.Usuarios
            .AsNoTracking()
            .Include(item => item.Tenant)
            .SingleOrDefaultAsync(
                item => item.Id == usuarioId && item.TenantId == tenantId,
                cancellationToken);

        return usuario is null || !usuario.Activo || !usuario.Tenant.Activo
            ? null
            : ToDto(usuario);
    }

    private string CreateToken(UsuarioSesionDto usuario, TimeSpan expiration)
    {
        if (string.IsNullOrWhiteSpace(jwtOptions.JwtSecret)
            || jwtOptions.JwtSecret.Length < 32)
        {
            throw new InvalidOperationException(
                "Authentication:JwtSecret debe tener al menos 32 caracteres.");
        }

        var claims = new Claim[]
        {
            new("sub", usuario.Id.ToString()),
            new("tenantId", usuario.TenantId.ToString()),
            new("rol", usuario.Rol),
            new("email", usuario.Email)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtOptions.JwtSecret));
        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.Add(expiration),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UsuarioSesionDto ToDto(Dominio.Entidades.Usuario usuario)
    {
        return new UsuarioSesionDto(
            usuario.Id,
            usuario.Nombre,
            usuario.Email,
            usuario.Rol.ToString(),
            usuario.TenantId,
            usuario.Tenant.Nombre);
    }
}
