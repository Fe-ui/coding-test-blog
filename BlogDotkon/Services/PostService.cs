using BlogDotkon.Interfaces;
using BlogDotkon.Models;
using BlogDotkon.ViewModels;
using System.Security.Claims;

public class PostService
{
    private readonly IPostRepository _postRepository; // Interface para repositório de posts
    private readonly IWebSocketService _webSocketManager; // Serviço para gerenciamento de WebSocket

    // Construtor para inicializar as dependências do repositório e do WebSocket
    public PostService(IPostRepository postRepository, IWebSocketService webSocketManager)
    {
        _postRepository = postRepository;
        _webSocketManager = webSocketManager;
    }

    // Método para adicionar um novo post
    public async Task AddAsync(PostViewModel postViewModel, ClaimsPrincipal user)
    {
        // Verifica se o usuário está autenticado
        if (!user.Identity.IsAuthenticated)
            throw new UnauthorizedAccessException("Usuário não autenticado.");

        // Obtém o ID do usuário logado a partir do ClaimsPrincipal
        var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));

        // Cria um novo objeto Post a partir do ViewModel
        var post = new Post
        {
            Title = postViewModel.Title,
            Content = postViewModel.Content,
            Author = user.Identity.Name, // Nome do autor do post
            CreatedAt = DateTime.UtcNow, // Data de criação do post
            IsPublished = postViewModel.IsPublished // Define se o post está publicado
        };

        // Adiciona o novo post ao repositório
        await _postRepository.AddAsync(post);

        // Envia uma mensagem via WebSocket para notificar a criação do post
        await _webSocketManager.SendMessageToClients($"Novo post criado: {post.Title}");
    }

    // Método para obter todos os posts
    public async Task<List<PostViewModel>> GetAllAsync()
    {
        var posts = await _postRepository.GetAllAsync(); // Obtém todos os posts do repositório
        // Mapeia os posts para ViewModels e retorna uma lista
        return posts.Select(ToViewModel).ToList();
    }

    // Método para obter um post pelo seu ID
    public async Task<PostViewModel?> GetByIdAsync(int id)
    {
        var post = await _postRepository.GetByIdAsync(id); // Obtém o post pelo ID
        // Se o post não for encontrado, retorna null, senão mapeia o post para o ViewModel
        return post != null ? ToViewModel(post) : null;
    }

    // Método para atualizar um post
    public async Task UpdateAsync(PostViewModel postViewModel, ClaimsPrincipal user)
    {
        var post = await _postRepository.GetByIdAsync(postViewModel.Id); // Obtém o post do repositório pelo ID
        if (post == null)
            throw new KeyNotFoundException("Post não encontrado."); // Lança exceção se o post não existir

        // Verifica se o usuário é o autor do post
        if (post.Author != user.Identity.Name)
            throw new UnauthorizedAccessException("Usuário não autorizado a editar este post.");

        // Atualiza os campos do post com os dados do ViewModel
        post.Title = postViewModel.Title;
        post.Content = postViewModel.Content;
        post.UpdatedAt = DateTime.UtcNow; // Atualiza a data de modificação

        // Atualiza o post no repositório
        await _postRepository.UpdateAsync(post);
    }

    // Método para excluir um post
    public async Task DeleteAsync(int id, ClaimsPrincipal user)
    {
        var post = await _postRepository.GetByIdAsync(id); // Obtém o post do repositório
        if (post == null)
            throw new KeyNotFoundException("Post não encontrado."); // Lança exceção se o post não existir

        // Verifica se o usuário é o autor do post
        if (post.Author != user.Identity.Name)
            throw new UnauthorizedAccessException("Usuário não autorizado a excluir este post.");

        // Exclui o post do repositório
        await _postRepository.DeleteAsync(id);
    }

    // Método privado para converter um post em PostViewModel
    private static PostViewModel ToViewModel(Post post)
    {
        return new PostViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Author = !string.IsNullOrEmpty(post.Author) ? post.Author : "Desconhecido", // Define "Desconhecido" caso o autor não seja informado
            CreatedAt = post.CreatedAt,
            IsPublished = post.IsPublished // Mapeia o campo de publicação
        };
    }
}
