using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SimpleSocialMedia.Models;
using SimpleSocialMedia.Utilities.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options
                    => options.UseNpgsql(connectionString));

ServiceActivator.Configure(builder.Services.BuildServiceProvider());

builder.Services.AddDefaultIdentity<UserModel>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityCore<UserModel>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<UserModel>>();

        await context.Database.MigrateAsync(); // ���������� ��������
        await context.Seed(userManager);
    }
    catch (Exception ex)
    {
        // ����������� ������ (���������� ����������� �� ������ ����������)
        Console.WriteLine(ex.Message);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
