using FreeStyleApp.Application.Interfaces;
using FreeStyleApp.Application.Services;
using FreeStyleApp.Infrastructure;
using FreeStyleApp.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;

// Cấu hình Serilog ban đầu (bootstrap)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Bắt đầu khởi tạo ứng dụng...");

    var builder = WebApplication.CreateBuilder(args);

    // Cấu hình Serilog làm hệ thống ghi log chính
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        )
    );

    // =================================================================
    // === BẮT ĐẦU: CẤU HÌNH DEPENDENCY INJECTION CHO CÁC TẦNG ===
    // =================================================================

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // 1. Đăng ký AppDbContext (lớp triển khai từ Infrastructure)
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));

    // 2. Đăng ký IAppDbContext (interface từ Application) và trỏ nó đến triển khai AppDbContext
    //    Điều này cho phép các service trong tầng Application có thể inject IAppDbContext
    //    mà không cần biết về tầng Infrastructure.
    builder.Services.AddScoped<IAppDbContext>(provider =>
        provider.GetRequiredService<AppDbContext>());

    // 3. Đăng ký các services từ tầng Application
    //    Ví dụ: Đăng ký IUserService và triển khai của nó là UserService
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IAccountService, AccountService>();
    builder.Services.AddScoped<IPermissionService, PermissionService>(); // Thêm dòng này
    builder.Services.AddScoped<IProfileService, ProfileService>();

    // Bạn sẽ thêm các đăng ký service khác ở đây trong tương lai, ví dụ:
    // builder.Services.AddScoped<IPatientService, PatientService>();

    // =================================================================
    // === KẾT THÚC: CẤU HÌNH DEPENDENCY INJECTION ===
    // =================================================================


    // Cấu hình Authentication
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Home/AccessDenied";
        });

    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

    // Configure the HTTP request pipeline.
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

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Ứng dụng không thể khởi động");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;