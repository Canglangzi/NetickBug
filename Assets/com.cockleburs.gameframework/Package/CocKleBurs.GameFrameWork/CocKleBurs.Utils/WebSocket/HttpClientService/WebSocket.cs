// using System;
// using System.Net.WebSockets;
// using System.Text;
// using System.Threading;
// using System.Threading.Tasks;
// using UnityEngine;
//
//
// namespace CockleBurs.GameFramework.Utility
// {
// public static partial class ClentWebSocket
// {
//     private static ClientWebSocket socket;
//
//     public static event Action<string> OnMessageReceived;
//     public static event Action OnConnected;
//     public static event Action<string> OnError;
//
//     public static async Task Connect(string uri)
//     {
//         if (socket != null && socket.State == WebSocketState.Open)
//         {
//             Debug.LogWarning("Already connected.");
//             return;
//         }
//
//         socket = new ClientWebSocket();
//
//         try
//         {
//             await socket.ConnectAsync(new Uri(uri), CancellationToken.None);
//             OnConnected?.Invoke();
//             await SendMessage("Hello!");
//             StartReceivingMessages();
//         }
//         catch (Exception ex)
//         {
//             OnError?.Invoke(ex.Message);
//         }
//     }
//
//     public static async Task SendMessage(string message)
//     {
//         if (socket != null && socket.State == WebSocketState.Open)
//         {
//             var bytes = Encoding.UTF8.GetBytes(message);
//             var buffer = new ArraySegment<byte>(bytes);
//             await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
//         }
//         else
//         {
//             Debug.LogWarning("WebSocket is not connected.");
//         }
//     }
//
//     private static async void StartReceivingMessages()
//     {
//         var buffer = new byte[1024 * 4];
//
//         try
//         {
//             while (socket.State == WebSocketState.Open)
//             {
//                 var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
//
//                 if (result.MessageType == WebSocketMessageType.Close)
//                 {
//                     await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
//                 }
//                 else
//                 {
//                     var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
//                     OnMessageReceived?.Invoke(message);
//                 }
//             }
//         }
//         catch (Exception ex)
//         {
//             OnError?.Invoke(ex.Message);
//         }
//     }
//
//     public static void Close()
//     {
//         if (socket != null)
//         {
//             socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None).Wait();
//             socket = null;
//         }
//     }
// }
//
// }