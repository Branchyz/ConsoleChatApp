using RiptideNetworking;
using System;
using System.Threading;

namespace ConsoleChatApp
{
    
    public enum ClientToServerId : ushort
    {
        Name = 4,
        ChatMessage
    }
    
    public static class AppClient
    {
        private static Client _client;
        private static bool _isRunning = true;
        
        public static void Start(string ip, string username)
        {
            Console.Title = "Chat Client";
            
            new Thread(() => Loop(ip, username)).Start();
            
            Console.WriteLine("You can always type '/quit' to stop the client at any time.");

            new Thread(InputLoop).Start();
        }

        private static void Loop(string ip, string username)
        {
            _client = new Client(ushort.MaxValue, 1000, 2, "Chat Client");

            _client.Connect($"{ip}:7777");
            
            SendName(username);

            while (_isRunning)
            {
                _client.Tick();
                Thread.Sleep(10);
            }

            _client.Disconnect();
        }

        private static void InputLoop()
        {
            while (_isRunning)
            {
                string chatMessage = Console.ReadLine();
                
                if (chatMessage == "/quit")
                {
                    _isRunning = false;
                }
                else if(!String.IsNullOrWhiteSpace(chatMessage))
                    SendChatMessage(chatMessage);
            }
        }
        
        #region Client Messages
        
        private static void SendName(string username)
        {
            Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.Name);
            message.AddString(username);
            message.AddUShort(_client.Id);
            _client.Send(message);
        }
        
        private static void SendChatMessage(string chatMessage)
        {
            Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.ChatMessage);
            message.AddString(chatMessage);
            message.AddUShort(_client.Id);
            _client.Send(message);  
            
            Console.WriteLine($"You: {chatMessage}");
        }
        
        [MessageHandler((ushort)ServerToClientId.ClientJoined)]
        public static void HandleJoin(Message message)
        {
            string username = message.GetString();
            
            Console.WriteLine($"{username} joined the chat!");
        }

        [MessageHandler((ushort)ServerToClientId.ChatMessage)]
        public static void HandleChatMessage(Message message)
        {
            string chatMessage = message.GetString();
            string username = message.GetString();
            Console.WriteLine($"{username}: {chatMessage}");
        }
        
        [MessageHandler((ushort)ServerToClientId.ClientLeft)]
        public static void HandleClientLeave(Message message)
        {
            string username = message.GetString();
            
            Console.WriteLine($"{username} disconnected.");
        }
        #endregion
    }
}