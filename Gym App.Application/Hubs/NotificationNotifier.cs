using Gym_App.Infastructure.DTOs.Notification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Threading.Channels;

namespace Gym_App.Application.Hubs
{
    public interface INotificationSink
    {
        ValueTask PushAsync(NotifSentMessage notification);
    }
    public class NotificationNotifier : BackgroundService, INotificationSink
    {
        //Planning to implement redis but for now, implementation will only be local
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationNotifier> _logger;
        private readonly ConnectionMultiplexer _redis;
        private readonly Channel<NotifSentMessage> _channel;
        private readonly static TimeSpan Period = TimeSpan.FromSeconds(2);
        
        public NotificationNotifier(ILogger<NotificationNotifier> logger,IServiceProvider serviceProvider)
        {
            _logger = logger;
            _channel = Channel.CreateUnbounded<NotifSentMessage>();
            _serviceProvider = serviceProvider;
            //_redis = ConnectionMultiplexer.Connect("Given ip");//not implemented

        }
        public ValueTask PushAsync(NotifSentMessage notification) => _channel.Writer.WriteAsync(notification);
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //this is a local implementation
            var timer = new PeriodicTimer(Period);
            while (true && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var notification = await _channel.Reader.ReadAsync(stoppingToken);
                    var message = notification.Message;
                    var userId = notification.recieverId;
                    using var scope = _serviceProvider.CreateScope();

                    var hub = scope.ServiceProvider.GetRequiredService<NotificationHub>();

                    var payload = new NotifSentMessage(userId.ToString(),message,notification.recieverId,DateTimeOffset.Now);
                    _logger.LogInformation($"Sending channel notification '{message}' to {userId}");
                    await hub.SendNotif(payload,stoppingToken);//curently it doesnt send the connection id but the actual user id which is wrong bs I am tired
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error in notification service.");
                }
            }
        }
    }
}
