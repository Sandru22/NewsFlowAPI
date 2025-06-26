using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using NewsFlowAPI.Dto;
using NewsFlowAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;


namespace NewsFlowAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        private readonly NewsDbContext _context;

        public NewsController(NewsDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<NewsItem>>> GetNews()
        {
            return await _context.News
                .OrderByDescending(n => n.PublishedAt)
                .ToListAsync();

        }


        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<NewsItem>>> GetNewsByCategory(
            string category,
            [FromQuery] string userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var news = await _context.News
        .Where(n => n.Category.ToLower() == category.ToLower())
        .OrderByDescending(n => n.PublishedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

            var result = await MapNewsWithSubscriptions(news, userId);
            return Ok(result);
        }

        [HttpGet("{newsId}")]
        public async Task<ActionResult<NewsItemDto>> GetNewsById(int newsId, [FromQuery] string userId)
        {
            var news = await _context.News.FindAsync(newsId);
            if (news == null)
                return NotFound();

            var subscribedSources = await _context.Subscriptions
                .Where(s => s.userId == userId)
                .Select(s => s.Source)
                .ToListAsync();

            var dto = new NewsItemDto
            {
                NewsId = news.NewsId,
                Title = news.Title,
                Content = news.Content,
                Category = news.Category,
                PublishedAt = news.PublishedAt,
                Source = news.Source,
                Url = news.Url,
                Likes = news.Likes,
                ImageUrl = news.ImageUrl,
                HasSubscribed = subscribedSources.Contains(news.Source)
            };

            return Ok(dto);
        }

        [HttpGet("search/{words}")]

        public async Task<ActionResult<IEnumerable<NewsItem>>> SearchNews(
            string words, 
            [FromQuery] string userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(words))
            {
                return BadRequest("Search words cannot be empty.");
            }
            var searchWords = words.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var news = await _context.News
                .Where(n => searchWords.All(word =>
                                                    n.Title.ToLower().Contains(word.ToLower()) ||
                                                    n.Content.ToLower().Contains(word.ToLower())))
                .OrderByDescending(n => n.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var mapped = await MapNewsWithSubscriptions(news, userId);
            return Ok(mapped);
        }

        private async Task<ActionResult<IEnumerable<NewsItem>>> GetNewsByCategory(string category)
        {
            return await _context.News
                .OrderByDescending(n => n.PublishedAt)
                .Where(n => n.Category == category)
                .ToListAsync();
        }

        [HttpGet("Sport")]
        public async Task<ActionResult<IEnumerable<NewsItem>>> GetSportNews()
    => await GetNewsByCategory("Sport");

        [HttpGet("Auto")]
        public async Task<ActionResult<IEnumerable<NewsItem>>> GetAutoNews()
    => await GetNewsByCategory("Auto");

        [HttpGet("Politica")]
        public async Task<ActionResult<IEnumerable<NewsItem>>> GetPoliticaNews()
    => await GetNewsByCategory("Politica");

        [HttpGet("Externe")]
        public async Task<ActionResult<IEnumerable<NewsItem>>> GetExterneNews()
    => await GetNewsByCategory("Externe");

        [HttpGet("Meteo")]
        public async Task<ActionResult<IEnumerable<NewsItem>>> GetMeteoNews()
    => await GetNewsByCategory("Meteo");

        [HttpGet("Tech")]
        public async Task<ActionResult<IEnumerable<NewsItem>>> GetTechNews()
    => await GetNewsByCategory("Tech");



        [HttpGet("ById/{id}")]
        public async Task<ActionResult<NewsItem>> GetNewsItem(int id)
        {
            var newsItem = await _context.News.FindAsync(id);

            if (newsItem == null)
            {
                return NotFound();
            }

            return newsItem;
        }


        [HttpPost]
        public async Task<ActionResult<NewsItem>> PostNewsItem(NewsItem newsItem)
        {
            _context.News.Add(newsItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNewsItem), new { id = newsItem.NewsId }, newsItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutNewsItem(int id, NewsItem newsItem)
        {
            if (id != newsItem.NewsId)
            {
                return BadRequest();
            }

            _context.Entry(newsItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NewsItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNewsItem(int id)
        {
            var newsItem = await _context.News.FindAsync(id);
            if (newsItem == null)
            {
                return NotFound();
            }

            _context.News.Remove(newsItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NewsItemExists(int id)
        {
            return _context.News.Any(e => e.NewsId == id);
        }


        [HttpPost("subscribe/{userId}")]
        public async Task<IActionResult> Subscribe(string userId, [FromQuery] string source)
        {
            var alreadySubscribed = await _context.Subscriptions
                .AnyAsync(s => s.userId == userId && s.Source == source);

            if (alreadySubscribed)
                return BadRequest("Deja abonat.");

            _context.Subscriptions.Add(new Subscriptions
            {
                userId = userId,
                Source = source,
                
            });

            await _context.SaveChangesAsync();
            return Ok("Abonat cu succes.");
        }

        [HttpDelete("unsubscribe/{userId}")]
        public async Task<IActionResult> Unsubscribe(string userId, [FromQuery] string Source)
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.userId == userId && s.Source == Source);
            if (subscription == null)
            {
                return NotFound("Nu ești abonat la această sursă.");
            }
            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
            return Ok("Dezabonat cu succes.");
        }

        [HttpGet("subscriptions")]
        public async Task<ActionResult<IEnumerable<NewsItem>>> GetSubscribedNews([FromQuery] string userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        {
            var sources = await _context.Subscriptions
                .Where(s => s.userId == userId)
                .Select(s => s.Source)
                .ToListAsync();

            if (!sources.Any())
                return NotFound("User is not subscribed to any sources.");

            var news = await _context.News
                .Where(n => sources.Contains(n.Source))
                .OrderByDescending(n => n.PublishedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

            var mapped = await MapNewsWithSubscriptions(news, userId);
            return Ok(mapped);
        }

        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikeNewsItem(int id, [FromBody] string userId)
        {
            var newsItem = await _context.News.FindAsync(id);
            if (newsItem == null)
                return NotFound();


            var existingLike = await _context.NewsLikes
                .FirstOrDefaultAsync(nl => nl.NewsId == id && nl.UserId == userId);

            if (existingLike != null)
                return BadRequest("User has already liked this news item.");

            var newLike = new NewsLike
            {
                NewsId = id,
                UserId = userId,
                LikedAt = DateTime.UtcNow
            };

            _context.NewsLikes.Add(newLike);

            var interaction = new UserInteraction
            {
                UserId = userId,
                NewsId = id,
                InteractionType = 2, 
                InteractionDate = DateTime.UtcNow
            };
            _context.UserInteractions.Add(interaction);

            await _context.SaveChangesAsync();


            newsItem.Likes = await _context.NewsLikes.CountAsync(nl => nl.NewsId == id);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Like added", likes = newsItem.Likes });
        }

        [HttpPost("{id}/unlike")]
        public async Task<IActionResult> UnlikeNewsItem(int id, [FromBody] string userId)
        {
            var newsItem = await _context.News.FindAsync(id);
            if (newsItem == null)
                return NotFound();

            var existingLike = await _context.NewsLikes
                .FirstOrDefaultAsync(l => l.NewsId == id && l.UserId == userId);

            if (existingLike == null)
                return BadRequest("User has not liked this news item.");

            _context.NewsLikes.Remove(existingLike);
            await _context.SaveChangesAsync();


            newsItem.Likes = await _context.NewsLikes.CountAsync(nl => nl.NewsId == id);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Like removed", likes = newsItem.Likes });
        }

        [HttpGet("{id}/likes")]
        public async Task<IActionResult> GetLikesData(int id, [FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required");

            var totalLikes = await _context.NewsLikes.CountAsync(nl => nl.NewsId == id);
            var userLiked = await _context.NewsLikes.AnyAsync(nl => nl.NewsId == id && nl.UserId == userId);

            return Ok(new
            {
                TotalLikes = totalLikes,
                UserLiked = userLiked
            });
        }

        [HttpPost("{id}/share")]
        public async Task<IActionResult> ShareNewsItem(int id, [FromBody] string userId)
        {
            var newsItem = await _context.News.FindAsync(id);
            if (newsItem == null)
                return NotFound();


            var existingShare = await _context.NewsShares
                .FirstOrDefaultAsync(ns => ns.NewsId == id && ns.UserId == userId);

            if (existingShare != null)
            {
                return Ok(new { message = "Share already recorded" });
            }

            var newShare = new NewsShare
            {
                NewsId = id,
                UserId = userId,
                SharedAt = DateTime.UtcNow
            };

            _context.NewsShares.Add(newShare);

            var interaction = new UserInteraction
            {
                UserId = userId,
                NewsId = id,
                InteractionType = 3, 
                InteractionDate = DateTime.UtcNow
            };
            _context.UserInteractions.Add(interaction);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Share recorded successfully" });
        }

        [HttpPost("{id}/View")]
        public async Task<IActionResult> RecordView(int id, [FromBody] string userId)
        {
            var interaction = new UserInteraction
            {
                UserId = userId,
                NewsId = id,
                InteractionType = 1,
                InteractionDate = DateTime.UtcNow
            };

            _context.UserInteractions.Add(interaction);

            await _context.SaveChangesAsync();

            return Ok(new { message = "View recorded" });
        }

        [HttpPost("register-device")]
        public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceDto model)
        {
            if (string.IsNullOrWhiteSpace(model.UserId) || string.IsNullOrWhiteSpace(model.DeviceToken))
                return BadRequest("Invalid data");

            var existing = await _context.UserDevices
                .FirstOrDefaultAsync(d => d.DeviceToken == model.DeviceToken);

            if (existing == null)
            {
                _context.UserDevices.Add(new UserDevice
                {
                    UserId = model.UserId,
                    DeviceToken = model.DeviceToken
                });
                await _context.SaveChangesAsync();
            }

            return Ok("Device registered");
        }

        [HttpPost("unregister-device")]
        public async Task<IActionResult> UnregisterDevice([FromBody] RegisterDeviceDto model)
        {
            if (string.IsNullOrWhiteSpace(model.UserId) || string.IsNullOrWhiteSpace(model.DeviceToken))
                return BadRequest("Invalid data");

            var existing = await _context.UserDevices
                .FirstOrDefaultAsync(d => d.DeviceToken == model.DeviceToken && d.UserId == model.UserId);

            if (existing != null)
            {
                _context.UserDevices.Remove(existing);
                await _context.SaveChangesAsync();
            }

            return Ok("Device unregistered");
        }

        [HttpGet("recommended")]
        public async Task<ActionResult<IEnumerable<NewsItem>>> GetRecommendedNews(
        [FromQuery] string userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        {
            var userInteractions = await _context.UserInteractions
        .Where(ui => ui.UserId == userId)
        .ToListAsync();

            List<NewsItem> finalNews;

            if (userInteractions.Count < 50)
            {
                finalNews = await _context.News
                    .OrderByDescending(n => n.PublishedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            else
            {
                var allNews = await _context.News.ToListAsync();

                finalNews = GetPersonalizedRecommendations(userId, userInteractions, allNews)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            }


            var mapped = await MapNewsWithSubscriptions(finalNews, userId);
            return Ok(mapped);
        }

        private List<NewsItem> GetPersonalizedRecommendations(string userId, List<UserInteraction> userInteractions, List<NewsItem> allNews)
        {

            var mlContext = new MLContext();


            var modelPath = Path.Combine(AppContext.BaseDirectory, "modelRecommendations.zip");
            if (!System.IO.File.Exists(modelPath))
                return new List<NewsItem>(); 

            ITransformer model;
            using (var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read))
            {
                model = mlContext.Model.Load(stream, out _);
            }

            var predictionEngine = mlContext.Model.CreatePredictionEngine<NewsTrainingInput, NewsPrediction>(model);

            var userCategoryPreferences = CalculateUserCategoryPreferences(userInteractions, allNews);
            var seenNewsIds = userInteractions.Select(ui => ui.NewsId).ToHashSet();
            var now = DateTime.Now;
            var maxAgeDays = 168;

            var scoredNews = allNews
                .Where(news => !seenNewsIds.Contains(news.NewsId))
                .Select(news =>
                {
                    var mlScore = predictionEngine.Predict(new NewsTrainingInput
                    {
                        UserId = userId,
                        Content = news.Content,
                        Title = news.Title,
                        Category = news.Category
                    }).Score;

                    var normalizedMlScore = 1 / (1 + Math.Exp(-mlScore));
                    var ageDays = (now - news.PublishedAt).TotalHours;
                    var freshnessBonus = Math.Max(0, 1 - (ageDays / maxAgeDays));
                    var categoryScore = userCategoryPreferences.TryGetValue(news.Category, out var score) ? score : 0;

                    var finalScore = (freshnessBonus * 0.5) + (categoryScore * 0.05) + (normalizedMlScore * 0.45);

                    return new { NewsItem = news, FinalScore = finalScore };
                })
                .OrderByDescending(x => x.FinalScore)
                .ToList();

            return scoredNews.Select(x => x.NewsItem).ToList();
        }

        private Dictionary<string, float> CalculateUserCategoryPreferences(List<UserInteraction> interactions, List<NewsItem> allNews)
        {

            var categoryWeights = new Dictionary<string, float>();


            foreach (var interaction in interactions)
            {
                var newsItem = allNews.FirstOrDefault(n => n.NewsId == interaction.NewsId);
                if (newsItem != null && !string.IsNullOrEmpty(newsItem.Category))
                {

                    var interactionWeight = interaction.InteractionType switch
                    {
                        2 => 2.0f, 
                        3 => 3.0f,  
                        _ => 1.0f   
                    };

                    if (categoryWeights.ContainsKey(newsItem.Category))
                    {
                        categoryWeights[newsItem.Category] += interactionWeight;
                    }
                    else
                    {
                        categoryWeights[newsItem.Category] = interactionWeight;
                    }
                }
            }

            if (categoryWeights.Any())
            {
                var maxWeight = categoryWeights.Values.Max();
                return categoryWeights.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value / maxWeight
                );
            }

            return new Dictionary<string, float>();
        }

        private async Task<List<NewsItemDto>> MapNewsWithSubscriptions(List<NewsItem> news, string userId)
        {
            var userSubscriptions = await _context.Subscriptions
                .Where(s => s.userId == userId)
                .Select(s => s.Source)
                .ToListAsync();

            return news.Select(n => new NewsItemDto
            {
                NewsId = n.NewsId,
                Title = n.Title,
                Content = n.Content,
                Category = n.Category,
                PublishedAt = n.PublishedAt,
                Source = n.Source,
                Url = n.Url,
                Likes = n.Likes,
                ImageUrl = n.ImageUrl,
                HasSubscribed = userSubscriptions.Contains(n.Source)
            }).ToList();
        }

    }
}
