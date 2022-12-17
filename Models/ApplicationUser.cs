using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenDiscussion.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Response>? Responses { get; set; }
        public virtual ICollection<Topic>? Topics { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }
    }
}
