using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlogDotkon.Models
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

          

            // Configurações adicionais do Identity (se necessário)
            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("Users"); // Nome da tabela de usuários
                b.Property(u => u.Id).ValueGeneratedOnAdd(); // Configura o ID como autoincremento
            });

            modelBuilder.Entity<IdentityRole<int>>(b =>
            {
                b.ToTable("Roles"); // Nome da tabela de roles
            });

            modelBuilder.Entity<IdentityUserRole<int>>(b =>
            {
                b.ToTable("UserRoles"); // Nome da tabela de relacionamento User-Roles
            });

            modelBuilder.Entity<IdentityUserClaim<int>>(b =>
            {
                b.ToTable("UserClaims"); // Nome da tabela de claims do usuário
            });

            modelBuilder.Entity<IdentityUserLogin<int>>(b =>
            {
                b.ToTable("UserLogins"); // Nome da tabela de logins do usuário
            });

            modelBuilder.Entity<IdentityRoleClaim<int>>(b =>
            {
                b.ToTable("RoleClaims"); // Nome da tabela de claims das roles
            });

            modelBuilder.Entity<IdentityUserToken<int>>(b =>
            {
                b.ToTable("UserTokens"); // Nome da tabela de tokens do usuário
            });
        }
    }
}