using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FamilyPasswordManager.Pages
{
    [Authorize]
    public class SharedItemsModel : PageModel
    {
        public void OnGet() { }
    }
}
