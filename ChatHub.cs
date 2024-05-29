using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;

namespace SignalRChatApplication
{
    public class ChatHub : Hub
    {
        private readonly IConnectionMultiplexer _redisConnection;

        public ChatHub(IConnectionMultiplexer redisConnection)
        {
            _redisConnection = redisConnection;
        }
        public async Task SendMessage(string user, string message)
        {
            // Save message to Redis for persistence
            IDatabase db = _redisConnection.GetDatabase();
            await db.ListLeftPushAsync("chatMessages", $"{user}: {message}");
            var encryptedMessage = EncryptMessage(message);

            //await Clients.All.SendAsync("ReceiveMessage", user, message);
            await Clients.Client(this.Context.ConnectionId).SendAsync("ReceiveMessage", user, encryptedMessage);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} has joined the group {groupName}");
        }

        public async Task LeaveGroup(string groupName)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} has left the group {groupName}");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        private string EncryptMessage(string message)
        {
            using (Aes aesAlg = Aes.Create())
            {
                
                // Determine the legal key sizes for encryption
                KeySizes[] ks = aesAlg.LegalKeySizes;
                foreach (KeySizes item in ks)
                {
                    Console.WriteLine("Legal min key size = " + item.MinSize);
                    Console.WriteLine("Legal max key size = " + item.MaxSize);

                    // Legal min key size = 128
                    // Legal max key size = 256
                }

                // Set key and IV
                aesAlg.Key = Encoding.UTF8.GetBytes("EncryptionKey123");
                aesAlg.IV = Encoding.UTF8.GetBytes("EncryptionIV1234");

                // Create an encryptor to perform the stream transform
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream
                            swEncrypt.Write(message);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }
    }
}
