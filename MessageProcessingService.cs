using StackExchange.Redis;
using System.Collections.Concurrent;

namespace SignalRChatApplication
{
    public class MessageProcessingService : BackgroundService
    {
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly ConcurrentQueue<string> _messageQueue;

        public MessageProcessingService(IConnectionMultiplexer redisConnection, ConcurrentQueue<string> messageQueue)
        {
            _redisConnection = redisConnection;
            _messageQueue = messageQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_messageQueue.TryDequeue(out string? message))
                {
                    // Process the message (e.g., save to Redis)
                    IDatabase db = _redisConnection.GetDatabase();
                    await db.ListLeftPushAsync("chatMessages", message);
                }
                await Task.Delay(100); // Adjust delay as needed
            }
        }
    }
}
