using System.ComponentModel.DataAnnotations;

namespace BlogDotkon.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public string Author { get; set; } // Nome do autor (não é mais uma chave estrangeira)

        public bool IsPublished { get; set; } = true;
    }
}