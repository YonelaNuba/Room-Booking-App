
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RoomBookingSystem.Data;
using RoomBookingSystem.Services;
using RoomBookingSystem.Background;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// DbContext - SQL Server
var conn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=.;Database=RoomBookingDB;Trusted_Connection=True;";
builder.Services.AddDbContext<BookingDbContext>(options => options.UseSqlServer(conn));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
}).AddEntityFrameworkStores<BookingDbContext>().AddDefaultTokenProviders();

builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddHostedService<BookingArchiveWorker>();

var app = builder.Build();

// Seed DB & roles on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<BookingDbContext>();
        db.Database.Migrate();
        await DbInitializer.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
