using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace WebApplication1.Tests;

public class TokenServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"JwtSettings:SecretKey", "SuperSecretKeyForTestingWarehouseApi123456!"},
            {"JwtSettings:Issuer", "WarehouseApi"},
            {"JwtSettings:Audience", "WarehouseClient"},
            {"JwtSettings:ExpiresInMinutes", "60"}
        };
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
        _tokenService = new TokenService(_configuration);

    }

    [Fact]
    public void GenerateToken_ShouldReturnValidJwtString_WithExpectedClaims()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "testuser@warehouse.com",
            PasswordHash = "fakehash",
            CreatedAt = DateTime.UtcNow

        };
        var tokenstr = _tokenService.GenerateToken(user);
        tokenstr.Should().NotBeNullOrWhiteSpace();
        var handler   = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenstr);
        jwtToken.Issuer.Should().Be("WarehouseApi");
        jwtToken.Audiences.Should().Contain("WarehouseClient");
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
        emailClaim.Should().NotBeNull();
        emailClaim!.Value.Should().Be(user.Email);
        var idClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        idClaim.Should().NotBeNull();
        idClaim!.Value.Should().Be(user.Id.ToString());
    }
}