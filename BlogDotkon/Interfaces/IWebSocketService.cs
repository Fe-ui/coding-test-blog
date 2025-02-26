using System.Net.WebSockets;
using System.Threading.Tasks;

public interface IWebSocketService
{
    Task AddClientAsync(WebSocket socket);  // Adiciona um cliente WebSocket
    Task SendMessageToClients(string message);  // Envia uma mensagem para todos os clientes
    Task HandleWebSocketAsync(WebSocket socket);
}
