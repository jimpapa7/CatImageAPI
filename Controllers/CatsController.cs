using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class CatsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public CatsController(AppDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }

    // POST /api/cats/fetch: Fetch 25 cat images and store them in the database
    [HttpPost("fetch")]
    public async Task<IActionResult> FetchCats()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetStringAsync("https://api.thecatapi.com/v1/images/search?limit=25&has_breeds=1&api_key=live_ULdzjzvb9spXvNGwGA9rElkWDUAcbQOYFkyTzVQBoG5SsQnJatGXEhHA1TnlqFAG");

            var cats = JsonConvert.DeserializeObject<List<CatResponse>>(response);

            foreach (var cat in cats)
            {
                if (await _context.Cats.AnyAsync(c => c.CatId == cat.Id)) continue;

                var catEntity = new CatEntity
                {
                    CatId = cat.Id,
                    Width = cat.Width,
                    Height = cat.Height,
                    Image = Encoding.UTF8.GetBytes(cat.Url),
                    Created = DateTime.Now
                };

                // Add the cat to the database
                _context.Cats.Add(catEntity);

                // Process and link the tags (temperaments)
                foreach (var breed in cat.Breeds)
                {
                    var tagEntity = await _context.Tags.FirstOrDefaultAsync(t => t.Name == breed.Temperament)
                                    ?? new TagEntity { Name = breed.Temperament, Created = DateTime.Now };

                    if (tagEntity.Id == 0)  // Tag doesn't exist yet, so add it
                        _context.Tags.Add(tagEntity);


                    // Add the relationship to the join table (CatTag)
                    _context.CatTags.Add(new CatTag
                    {
                        CatId = catEntity.CatId,
                        TagId = tagEntity.Id
                    });

                }

                await _context.SaveChangesAsync();
            }
            return Ok("25 cat images fetched and saved!");


        }
        catch
        {
            _context.Dispose();
            return Ok("asdas");
        }
    }

    // GET /api/cats/{id}: Retrieve a cat by its ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCatById(string id)
    {
        var cat = await _context.Cats.FindAsync(id);

        if (cat == null)
            return NotFound("Cat not found");
        return Ok(new { cat.CatId, cat.Created, cat.Height, cat.Width, Image = Encoding.UTF8.GetString(cat.Image) });
    }
    // GET /api/cats: Retrieve all cats with paging support
    [HttpGet]
    public async Task<IActionResult> GetAllCats([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var cats = await _context.Cats
            .OrderBy(cat => cat.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(cats.Select(cat => new { cat.CatId, cat.Created, cat.Height, cat.Width, Image = Encoding.UTF8.GetString(cat.Image) }).ToList());
    }
    // GET /api/cats?tag=playful&page=1&pageSize=10: Retrieve cats by tag with paging support
    [HttpGet("byTag")]
    public async Task<IActionResult> GetCatsByTag([FromQuery] string tag, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var tags = tag.Split(',');
        var dbTags = await _context.Tags.ToListAsync();
        List<int> tagIds = dbTags
        .Where(t => tags.Any(x => t.Name.Contains(x, StringComparison.OrdinalIgnoreCase)))
        .Select(t => t.Id)
        .ToList();

        if (tagIds.Count == 0)
        {
            // No tag found with the specified name
            return NotFound("Tag not found.");
        }

        // Query CatTags to find cats with the specified tagId
        var catsWithTag = await _context.CatTags
            .Where(ct => tagIds.Contains(ct.TagId))
            .Select(ct => ct.CatId)  // Project to 
            .ToListAsync();

        if (catsWithTag == null || !catsWithTag.Any())
        {
            // No cats found with the specified tag
            return NotFound("No cats found with the specified tag.");
        }
        return Ok(await _context.Cats.Where(cat => catsWithTag.Contains(cat.CatId))
            .OrderBy(cat => cat.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cat => new { cat.CatId, cat.Created, cat.Height, cat.Width, Image = Encoding.UTF8.GetString(cat.Image) }).ToListAsync());
    }
}

// Helper classes for deserialization
public class CatResponse
{
    public string Id { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Url { get; set; }
    public List<Breed> Breeds { get; set; }
}
public class Breed
{
    public string Temperament { get; set; }
}
