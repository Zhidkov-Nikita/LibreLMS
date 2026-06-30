using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LibreStudium.Api.Pages.Admin;

[Authorize(Roles = "Admin")]
public sealed class IndexModel(AppDbContext db) : PageModel
{
    public int TotalUsers { get; set; }
    public int AdminCount { get; set; }
    public int TeacherCount { get; set; }
    public int StudentCount { get; set; }

    public async Task OnGetAsync()
    {
        TotalUsers = await db.Users.CountAsync();
        AdminCount = await db.Users.CountAsync(u => u.Role == Role.Admin);
        TeacherCount = await db.Users.CountAsync(u => u.Role == Role.Teacher);
        StudentCount = await db.Users.CountAsync(u => u.Role == Role.Student);
    }
}
