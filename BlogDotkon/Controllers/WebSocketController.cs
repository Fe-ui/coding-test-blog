using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class WebSocketController : ControllerBase
{
    private readonly IWebSocketService _webSocketService;

    public WebSocketController(IWebSocketService webSocketService)
    {
        _webSocketService = webSocketService;
    }

    // Endpoint para aceitar conexões WebSocket
    [HttpGet("connect")]
    public async Task<IActionResult> Connect()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            return BadRequest();
        }

        var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        await _webSocketService.AddClientAsync(webSocket);

        // Loop para continuar recebendo mensagens
        await ReceiveMessages(webSocket);

        return Ok();
    }

    // Recebe mensagens de um WebSocket e envia para todos os outros clientes
    private async Task ReceiveMessages(WebSocket socket)
    {
        var buffer = new byte[1024 * 4];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                // Envia a mensagem para todos os clientes
                await _webSocketService.SendMessageToClients(message);
            }
        }
    }
}
