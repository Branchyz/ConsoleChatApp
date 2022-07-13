using RiptideNetworking;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleChatApp
{
    public enum ServerToClientId : ushort
    {
        ClientJoined = 1,
        ChatMessage,
        ClientLeft,
    }
    
    public static class AppServer
    {
        private static Server _server;
        private static bool _isRunning = true;
        
        private static readonly Dictionary<ushort, string> Clients = new Dictionary<ushort, string>();

        public static void Start()
        {
            Console.Title = "Chat Server";
            new Thread(Loop).Start();
            
            Console.WriteLine("Press enter to stop the server at any time.");
            Console.ReadLine();

            _isRunning = false;

            Console.ReadLine();
        }

        private static void Loop()
        {
            _server = new Server(ushort.MaxValue, 1000, "Chat Server");
            
            _server.ClientDisconnected += ClientDisconnected;
            
            _server.Start(7777, 10);

            while (_isRunning)
            {
                _server.Tick();
                Thread.Sleep(10);
            }
            
            _server.Stop();
        }
        
        #region Server Messages

        private static void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            Console.WriteLine($"Client {e.Id} disconnected.");

            Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.ClientLeft);
            message.AddString(Clients[e.Id]);
            _server.SendToAll(message);
            
            Clients.Remove(e.Id);
        }

        [MessageHandler((ushort)ClientToServerId.ChatMessage)]
        public static void HandleChatMessage(Message message)
        {
            string chatMessage = message.GetString();
            ushort id = message.GetUShort();
            Console.WriteLine($"{Clients[id]}: {chatMessage}");
            
            Message messageToSendToClients = Message.Create(MessageSendMode.reliable, ServerToClientId.ChatMessage);
            messageToSendToClients.AddString(chatMessage);
            messageToSendToClients.AddString(Clients[id]);
            _server.SendToAll(messageToSendToClients, id);
        }
        
        [MessageHandler((ushort)ClientToServerId.Name)]
        public static void HandleName(Message message)
        {
            string username = message.GetString();
            ushort id = message.GetUShort();
            
            Console.WriteLine($"{id} has joined the chat as {username}.");
            Clients.Add(id, username);
            
            Message newClientMessage = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.ClientJoined);
            newClientMessage.AddString(username);
            _server.SendToAll(message, id);
        }
        
        #endregion
    }
}