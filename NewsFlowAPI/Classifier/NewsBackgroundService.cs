using NewsFlowAPI.Models;

namespace NewsFlowAPI.Classifier
{
    public class NewsBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly List<string> RssUrls = RssSources.Feeds.ToList();


        public NewsBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var newsProcessor = scope.ServiceProvider.GetRequiredService<NewsProcessorService>();
                    await newsProcessor.FetchAndStoreNewsAsync(RssUrls);
                }

                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }
    }
}
