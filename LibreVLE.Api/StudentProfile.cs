namespace LibreVLE.Api;

public sealed class StudentProfile
{
    public required Guid Id { get; init; } = Guid.NewGuid();
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required DateOnly EnrollmentDate { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
