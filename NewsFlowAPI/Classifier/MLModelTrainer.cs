using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using NewsFlowAPI.Models;

namespace NewsFlowAPI.Classifier
{
    public class MLModelTrainer
    {
        private readonly NewsDbContext _context;

        public MLModelTrainer(NewsDbContext context)
        {
            _context = context;
        }

        public async Task TrainAndSaveModelAsync()
        {
            var interactions = await _context.UserInteractions.ToListAsync();
            var allNews = await _context.News.ToListAsync();

            var trainingData = interactions
                .Join(allNews,
                    i => i.NewsId,
                    n => n.NewsId,
                    (i, n) => new NewsTrainingInput
                    {
                        UserId = i.UserId,
                        Category = n.Category,
                        Title = n.Title,
                        Content = n.Content,
                        Label = i.InteractionType
                    })
                .ToList();

            if (!trainingData.Any()) return;

            var mlContext = new MLContext();

            var dataView = mlContext.Data.LoadFromEnumerable(trainingData);

            var pipeline = mlContext.Transforms.Conversion.MapValueToKey("UserId")
                .Append(mlContext.Transforms.Text.FeaturizeText("TitleFeaturized", "Title"))
                .Append(mlContext.Transforms.Text.FeaturizeText("DescriptionFeaturized", "Content"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("CategoryEncoded", "Category"))
                .Append(mlContext.Transforms.Concatenate("Features", "TitleFeaturized", "DescriptionFeaturized", "CategoryEncoded"))
                .Append(mlContext.Regression.Trainers.FastTree());

            var model = pipeline.Fit(dataView);

            var modelPath = Path.Combine(AppContext.BaseDirectory, "modelRecommendations.zip");
            mlContext.Model.Save(model, dataView.Schema, modelPath);

            Console.WriteLine("Model ML salvat cu succes în model.zip");
        }
    }
}
