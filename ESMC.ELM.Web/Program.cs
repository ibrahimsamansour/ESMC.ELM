using ESMC.ELM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ESMC.ELM.Infrastructure.Identity;
using ESMC.ELM.Infrastructure.Seed;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ESMC.ELM.Web.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddUserStore<
        UserStore<
            ApplicationUser,
            IdentityRole<Guid>,
            ApplicationDbContext,
            Guid>>()
    .AddRoleStore<
        RoleStore<
            IdentityRole<Guid>,
            ApplicationDbContext,
            Guid>>()
    .AddSignInManager<ApplicationSignInManager>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.AddScoped<
    SignInManager<ApplicationUser>,
    ApplicationSignInManager>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedRolesAsync(scope.ServiceProvider);
    await IdentitySeeder.SeedSystemAdminAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();