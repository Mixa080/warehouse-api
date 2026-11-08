namespace WebApplication1;
using Microsoft.EntityFrameworkCore;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");
        
        group.MapPost("/register", async (RegisterUserDto dto, WarehouseDbContext db) =>
        {
            var emailExist = await db.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExist)
            {
                return Results.Conflict(new { message = "A user with this email address already exists." });
            }

            var passHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                PasswordHash = passHash,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var response = new UserResponseDto(user.Id, user.Email, user.CreatedAt);
            return Results.Created($"/users/{user.Id}", response);
        });
        group.MapPost("/login", async (LoginUserDto dto, WarehouseDbContext db, ITokenService tokenService) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return Results.Unauthorized();
            }

            var jwtString = tokenService.GenerateToken(user);

            return Results.Ok(new AuthResponseDto(jwtString));
        });

        return app;
    }    
}