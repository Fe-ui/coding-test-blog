using Microsoft.AspNetCore.Identity;
using BlogDotkon.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class AuthRepository : IAuthRepository
{
    private readonly ApplicationDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthRepository(ApplicationDbContext context)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<User>(); // Instancia o PasswordHasher manualmente
    }

    public async Task<User> FindUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email); // Encontrar usuário pelo e-mail diretamente no DbContext
    }

    public async Task<IdentityResult> CreateUserAsync(User user, string password)
    {
        Console.WriteLine($"Criando usuário: {user.UserName}, Email: {user.Email}");

        // Verifica se o nome de usuário ou o e-mail já estão em uso
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == user.UserName || u.Email == user.Email);

        if (existingUser != null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UserAlreadyExists",
                Description = "O nome de usuário ou e-mail já está em uso."
            });
        }

        // Gerar o hash da senha manualmente
        var passwordHasher = new PasswordHasher<User>(); // Criação do PasswordHasher
        user.PasswordHash = passwordHasher.HashPassword(user, password); // Geração do hash da senha

        try
        {
            // Inserir o usuário no DbContext
            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // Salva o novo usuário no banco de dados

            // Sucesso
            return IdentityResult.Success;
        }
        catch (Exception ex)
        {
            // Em caso de erro ao salvar no banco
            return IdentityResult.Failed(new IdentityError
            {
                Code = "ErrorCreatingUser",
                Description = $"Erro ao criar o usuário: {ex.Message}"
            });
        }
    }


    public async Task<SignInResult> CheckPasswordSignInAsync(User user, string password)
    {
        // Não usamos o SignInManager aqui, pois ele não é mais necessário
        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            return SignInResult.Failed; // Senha incorreta
        }

        return SignInResult.Success; // Senha correta
    }

    public async Task SignInUserAsync(User user, bool isPersistent)
    {   
        Console.WriteLine($"Usuário {user.UserName} logado com persistência {isPersistent}");

    }
}
