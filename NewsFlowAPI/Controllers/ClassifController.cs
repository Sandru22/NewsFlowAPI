using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewsFlowAPI.Classifier;

namespace NewsFlowAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassifController : ControllerBase
    {
        private readonly NewsClassifier _newsClassifier;

        public ClassifController(NewsClassifier newsClassifier)
        {
            _newsClassifier = newsClassifier;
        }

        [HttpPost("classify")]
        public IActionResult ClassifyNews([FromBody] NewsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest("Title and Description are required.");
            }

            string predictedCategory = _newsClassifier.PredictCategory(request.Title, request.Description);
            return Ok(new { Category = predictedCategory });
        }
    }

    public class NewsRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
