using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LibreLMS.Api.Pages.Admin.Students;

public sealed class IndexModel(AppDbContext db) : PageModel
{
    public List<StudentProfile> Students { get; set; } = [];
    public string? SearchTerm { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    private const int PageSize = 10;

    public async Task OnGetAsync([FromQuery] int page = 1, [FromQuery] string? search = null)
    {
        SearchTerm = search;
        CurrentPage = Math.Max(1, page);

        var query = db.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(s =>
                s.FirstName.ToLower().Contains(term) ||
                s.LastName.ToLower().Contains(term) ||
                s.Email.ToLower().Contains(term));
        }

        var total = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(total / (double)PageSize);
        if (TotalPages < 1) TotalPages = 1;

        Students = await query
            .OrderBy(s => s.LastName).ThenBy(s => s.FirstName)
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }
}
