using Microsoft.AspNetCore.Identity;

namespace BlogDotkon.Models
{
    public class User : IdentityUser<int> // Usa int como tipo de chave primária
    {
        // Não é necessário redeclarar UserName ou Email, pois já estão em IdentityUser   

        // Relação com Post (opcional, se necessário)
        public virtual ICollection<Post> Posts { get; set; }
    }
}