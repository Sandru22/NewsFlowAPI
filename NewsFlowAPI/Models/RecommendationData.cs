using Microsoft.ML.Data;

public class RecommendationData
{
    [LoadColumn(0)] public string UserId { get; set; }
    [LoadColumn(1)] public float NewsId { get; set; }
    [LoadColumn(2)] public float Score { get; set; }
}

public class RecommendationPrediction
{
    [ColumnName("Score")] public float Score { get; set; }
}
