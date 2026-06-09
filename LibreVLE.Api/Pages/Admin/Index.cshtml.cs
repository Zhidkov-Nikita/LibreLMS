using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LibreVLE.Api.Pages.Admin;

public sealed class IndexModel(AppDbContext db) : PageModel
{
    public int StudentCount { get; set; }

    public async Task OnGetAsync()
    {
        StudentCount = await db.Students.CountAsync();
    }
}
