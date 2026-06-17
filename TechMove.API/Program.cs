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

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddScoped<ICurrencyService, CurrencyService>();
        builder.Services.AddScoped<IContractStatusService, ContractStatusService>();
        builder.Services.AddScoped<IFileValidationService, FileValidationService>();
        builder.Services.AddHttpClient<ICurrencyService, CurrencyService>();

        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();
        builder.Services.AddControllers();

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}