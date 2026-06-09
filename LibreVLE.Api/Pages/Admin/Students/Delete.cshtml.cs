using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LibreVLE.Api.Pages.Admin.Students;

public sealed class DeleteModel(AppDbContext db) : PageModel
{
    public StudentProfile? Student { get; set; }
    public bool Deleted { get; set; }

    public async Task<IActionResult> OnGetAsync([FromQuery] Guid id)
    {
        Student = await db.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (Student is null) return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync([FromForm] Guid id)
    {
        Student = await db.Students.FirstOrDefaultAsync(s => s.Id == id);
        if (Student is null) return NotFound();

        db.Students.Remove(Student);
        await db.SaveChangesAsync();

        Deleted = true;
        return Page();
    }
}
