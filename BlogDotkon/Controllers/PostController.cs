using BlogDotkon.Interfaces;
using BlogDotkon.Models;
using BlogDotkon.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlogDotkon.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostRepository _postRepository; // Interface para o repositório de posts
        private readonly ILogger<PostController> _logger; // Injeção do ILogger para registrar logs
        private readonly IWebSocketService _webSocketService; // Serviço para comunicação via WebSocket

        // Construtor do controlador, injetando dependências de repositório, logger e WebSocket
        public PostController(IPostRepository postRepository, ILogger<PostController> logger, IWebSocketService webSocketService)
        {
            _postRepository = postRepository;
            _logger = logger;
            _webSocketService = webSocketService;
        }

        // GET: / - Exibe todos os posts
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            // Obtém todos os posts do repositório
            var posts = await _postRepository.GetAllAsync();

            // Mapeia os posts para o ViewModel para exibição
            var postListViewModel = new PostListViewModel
            {
                Posts = posts.Select(p => new PostViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    Author = !string.IsNullOrEmpty(p.Author) ? p.Author : "Anônimo",
                    CreatedAt = p.CreatedAt
                }).ToList()
            };

            return View(postListViewModel); // Retorna a view com a lista de posts
        }

        // GET: /Create - Exibe o formulário para criar um novo post
        [Authorize] // Apenas usuários autenticados podem acessar
        [HttpGet("Create")]
        public IActionResult Create()
        {
            // Verifica se o usuário está autenticado antes de permitir a criação de post
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }

            return View(new PostViewModel()); // Retorna a view de criação com um ViewModel vazio
        }

        // POST: /Create - Processa a criação de um novo post
        [HttpPost("Create")]
        public async Task<IActionResult> Create(PostViewModel model)
        {
            // Verifica se o usuário está autenticado
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized(); // Retorna erro 401 se o usuário não estiver autenticado
            }

            if (!ModelState.IsValid)
            {
                // Se o modelo de dados não for válido, registra os erros e retorna à view
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.LogError("Erros de validação ao criar post: {Errors}", string.Join(", ", errors));

                return View(model); // Retorna a view com erros de validação
            }

            try
            {
                // Obtém o ID do usuário logado
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                // Criação de um novo post a partir dos dados do ViewModel
                var post = new Post
                {
                    Title = model.Title,
                    Content = model.Content,
                    Author = User.Identity.Name, // Autor do post é o nome do usuário logado
                    CreatedAt = DateTime.Now,
                    IsPublished = model.IsPublished
                };

                // Adiciona o post ao repositório
                await _postRepository.AddAsync(post);

                // Envia uma mensagem via WebSocket para todos os clientes conectados
                var message = "Post criado com sucesso!";
                await _webSocketService.SendMessageToClients(message); // Notifica todos os clientes via WebSocket

                // Adiciona uma mensagem de sucesso na sessão temporária e redireciona para a página inicial
                TempData["SuccessMessage"] = "Post criado com sucesso!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Em caso de erro, retorna uma mensagem de erro
                TempData["SuccessMessage"] = "Não foi possível salvar o post!";
                ModelState.AddModelError(string.Empty, "Erro ao salvar o post: " + ex.Message);
                return View(model);
            }
        }

        // POST: /Delete/{id} - Exclui uma postagem
        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Obtém o post pelo ID
            var post = await _postRepository.GetByIdAsync(id);

            // Verifica se o post existe e se o autor é o usuário logado
            if (post == null || post.Author != User.Identity.Name)
            {
                return Unauthorized(); // Retorna erro 401 se o post não for encontrado ou se o usuário não for o autor
            }

            // Exclui o post do repositório
            await _postRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = "Postagem excluída com sucesso.";
            return RedirectToAction("Index"); // Redireciona para a página inicial
        }

        // GET: /Edit/{id} - Exibe o formulário de edição de um post
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            // Obtém o post pelo ID
            var post = await _postRepository.GetByIdAsync(id);

            // Verifica se o post existe
            if (post == null)
            {
                return NotFound(); // Retorna erro 404 se o post não for encontrado
            }

            // Mapeia o post para o ViewModel para edição
            var viewModel = new PostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content
            };

            ModelState.Clear(); // Limpa o estado do modelo
            return View(viewModel); // Retorna a view de edição com os dados do post
        }

        // POST: /Edit/{id} - Processa a atualização de um post
        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, PostViewModel model)
        {
            // Verifica se o modelo de dados é válido
            if (!ModelState.IsValid)
            {
                return View(model); // Retorna a view com erros de validação
            }

            // Obtém o post pelo ID
            var post = await _postRepository.GetByIdAsync(model.Id);

            // Verifica se o post existe
            if (post == null)
            {
                return NotFound(); // Retorna erro 404 se o post não for encontrado
            }

            // Atualiza as propriedades do post com os dados fornecidos
            post.Title = model.Title;
            post.Content = model.Content;
            post.UpdatedAt = DateTime.Now;

            // Atualiza o post no repositório
            await _postRepository.UpdateAsync(post);

            // Adiciona uma mensagem de sucesso e redireciona para a página inicial
            TempData["SuccessMessage"] = "Post atualizado com sucesso!";
            return RedirectToAction("Index");
        }

        // GET: /Details/{id} - Exibe os detalhes de um post
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            // Obtém o post pelo ID
            var post = await _postRepository.GetByIdAsync(id);

            // Verifica se o post existe
            if (post == null)
            {
                return NotFound(); // Retorna erro 404 se o post não for encontrado
            }

            // Mapeia o post para o ViewModel de detalhes
            var model = new PostDetailsViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Author = !string.IsNullOrEmpty(post.Author) ? post.Author : "Anônimo",
                CreatedAt = post.CreatedAt
            };

            return View(model); // Retorna a view de detalhes com as informações do post
        }
    }
}
