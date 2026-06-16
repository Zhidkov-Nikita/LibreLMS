using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LibreLMS.Api.Pages.Admin.Users;

[Authorize(Roles = "Admin")]
public sealed class DeleteModel(AppDbContext db) : PageModel
{
    public new User? User { get; set; }
    public bool Deleted { get; set; }

    public async Task<IActionResult> OnGetAsync([FromQuery] Guid id)
    {
        User = await db.Users
            .Include(u => u.StudentProfile)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
        if (User is null) return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync([FromForm] Guid id)
    {
        var user = await db.Users
            .Include(u => u.StudentProfile)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        Deleted = true;
        return Page();
    }
}
