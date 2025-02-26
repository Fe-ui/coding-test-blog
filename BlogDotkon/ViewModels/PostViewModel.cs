using BlogDotkon.Models;
using System.ComponentModel.DataAnnotations;

namespace BlogDotkon.ViewModels
{
    public class PostViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Define um valor padrão vazio para Author
        public string Author { get; set; } = string.Empty;

        public bool IsPublished { get; set; } = true;

    }
}
