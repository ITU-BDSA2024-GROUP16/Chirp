using Chirp;
using Chirp.Core;
using Chirp.Infrastructure;
using Chirp.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Microsoft.AspNetCore.Identity;



var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CheepDBContext>(options => options.UseSqlite(connectionString));


builder.Services.AddScoped<DBFacade>();
builder.Services.AddScoped<ICheepRepository, CheepRepository>();

builder.Services.AddDefaultIdentity<Author>(options =>
    options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<CheepDBContext>();

// Add services to the container.
builder.Services.AddRazorPages();
var app = builder.Build();

//Here we are seeding the CheepDBConte
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CheepDBContext>();

        context.Database.EnsureCreated();

        // Seed the database using DbInitializer
        DbInitializer.SeedDatabase(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();

//This makes the program public, then the test class can access it
public partial class Program { }
