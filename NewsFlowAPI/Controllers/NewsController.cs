using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using NewsFlowAPI.Models;
using System.IdentityModel.Tokens.Jwt;


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

        // GET: api/news - Obține toate știrile
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NewsItem>>> GetNews()
        {
            return await _context.News.ToListAsync();
        }

        // GET: api/news/5 - Obține o singură știre după ID
        [HttpGet("{id}")]
        public async Task<ActionResult<NewsItem>> GetNewsItem(int id)
        {
            var newsItem = await _context.News.FindAsync(id);

            if (newsItem == null)
            {
                return NotFound();
            }

            return newsItem;
        }

        // POST: api/news - Adaugă o nouă știre
        [HttpPost]
        public async Task<ActionResult<NewsItem>> PostNewsItem(NewsItem newsItem)
        {
            _context.News.Add(newsItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNewsItem), new { id = newsItem.NewsId }, newsItem);
        }

        // PUT: api/news/5 - Actualizează o știre existentă
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

        // DELETE: api/news/5 - Șterge o știre
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




        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikeNewsItem(int id, [FromBody] string userId)
        {
            var newsItem = await _context.News.FindAsync(id);
            if (newsItem == null)
                return NotFound();

            // Schimbă metoda de căutare a like-ului existent
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
                InteractionType = 2, // Like
                InteractionDate = DateTime.UtcNow
            };
            _context.UserInteractions.Add(interaction);

            await _context.SaveChangesAsync();

            // Actualizează numărul de like-uri după salvare
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

            // Găsește like-ul corect
            var existingLike = await _context.NewsLikes
                .FirstOrDefaultAsync(l => l.NewsId == id && l.UserId == userId);

            if (existingLike == null)
                return BadRequest("User has not liked this news item.");

            _context.NewsLikes.Remove(existingLike);
            await _context.SaveChangesAsync();

            // Actualizează numărul de like-uri
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

            // Verifică dacă utilizatorul a distribuit deja această știre
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
                InteractionType = 3, // Share
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

    }
}
