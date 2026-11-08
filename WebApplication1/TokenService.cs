namespace WebApplication1;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
public interface ITokenService
{
    string GenerateToken(User user);
}

public class TokenService(IConfiguration configuration) : ITokenService
{
    public string GenerateToken(User user)
    {
        var settings = configuration.GetSection("JwtSettings");
        var secretkey = settings["SecretKey"] ?? throw new InvalidOperationException("SecretKey is not configured.");
        var expiryMinutes = double.Parse(settings["ExpiresInMinutes"] ?? throw new InvalidOperationException("ExpiresInMinutes is not configured."));
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretkey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: settings["Issuer"],
            audience: settings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}