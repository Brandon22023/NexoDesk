namespace Infraestructura.Autenticacion;

public sealed class JwtOptions
{
    public const string SectionName = "Authentication";

    public string JwtSecret { get; set; } = string.Empty;

    public int ExpirationHours { get; set; } = 8;
}
