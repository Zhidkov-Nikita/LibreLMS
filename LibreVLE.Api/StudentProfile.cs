namespace LibreVLE.Api;

public sealed class StudentProfile
{
    public required Guid Id { get; init; } = Guid.NewGuid();
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required DateOnly EnrollmentDate { get; init; }
}
