using System.ComponentModel.DataAnnotations;

namespace LibreStudium.Api.Models;

public sealed class UserEditModel
{
    public Guid? Id { get; set; }

    [Required, MaxLength(256), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public Role Role { get; set; } = Role.Student;

    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? EnrollmentDate { get; set; }
}
