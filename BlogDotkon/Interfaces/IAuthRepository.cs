using Microsoft.AspNetCore.Identity;
using BlogDotkon.Models;
using System.Threading.Tasks;

public interface IAuthRepository
{
    Task<User> FindUserByEmailAsync(string email);
    Task<IdentityResult> CreateUserAsync(User user, string password);
    Task<SignInResult> CheckPasswordSignInAsync(User user, string password);
    Task SignInUserAsync(User user, bool isPersistent);
}
