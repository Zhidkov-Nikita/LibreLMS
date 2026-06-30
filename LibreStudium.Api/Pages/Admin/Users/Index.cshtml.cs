using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LibreStudium.Api.Pages.Admin.Users;

[Authorize(Roles = "Admin")]
public sealed class IndexModel(AppDbContext db) : PageModel
{
    public List<User> Users { get; set; } = [];
    public string? SearchTerm { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    private const int PageSize = 20;

    public async Task OnGetAsync([FromQuery] int page = 1, [FromQuery] string? search = null)
    {
        SearchTerm = search;
        CurrentPage = Math.Max(1, page);

        var query = db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(u => u.Email.ToLower().Contains(term));
        }

        var total = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(total / (double)PageSize);
        if (TotalPages < 1) TotalPages = 1;

        Users = await query
            .Include(u => u.StudentProfile)
            .OrderBy(u => u.Role).ThenBy(u => u.Email)
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }
}
