using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace NewsFlowAPI.Classifier
{
    public class NewsClassifier
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;
        private readonly PredictionEngine<NewsData, NewsPrediction> _predictionEngine;

        private static readonly string modelPath = Path.Combine(Directory.GetCurrentDirectory(), "Models", "news_classification_model.zip");

        public NewsClassifier()
        {
            _mlContext = new MLContext();

            if (File.Exists(modelPath))
            {
                DataViewSchema modelSchema;
                _model = _mlContext.Model.Load(modelPath, out modelSchema);
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<NewsData, NewsPrediction>(_model);
            }
            else
            {
                throw new FileNotFoundException($"Model file not found: {modelPath}");
            }
        }

        public string PredictCategory(string title, string description)
        {
            var prediction = _predictionEngine.Predict(new NewsData
            {
                Title = title,
                Description = description
            });

            return prediction.PredictedCategory;
        }
    }

    public class NewsData
    {
        [LoadColumn(0)] public string Title { get; set; }
        [LoadColumn(1)] public string Description { get; set; }
        [LoadColumn(2)] public string Link { get; set; }
        [LoadColumn(3)] public string Category { get; set; }
    }

    public class NewsPrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedCategory { get; set; }
    }
}
