using System.ComponentModel.DataAnnotations;

namespace OpenDiscussion.Models
{
    public class Response
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Continutul este obligatoriu")]
        public string? Content { get; set; }

        public DateTime Date { get; set; }

        public string? UserId { get; set; }

        public int? TopicId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Topic Topic { get; set; }
    }
}