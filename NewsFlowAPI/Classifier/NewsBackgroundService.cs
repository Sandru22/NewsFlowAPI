using NewsFlowAPI.Models;

namespace NewsFlowAPI.Classifier
{
    public class NewsBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private static readonly List<string> RssUrls = RssSources.Feeds.ToList();
        private DateTime _lastModelTrainingTime = DateTime.MinValue;
        private readonly TimeSpan _modelTrainingInterval = TimeSpan.FromHours(6); 

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

                    
                    if (DateTime.Now - _lastModelTrainingTime > _modelTrainingInterval)
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<NewsDbContext>();
                        var trainer = new MLModelTrainer(dbContext);

                        try
                        {
                            await trainer.TrainAndSaveModelAsync();
                            _lastModelTrainingTime = DateTime.Now;
                            Console.WriteLine("Model ML reantrenat automat.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Eroare la reantrenarea modelului: {ex.Message}");
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken); 
            }
        }
    }
}
