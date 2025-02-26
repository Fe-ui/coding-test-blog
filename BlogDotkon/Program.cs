using BlogDotkon.Interfaces;
using BlogDotkon.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuração de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  // Permite qualquer origem
              .AllowAnyMethod()  // Permite qualquer método (GET, POST, PUT, DELETE, etc.)
              .AllowAnyHeader(); // Permite qualquer cabeçalho
    });
});

// Registra o WebSocketService primeiro, para que outros serviços possam injetá-lo
builder.Services.AddScoped<IWebSocketService, WebSocketService>();

// Configura o DbContext com a connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registra a identidade e o Entity Framework
builder.Services.AddIdentity<User, IdentityRole<int>>() // Usa IdentityRole<int> para compatibilidade com User<int>
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configuração da autenticação com cookies
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Auth/Login"; // Caminho para a página de login
    options.AccessDeniedPath = "/Auth/AccessDenied"; // Caminho para acesso negado
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // Expira em 30 dias
    options.SlidingExpiration = true; // Renova o tempo de expiração a cada requisição
});

// Registra os repositórios e serviços
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Middleware para servir arquivos estáticos (CSS, JS, etc.)
app.UseStaticFiles();

// Habilita WebSockets antes da autenticação para permitir conexões WebSocket não autenticadas
app.UseWebSockets();

// Middleware de CORS (Coloca antes de outros middlewares)
app.UseCors("AllowAll");

// Middleware de roteamento
app.UseRouting();

// Middleware de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

var publicPaths = new[] {
    "/Auth/Login",
    "/Auth/Register",
    "/Post/Details",
    "/Post/Create",
    "/Post/Delete",
    "/index" // Adicionando a URL /index à lista de caminhos públicos
};

// Middleware personalizado para redirecionar usuários não autenticados
app.Use(async (context, next) =>
{
    if (!context.User.Identity.IsAuthenticated)
    {
        var path = context.Request.Path.Value.ToLowerInvariant();
        if (!publicPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            context.Response.Redirect("/Auth/Login");
            return;
        }
    }

    await next();
});

// Mapeia a rota do WebSocket para o serviço WebSocket
app.Map("/api/websocket/connect", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var socket = await context.WebSockets.AcceptWebSocketAsync();
        var webSocketService = context.RequestServices.GetRequiredService<IWebSocketService>();
        await webSocketService.HandleWebSocketAsync(socket);
    }
    else
    {
        context.Response.StatusCode = 400; // Bad Request se não for uma solicitação WebSocket
    }
});

// Redireciona HTTP para HTTPS
app.UseHttpsRedirection();

// Configura as rotas padrão do MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}"); // Rota padrão para /Auth/Login

// Mapeia controladores para APIs
app.MapControllers();

// Redireciona a raiz da aplicação para /Auth/Login
app.MapGet("/", context =>
{
    context.Response.Redirect("/Auth/Login");
    return Task.CompletedTask;
});

// Inicia a aplicação
app.Run();
