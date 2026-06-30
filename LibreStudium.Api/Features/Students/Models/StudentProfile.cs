namespace LibreStudium.Api;

public sealed class StudentProfile
{
    public Guid UserId { get; init; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required DateOnly EnrollmentDate { get; set; }

    public User User { get; set; } = null!;

    public string FullName => $"{FirstName} {LastName}";
}
