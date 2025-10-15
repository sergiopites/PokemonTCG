using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;


internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add services to the container.
        builder.Services.AddDbContext<PokemonTCGContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.Run();
    }
}
