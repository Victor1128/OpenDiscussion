using System.ComponentModel.DataAnnotations;

namespace OpenDiscussion.Models
{
    public class Response
    {
        [Key]
        public int ResponseId { get; set; }

        [Required(ErrorMessage = "Continutul este obligatoriu")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        public int TopicId { get; set; }

        public virtual Topic Topic { get; set; }
    }
}