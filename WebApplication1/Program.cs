using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<WarehouseDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<ProductValidators>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("SecretKey is not configured.");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- Endpoints: Produkty ---

app.MapGet("/products", async (WarehouseDbContext db) =>
{
    var products = await db.Products.ToListAsync();
    return Results.Ok(products);
});

app.MapGet("/products/{id:guid}", async (Guid id, WarehouseDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

app.MapPost("/products", async (CreateProductDto dto, WarehouseDbContext db, ProductValidators vd) =>
{
    var valid = await vd.ValidateAsync(dto);
    if (!valid.IsValid)
    {
        return Results.ValidationProblem(valid.ToDictionary());
    }

    var newProduct = new Product
    {
        Id = Guid.NewGuid(),
        Name = dto.Name,
        Sku = dto.Sku,
        Price = dto.Price,
        Quantity = dto.QuantityInStock,
        CreatedAt = DateTime.UtcNow
    };

    db.Products.Add(newProduct);
    await db.SaveChangesAsync();
    return Results.Created($"/products/{newProduct.Id}", newProduct);
});

app.MapPut("/products/{id:guid}", async (Guid id, UpdateProductDto dto, WarehouseDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null)
    {
        return Results.NotFound();
    }

    product.Name = dto.Name;
    product.Sku = dto.Sku;
    product.Price = dto.Price;
    product.Quantity = dto.QuantityInStock; 

    await db.SaveChangesAsync();
    return Results.Ok(product);
});

app.MapDelete("/products/{id:guid}", async (Guid id, WarehouseDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null)
    {
        return Results.NotFound();
    }

    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.Ok(product);
});


app.MapPost("/api/auth/register", async (RegisterUserDto dto, WarehouseDbContext db) =>
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

app.MapPost("/api/auth/login", async (LoginUserDto dto, WarehouseDbContext db, IConfiguration configuration) =>
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

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Email, user.Email)
    };

    var settings = configuration.GetSection("JwtSettings");
    var keyString = settings["SecretKey"]!;
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var expiryMinutes = double.Parse(settings["ExpiresInMinutes"]!);

    var token = new JwtSecurityToken(
        issuer: settings["Issuer"],
        audience: settings["Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
        signingCredentials: creds
    );

    var jwtString = new JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(new AuthResponseDto(jwtString));
});

app.Run();