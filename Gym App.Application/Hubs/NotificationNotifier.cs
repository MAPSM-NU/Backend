using Gym_App.Domain;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Gym_App.Application.Hubs
{
    public interface INotificationSink
    {
        ValueTask PushAsync(Notification notification);
    }
    public class NotificationNotifier : BackgroundService, INotificationSink
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationNotifier> _logger;
        private readonly Channel<Notification> _channel;
        private readonly static TimeSpan Period = TimeSpan.FromSeconds(2);
        
        public NotificationNotifier(ILogger<NotificationNotifier> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _channel = Channel.CreateUnbounded<Notification>();
            _serviceProvider = serviceProvider;
        }

        public ValueTask PushAsync(Notification notification) => _channel.Writer.WriteAsync(notification);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(_logger.IsEnabled(LogLevel.Information) ? $"Notification notifier started at {DateTimeOffset.Now}" : "Notification notifier started.");
            var timer = new PeriodicTimer(Period);
            while (true && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        timer.Dispose();
                        return;
                    }

                    var notification = await _channel.Reader.ReadAsync(stoppingToken);
                    var userId = notification.User.Id.ToString();
                    
                    using var scope = _serviceProvider.CreateScope();
                    var hub = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();

                    var payload = new NotifMessage(userId, notification.Content ?? "", DateTimeOffset.Now);
                    
                    _logger.LogInformation($"Sending notification '{notification.Title}' to user {userId}");
                    await hub.Clients.User(userId).SendAsync("Receive_Notification", payload, stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error in notification service.");
                }
            }
        }
    }
}
