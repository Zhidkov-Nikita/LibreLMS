namespace LibreLMS.Api;

public sealed class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required Role Role { get; set; }
    public required string PasswordHash { get; set; }

    public StudentProfile? StudentProfile { get; set; }
}
