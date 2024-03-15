using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using WIA;

namespace WpfApp1.Models
{
    public class HttpServer
    {
        private HttpListener _listener;
        private ConcurrentDictionary<Guid, WebSocket> _clients = new ConcurrentDictionary<Guid, WebSocket>();
        private bool _stopRequested = false;
        bool _isWriteLog;
        private WIA.CommonDialog _wiaManager = new WIA.CommonDialog();
        public HttpServer(string url, bool writeLog)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(url);
            _isWriteLog = writeLog;
            if (writeLog)
            {
                string currentExecutionPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                LogWritter.SetUrl(currentExecutionPath);
            }
        }

        public void Start()
        {
            _listener.Start();
            Task.Run(() => Listen());
            if (_isWriteLog)
            {
                LogWritter.Write($"HTTP server started ");
            }
        }
        public void Stop()
        {
            _stopRequested = true;
            _listener.Stop();
            _listener.Close();
            if (_isWriteLog)
            {
                LogWritter.Write($"HTTP server stopped ");
            }
        }

        private async Task Listen()
        {
            while (!_stopRequested)
            {
                try
                {
                    HttpListenerContext context = await _listener.GetContextAsync();
                    if (context.Request.IsWebSocketRequest)
                    {
                        await HandleWebSocketRequest(context);
                    }
                }
                catch (Exception ex)
                {
                    if (_isWriteLog)
                    {
                        LogWritter.Write(ex.Message + "At function: Listen", LogType.ERROR);
                    }
                }

            }
        }
        private async Task HandleWebSocketRequest(HttpListenerContext context)
        {
            try
            {
                var webSocketContext = await context.AcceptWebSocketAsync(null);
                var webSocket = webSocketContext.WebSocket;
                var clientId = Guid.NewGuid();
                if (_isWriteLog)
                {
                    LogWritter.Write($@"{clientId} connected");
                }
                _clients.TryAdd(clientId, webSocket);
                await ReceiveMessages(clientId, webSocket);
            }
            catch (WebSocketException ex)
            {
                if (_isWriteLog)
                {
                    LogWritter.Write(ex.Message+ "At function: HandleWebSocketRequest ", LogType.ERROR);
                }
            }
           
        }

        private async Task ReceiveMessages(Guid clientId, WebSocket webSocket)
        {
            var buffer = new byte[1024];
            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _clients.TryRemove(clientId, out _);
                        if (_isWriteLog)
                        {
                            LogWritter.Write($" {clientId}: disconnected");
                        }
                        break;
                    }
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await HandleRequest(message, webSocket);
                    if (_isWriteLog)
                    {
                        LogWritter.Write($"Received message from client {clientId}: {message}");
                    }
                }
                catch (WebSocketException ex)
                {
                    _clients.TryRemove(clientId, out _);
                    if (_isWriteLog)
                    {
                        LogWritter.Write(ex.Message + " At function:  ReceiveMessages ", LogType.ERROR);
                    }
                    break;
                }
            }
        }
        private async Task HandleRequest(string requestType, WebSocket webSocket)
        {
            switch (requestType.ToUpper())
            {
                case "SCAN":
                    byte[] imageData = Scan();
                    if (imageData != null)
                    {
                        await SendMessageAsync(webSocket, imageData, WebSocketMessageType.Binary);
                        if (_isWriteLog)
                        {
                            LogWritter.Write($"Send Message:'Image is scanned' successfully");
                        }
                    }
                    else
                    {
                        await SendMessageAsync(webSocket, Encoding.UTF8.GetBytes("No image scanned."), WebSocketMessageType.Text);
                        if (_isWriteLog)
                        {
                            LogWritter.Write($@"Send Message:'No image scanned' successfully");
                        }
                    }
                    break;
                default:
                    await SendMessageAsync(webSocket, Encoding.UTF8.GetBytes("Unknown request type"), WebSocketMessageType.Text);
                    if (_isWriteLog)
                    {
                        LogWritter.Write($@"Send Message:'Unknown request type' successfully");
                    }
                    break;
            }
        }
        private async Task SendMessageAsync(WebSocket webSocket, byte[] buffer, WebSocketMessageType messageType)
        {
            try
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer), messageType, true, CancellationToken.None);
            }
            catch (WebSocketException ex)
            {
                if (_isWriteLog)
                {
                    LogWritter.Write(ex.Message + "Function: SendMessageAsync ",LogType.ERROR);
                }
            }
        }

        public byte[] Scan()
        {
            try
            {
                ImageFile imageFile = _wiaManager.ShowAcquireImage(
                      WiaDeviceType.ScannerDeviceType,
                      WiaImageIntent.ColorIntent,
                      WiaImageBias.MaximizeQuality,
                      "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}",
                      false,
                      true,
                      false);

                if (imageFile != null)
                {
                    byte[] imageData = (byte[])imageFile.FileData.get_BinaryData();
                    if (_isWriteLog)
                    {
                        LogWritter.Write("Image scanned successfully");
                    }
                    return imageData;
                }
                else
                {
                    if (_isWriteLog)
                    {
                        LogWritter.Write("No image  scanned");
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (_isWriteLog)
                {
                    LogWritter.Write(ex.Message+ "At function: Scan", LogType.ERROR);
                }
                return null;
            }
        }
    }
}
