using LibreVLE.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LibreVLE.Api.Pages.Admin.Students;

public sealed class EditModel(AppDbContext db) : PageModel
{
    [BindProperty]
    public StudentProfileEditModel Form { get; set; } = new();

    public bool IsEdit { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync([FromQuery] Guid? id)
    {
        if (id.HasValue)
        {
            var student = await db.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id.Value);
            if (student is null) return NotFound();

            IsEdit = true;
            Form = new StudentProfileEditModel
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                EnrollmentDate = student.EnrollmentDate
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        if (Form.Id.HasValue)
        {
            var existing = await db.Students.FirstOrDefaultAsync(s => s.Id == Form.Id.Value);
            if (existing is null) return NotFound();

            existing.FirstName = Form.FirstName;
            existing.LastName = Form.LastName;
            existing.Email = Form.Email;
            existing.EnrollmentDate = Form.EnrollmentDate;

            SuccessMessage = "Student updated successfully.";
        }
        else
        {
            var student = new StudentProfile
            {
                Id = Guid.NewGuid(),
                FirstName = Form.FirstName,
                LastName = Form.LastName,
                Email = Form.Email,
                EnrollmentDate = Form.EnrollmentDate
            };

            db.Students.Add(student);
            SuccessMessage = "Student created successfully.";
            IsEdit = true;
            Form.Id = student.Id;
        }

        await db.SaveChangesAsync();
        return Page();
    }
}
