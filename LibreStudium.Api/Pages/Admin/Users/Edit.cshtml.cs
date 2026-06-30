using LibreStudium.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LibreStudium.Api.Pages.Admin.Users;

[Authorize(Roles = "Admin")]
public sealed class EditModel(AppDbContext db) : PageModel
{
    [BindProperty]
    public UserEditModel Form { get; set; } = new();

    public bool IsEdit { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync([FromQuery] Guid? id)
    {
        if (id.HasValue)
        {
            var user = await db.Users
                .Include(u => u.StudentProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id.Value);
            if (user is null) return NotFound();

            IsEdit = true;
            Form = new UserEditModel
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                FirstName = user.StudentProfile?.FirstName,
                LastName = user.StudentProfile?.LastName,
                EnrollmentDate = user.StudentProfile?.EnrollmentDate
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        if (!Form.Id.HasValue && string.IsNullOrWhiteSpace(Form.Password))
        {
            ModelState.AddModelError("Form.Password", "Password is required for new users.");
            return Page();
        }

        if (Form.Role == Role.Student)
        {
            if (string.IsNullOrWhiteSpace(Form.FirstName))
                ModelState.AddModelError("Form.FirstName", "First name is required for students.");
            if (string.IsNullOrWhiteSpace(Form.LastName))
                ModelState.AddModelError("Form.LastName", "Last name is required for students.");
            if (!Form.EnrollmentDate.HasValue)
                ModelState.AddModelError("Form.EnrollmentDate", "Enrollment date is required for students.");

            if (!ModelState.IsValid) return Page();
        }

        if (Form.Id.HasValue)
        {
            var existing = await db.Users
                .Include(u => u.StudentProfile)
                .FirstOrDefaultAsync(u => u.Id == Form.Id.Value);
            if (existing is null) return NotFound();

            existing.Email = Form.Email;
            existing.Role = Form.Role;

            if (!string.IsNullOrWhiteSpace(Form.Password))
                existing.PasswordHash = PasswordHasher.Hash(Form.Password);

            if (Form.Role == Role.Student)
            {
                if (existing.StudentProfile is null)
                {
                    existing.StudentProfile = new StudentProfile
                    {
                        UserId = existing.Id,
                        FirstName = Form.FirstName!,
                        LastName = Form.LastName!,
                        EnrollmentDate = Form.EnrollmentDate!.Value
                    };
                }
                else
                {
                    existing.StudentProfile.FirstName = Form.FirstName!;
                    existing.StudentProfile.LastName = Form.LastName!;
                    existing.StudentProfile.EnrollmentDate = Form.EnrollmentDate!.Value;
                }
            }
            else
            {
                if (existing.StudentProfile is not null)
                    db.StudentProfiles.Remove(existing.StudentProfile);
            }

            SuccessMessage = "User updated successfully.";
        }
        else
        {
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = Form.Email,
                Role = Form.Role,
                PasswordHash = PasswordHasher.Hash(Form.Password!)
            };

            if (Form.Role == Role.Student)
            {
                user.StudentProfile = new StudentProfile
                {
                    UserId = userId,
                    FirstName = Form.FirstName!,
                    LastName = Form.LastName!,
                    EnrollmentDate = Form.EnrollmentDate!.Value
                };
            }

            db.Users.Add(user);
            SuccessMessage = "User created successfully.";
            IsEdit = true;
            Form.Id = userId;
        }

        await db.SaveChangesAsync();
        return Page();
    }
}
