using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class WebSocketService : IWebSocketService
{
    private readonly List<WebSocket> _clients = new List<WebSocket>();

    // Método para adicionar um cliente à lista
    public async Task AddClientAsync(WebSocket socket)
    {
        _clients.Add(socket);
    }

    // Método para enviar mensagens para todos os clientes conectados
    public async Task SendMessageToClients(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        foreach (var client in _clients)
        {
            await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    // Implementação do método HandleWebSocketAsync
    public async Task HandleWebSocketAsync(WebSocket socket)
    {
        await AddClientAsync(socket);
        var buffer = new byte[1024 * 4];

        // Loop para receber mensagens do cliente
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                await SendMessageToClients(message);  // Envia a mensagem para todos os clientes
            }
        }

        // Quando o socket for fechado, remove o cliente
        await RemoveClientAsync(socket);
    }

    // Método para remover um cliente da lista
    public async Task RemoveClientAsync(WebSocket socket)
    {
        _clients.Remove(socket);
        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the server", CancellationToken.None);
        socket.Dispose();
    }
}
