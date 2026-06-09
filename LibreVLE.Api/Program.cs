using LibreVLE.Api;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

// ── .env loading (project dir → solution root → bin dir) ────────────────────
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

// ── Connection string from .env ──────────────────────────────────────────────
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

// ── Razor Pages ───────────────────────────────────────────────────────────────
builder.Services.AddRazorPages();

// ── Compression ──────────────────────────────────────────────────────────────
builder.Services.AddResponseCompression(opts =>
{
    opts.EnableForHttps = true;
    opts.Providers.Add<BrotliCompressionProvider>();
    opts.Providers.Add<GzipCompressionProvider>();
});

// ── CORS ─────────────────────────────────────────────────────────────────────
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

// ── Database (PostgreSQL) ────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(connStr ?? builder.Configuration.GetConnectionString("Postgres")));

var app = builder.Build();

// ── Middleware ───────────────────────────────────────────────────────────────
app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseCors("SpaPolicy");

// ── Seed ─────────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Students.Any())
    {
        db.Students.AddRange(
            new StudentProfile
            {
                Id = Guid.NewGuid(),
                FirstName = "Alice",
                LastName = "Johnson",
                Email = "alice@example.com",
                EnrollmentDate = DateOnly.FromDateTime(DateTime.UtcNow)
            },
            new StudentProfile
            {
                Id = Guid.NewGuid(),
                FirstName = "Bob",
                LastName = "Smith",
                Email = "bob@example.com",
                EnrollmentDate = DateOnly.FromDateTime(DateTime.UtcNow)
            });
        db.SaveChanges();
    }
}

// ── Endpoints ────────────────────────────────────────────────────────────────
var students = app.MapGroup("/api/v1/students").WithTags("Students");

students.MapGet("/", async (AppDbContext db) =>
    await db.Students.AsNoTracking().ToListAsync())
    .WithName("GetStudents");

// ── Admin Razor Pages ─────────────────────────────────────────────────────────
app.MapRazorPages();

// ── SPA static files ─────────────────────────────────────────────────────────
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();

// ── Local helper ─────────────────────────────────────────────────────────────
static string? ResolveEnvFilePath()
{
    var cwd = Directory.GetCurrentDirectory();
    var baseDir = AppContext.BaseDirectory;

    var candidates = new[]
    {
        cwd,                                          // project dir (dotnet run --project)
        Path.Combine(cwd, ".."),                      // solution root (one level up)
        baseDir,                                      // bin/Debug/net10.0
        Path.Combine(baseDir, "..", "..", ".."),      // project dir from bin
    };

    foreach (var dir in candidates)
    {
        var full = Path.GetFullPath(Path.Combine(dir, ".env"));
        if (File.Exists(full))
            return full;
    }

    return null;
}
