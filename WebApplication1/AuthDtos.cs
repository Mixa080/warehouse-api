namespace WebApplication1;
public record RegisterUserDto(string Email, string Password);
public record LoginUserDto(string Email, string Password);
public record UserResponseDto(Guid Id, string Email, DateTime CreatedAt);
public record AuthResponseDto(string Token);