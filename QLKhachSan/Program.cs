using Microsoft.EntityFrameworkCore;
using QLKhachSan.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Add DB Context using SQL Server Connection String
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add Session Support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. Add Controllers with Views (MVC)
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<QLKhachSan.Filters.RoleAuthorizationFilter>();
});

var app = builder.Build();

// Tạo dữ liệu mẫu nếu DB trống
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    DataSeeder.Initialize(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add Session middleware (MUST be before UseAuthorization)
app.UseSession();

app.UseAuthorization();

// 4. Map Default MVC Route - Mặc định vào trang Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
