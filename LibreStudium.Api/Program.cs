using LibreStudium.Api;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

var envFile = ResolveEnvFilePath();

if (envFile is not null)
{
    DotNetEnv.Env.Load(envFile);
    Console.WriteLine($"[INFO] Loaded .env from {envFile}");
}
else
{
    Console.WriteLine("[WARN] No .env file found. Ensure it exists at the solution root.");
}

var builder = WebApplication.CreateBuilder(args);

var connStr = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__POSTGRES");

if (string.IsNullOrWhiteSpace(connStr))
{
    Console.WriteLine("[ERROR] CONNECTIONSTRINGS__POSTGRES is not set or empty — DbContext will fail.");
}
else
{
    Console.WriteLine("[INFO] PostgreSQL connection string resolved.");
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["ConnectionStrings:Postgres"] = connStr
    });
}

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opts =>
    {
        opts.LoginPath = "/Login";
        opts.AccessDeniedPath = "/Login";
        opts.ExpireTimeSpan = TimeSpan.FromHours(8);
        opts.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(opts =>
{
    opts.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    opts.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

builder.Services.AddRazorPages(opts =>
{
    opts.Conventions.AuthorizeFolder("/Admin", "AdminOnly");
    opts.Conventions.AllowAnonymousToPage("/Login");
});

builder.Services.AddResponseCompression(opts =>
{
    opts.EnableForHttps = true;
    opts.Providers.Add<BrotliCompressionProvider>();
    opts.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddCors(opts =>
{
    opts.AddPolicy("SpaPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetValue<string>("Cors:Origin") ?? "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(connStr ?? builder.Configuration.GetConnectionString("Postgres")));

var app = builder.Build();

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseCors("SpaPolicy");

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any(u => u.Role == Role.Admin))
    {
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@librestudium.com",
            Role = Role.Admin,
            PasswordHash = PasswordHasher.Hash("LibreStudium%")
        });

        db.SaveChanges();
        Console.WriteLine("[INFO] Master admin account seeded.");
    }
}

var students = app.MapGroup("/api/v1/students")
    .RequireAuthorization("AdminOnly")
    .WithTags("Students");

students.MapGet("/", async (AppDbContext db) =>
    await db.Users
        .Where(u => u.Role == Role.Student)
        .Include(u => u.StudentProfile)
        .Select(u => new
        {
            u.Id,
            u.Email,
            FirstName = u.StudentProfile!.FirstName,
            LastName = u.StudentProfile.LastName,
            EnrollmentDate = u.StudentProfile.EnrollmentDate
        })
        .AsNoTracking()
        .ToListAsync())
    .WithName("GetStudents");

app.MapRazorPages();

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();

static string? ResolveEnvFilePath()
{
    var cwd = Directory.GetCurrentDirectory();
    var baseDir = AppContext.BaseDirectory;

    var candidates = new[]
    {
        cwd,
        Path.Combine(cwd, ".."),
        baseDir,
        Path.Combine(baseDir, "..", "..", ".."),
    };

    foreach (var dir in candidates)
    {
        var full = Path.GetFullPath(Path.Combine(dir, ".env"));
        if (File.Exists(full))
            return full;
    }

    return null;
}
