using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WindowsNotifier.Models;

namespace WindowsNotifier.Services
{
    public class LocalReceiverService
    {
        private readonly int _port;
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        public event Action<OrderEvent>? OnOrderEventReceived;

        public LocalReceiverService(int port)
        {
            _port = port;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Loopback, _port);
            _listener.Start();
            Task.Run(() => ListenLoopAsync(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel();
            _listener?.Stop();
        }

        private async Task ListenLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener!.AcceptTcpClientAsync(token);
                    _ = Task.Run(() => HandleClientAsync(client, token), token);
                }
                catch (Exception) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error de red aceptando cliente: {ex.Message}");
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            using (client)
            using (var stream = client.GetStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                try
                {
                    while (!token.IsCancellationRequested && client.Connected)
                    {
                        var line = await reader.ReadLineAsync(token);
                        if (string.IsNullOrEmpty(line)) break;

                        try
                        {
                            var orderEvent = JsonSerializer.Deserialize<OrderEvent>(line);
                            if (orderEvent != null)
                            {
                                OnOrderEventReceived?.Invoke(orderEvent);
                            }
                        }
                        catch (JsonException ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"JSON deserialization error: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en conexión cliente: {ex.Message}");
                }
            }
        }
    }
}
