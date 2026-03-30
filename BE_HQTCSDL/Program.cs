using BE_HQTCSDL.Database;
using BE_HQTCSDL.Repositories;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BE_HQTCSDL.Repositories.Interfaces;
using BE_HQTCSDL.Services;
using BE_HQTCSDL.Services.Interfaces;
using Microsoft.AspNetCore.Http;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseOracle(BE_HQTCSDL.Config.Environment.OracleConnectionString));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(BE_HQTCSDL.Config.Environment.FrontendOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// JWT Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = BE_HQTCSDL.Config.Environment.JwtIssuer,
            ValidAudience = BE_HQTCSDL.Config.Environment.JwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(BE_HQTCSDL.Config.Environment.JwtSecret))
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ITcgCardRepository, TcgCardRepository>();
builder.Services.AddScoped<ITcgCardService, TcgCardService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITcgSetRepository, TcgSetRepository>();
builder.Services.AddScoped<ITcgSetService, TcgSetService>();
builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserAddressRepository, UserAddressRepository>();
builder.Services.AddScoped<IUserAddressService, UserAddressService>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/v1/health/db", async (ApplicationDbContext db, CancellationToken cancellationToken) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync(cancellationToken);
        if (!canConnect)
        {
            return Results.Problem(
                title: "Database unreachable",
                detail: "Cannot connect to Oracle database.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return Results.Ok(new
        {
            status = "ok",
            database = "reachable",
            timestamp = DateTime.UtcNow
        });
    }
    catch
    {
        return Results.Problem(
            title: "Database health check failed",
            detail: "An exception occurred while checking database connection.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapControllers();
app.Run();