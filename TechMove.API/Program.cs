using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Services;

namespace TechMove.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddScoped<ICurrencyService, CurrencyService>();
        builder.Services.AddScoped<IContractStatusService, ContractStatusService>();
        builder.Services.AddScoped<IFileValidationService, FileValidationService>();
        builder.Services.AddHttpClient<ICurrencyService, CurrencyService>();

        var app = builder.Build();

        // Enable Swagger
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}