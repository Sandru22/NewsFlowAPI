using Microsoft.AspNetCore.Identity;

namespace NewsFlowAPI.Models
{
    public class User : IdentityUser
    {
        // Poți adăuga proprietăți suplimentare:
        public string FullName { get; set; }
        // Dacă dorești să stochezi CreatedAt, de exemplu:
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
