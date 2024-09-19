using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class CatsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;

    public CatsController(AppDbContext context, IHttpClientFactory httpClientFactory = null)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
    }
    private string GetCatsTags(string Id)
    {
        var TagIds = _context.CatTags.Where(ct => ct.CatId == Id).Select(ct => ct.TagId).ToList();
        return String.Join(',', _context.Tags.Where(t => TagIds.Contains(t.Id)).Select(t => t.Name));
    }
    [HttpPost("fetch")]
    public async Task<IActionResult> FetchCats()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetStringAsync("https://api.thecatapi.com/v1/images/search?limit=25&has_breeds=1&api_key=live_ULdzjzvb9spXvNGwGA9rElkWDUAcbQOYFkyTzVQBoG5SsQnJatGXEhHA1TnlqFAG");
            _context.Database.OpenConnection();
            var cats = JsonConvert.DeserializeObject<List<CatResponse>>(response);
            foreach (var cat in cats)
            {
                if (await _context.Cats.AnyAsync(c => c.CatId == cat.Id)) continue;
                //Determine if a cat exists
                var catEntity = new CatEntity
                {
                    CatId = cat.Id,
                    Width = cat.Width,
                    Height = cat.Height,
                    Image = Encoding.UTF8.GetBytes(cat.Url),
                    Created = DateTime.Now
                };
                var catValidation = new CatValidator().Validate(catEntity);
                if (!catValidation.IsValid)
                    return BadRequest(catValidation.Errors);
                // Add the cat to the database
                _context.Cats.Add(catEntity);
                await _context.SaveChangesAsync();

                // Process and link the tags (temperaments)
                foreach (var breed in cat.Breeds)
                {
                    //Determine if a temperament exists
                    foreach (var tag in breed.Temperament.Trim().Split(','))
                    {
                        var tagEntity = await _context.Tags.FirstOrDefaultAsync(t => t.Name.Trim() == tag.Trim())
                                        ?? new TagEntity { Name = tag.Trim(), Created = DateTime.Now };

                        var TagValidation = new TagValidator().Validate(tagEntity);
                        if (!TagValidation.IsValid)
                            return BadRequest(TagValidation.Errors);
                        if (tagEntity.Id == 0)  // Tag doesn't exist yet, so add it
                            _context.Tags.Add(tagEntity);

                        await _context.SaveChangesAsync();


                        // Add the relationship to the join table (CatTag)
                        _context.CatTags.Add(new CatTag
                        {
                            CatId = catEntity.CatId,
                            TagId = tagEntity.Id
                        });

                        await _context.SaveChangesAsync();
                    }
                }

                _context.Database.CloseConnection();
            }
            return Ok("25 cat images fetched and saved!");


        }
        catch
        {
            _context.Dispose();
            return Ok("Failed");
        }
    }
    [HttpPost("getcatbyid")]
    public async Task<IActionResult> GetCatById([FromBody] string id)
    {
        var cat = await _context.Cats.FindAsync(id);

        if (cat == null)
            return NotFound("Cat not found");
        return Ok(new { cat.CatId, cat.Height, cat.Width, Tags = GetCatsTags(cat.CatId), Image = Encoding.UTF8.GetString(cat.Image), cat.Created });
    }
    [HttpGet]
    public async Task<IActionResult> GetAllCats([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var cats = await _context.Cats
            .OrderBy(cat => cat.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(cats.Select(cat => new { cat.CatId, cat.Height, cat.Width, Tags = GetCatsTags(cat.CatId), Image = Encoding.UTF8.GetString(cat.Image), cat.Created }).ToList());
        //return Ok();
    }
    [HttpGet("byTag")]
    public async Task<IActionResult> GetCatsByTag([FromQuery] string tag, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var tags = tag.Split(',');
        var dbTags = await _context.Tags.ToListAsync();
        List<int> tagIds = dbTags
        .Where(t => tags.Any(x => t.Name.Trim().Equals(x.Trim(), StringComparison.OrdinalIgnoreCase)))
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
        var catTags = await _context.Cats.Where(cat => catsWithTag.Contains(cat.CatId)).ToListAsync();
        if (catTags != null)
            return Ok(catTags.OrderBy(cat => cat.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(cat => new { cat.CatId, cat.Height, cat.Width, Tags = GetCatsTags(cat.CatId), Image = Encoding.UTF8.GetString(cat.Image), cat.Created }).ToList());
        else
            return NotFound("No cats found with the specified tag.");
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
