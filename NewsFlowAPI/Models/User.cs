using Microsoft.AspNetCore.Identity;

namespace NewsFlowAPI.Models
{
    public class User : IdentityUser
    {

        public string FullName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
