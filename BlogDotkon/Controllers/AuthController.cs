using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using BlogDotkon.Models;
using BlogDotkon.ViewModels;

public class AuthController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    // Injeção de dependências do UserManager e SignInManager
    public AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // GET: /Auth/Login - Exibe o formulário de login
    [HttpGet("Auth/Login")]
    public IActionResult Login()
    {
        return View(); // Retorna a view de login
    }

    // POST: /Auth/Login - Processa o login do usuário
    [HttpPost("Auth/Login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // Verifica se o modelo de login está válido
        if (!ModelState.IsValid)
        {
            return View(model); // Retorna a view com erros de validação
        }

        // Verifica se o usuário existe no banco de dados
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Usuário não encontrado!"; // Mensagem de erro se o usuário não for encontrado
            ModelState.AddModelError(string.Empty, "Usuário não encontrado.");
            return View(model); // Retorna a view com o erro
        }

        // Verifica se a senha do usuário está correta
        var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!passwordValid)
        {
            TempData["ErrorMessage"] = "Senha incorreta!"; // Mensagem de erro para senha incorreta
            ModelState.AddModelError(string.Empty, "Senha incorreta.");
            return View(model); // Retorna a view com o erro
        }

        // Criação das claims do usuário para a autenticação
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName), // Nome do usuário
            new Claim(ClaimTypes.Email, user.Email),   // E-mail do usuário
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // ID do usuário
        };

        // Criação da identidade do usuário e da principal (identidade completa)
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        // Configuração do cookie de autenticação
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return RedirectToAction("Index", "Post"); // Redireciona para a página inicial após login
    }

    // GET: /Auth/Register - Exibe o formulário de registro de novo usuário
    [HttpGet("Auth/Register")]
    public IActionResult Register()
    {
        return View(); // Retorna a view de registro
    }

    // POST: /Auth/Register - Processa o registro de um novo usuário
    [HttpPost("Auth/Register")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // Verifica se o modelo de registro está válido
        if (!ModelState.IsValid)
        {
            return View(model); // Retorna a view com erros de validação
        }

        // Verifica se o e-mail já está em uso
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            TempData["ErrorMessage"] = "Este e-mail já está em uso!"; 
            ModelState.AddModelError(string.Empty, "Este e-mail já está em uso.");
            return View(model); // Retorna a view com o erro de e-mail já existente
        }

        // Criação do objeto de usuário com base nas informações do modelo
        var user = new User
        {
            UserName = model.Username,  // Nome de usuário
            Email = model.Email         // E-mail do usuário
        };

        // Criação do usuário no banco de dados
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Mensagem de sucesso quando o cadastro é feito corretamente
            TempData["SuccessMessage"] = "Cadastro realizado com sucesso! Faça login para continuar.";

            return RedirectToAction("Login", "Auth"); // Redireciona para a página de login após sucesso
        }

        // Adiciona erros ao modelo de validação se a criação falhar
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description); // Adiciona mensagens de erro
        }

        return View(model); // Retorna a view com os erros de validação
    }

    // POST: /Auth/Logout - Processa o logout do usuário
    [HttpPost("Auth/Logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync(); // Realiza o logout do usuário
        return RedirectToAction("Login", "Auth"); // Redireciona para a página de login após o logout
    }
}
