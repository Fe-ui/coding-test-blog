using BlogDotkon.Models;
using BlogDotkon.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

public class AuthService
{
    private readonly IAuthRepository _authRepository;

    public AuthService(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
    {
        // Verifica se o e-mail é válido
        if (string.IsNullOrWhiteSpace(model.Email) || !new EmailAddressAttribute().IsValid(model.Email))
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "InvalidEmail",
                Description = "O e-mail fornecido não é válido."
            });
        }

        // Se o Username não for fornecido, usa a parte do e-mail antes do "@"
        var username = string.IsNullOrWhiteSpace(model.Username)
            ? model.Email.Split('@')[0]  // Parte do e-mail antes do "@"
            : model.Username;

        // Criação do usuário manualmente
        var user = new User
        {
            UserName = username, // Usa o nome de usuário fornecido ou a parte do e-mail
            NormalizedUserName = username.ToUpper(),
            Email = model.Email,
            NormalizedEmail = model.Email.ToUpper()
        };

        // Cria o usuário no repositório de autenticação
        var result = await _authRepository.CreateUserAsync(user, model.Password);

        return result;
    }

    public async Task<SignInResult> LoginAsync(LoginViewModel model)
    {
        var user = await _authRepository.FindUserByEmailAsync(model.Email);
        if (user == null)
        {
            return SignInResult.Failed; // Se o usuário não for encontrado
        }

        // Verifica se a senha está correta
        return await _authRepository.CheckPasswordSignInAsync(user, model.Password);
    }

    public async Task SignInAsync(User user, bool isPersistent)
    {
        await _authRepository.SignInUserAsync(user, isPersistent);
    }

    public async Task SignOutAsync()
    {
        Console.WriteLine($"Usuário deslogado.");
    }

    public async Task<User> FindUserByEmailAsync(string email)
    {
        return await _authRepository.FindUserByEmailAsync(email);
    }
}
