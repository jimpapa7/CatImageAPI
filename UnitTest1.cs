using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

public class CatsControllerTest
{
    private readonly CatsController _controller;
    private readonly AppDbContext dbContext;
    public CatsControllerTest() //create temporary db
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        dbContext = new AppDbContext(options);
        _controller = new CatsController(dbContext, null);
    }

    [Fact]
    public async Task GetCatById_ReturnsNotFound_WhenCatDoesNotExist()
    {
        var result = await _controller.GetCatById("noCat");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetCatById_ReturnsCat_WhenCatExists()
    {
        var cat = new CatEntity { Id = 1, CatId = "cat1", Width = 500, Height = 500, Image = new byte[] { 1, 2, 3, 4 }, Created = DateTime.Now };
        await dbContext.Cats.AddAsync(cat);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.GetCatById("cat1");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var RCat = new ReturnedCat(okResult.Value);
        Assert.Equal(cat.CatId, RCat.CatId);
    }
}
public class ReturnedCat
{
    public string CatId { get; set; }  // The id returned by the Cat API
    public int Width { get; set; }  // Image width returned by the Cat API
    public int Height { get; set; }  // Image height returned by the Cat API
    public string Image { get; set; }  // You will decide how to store the image (URL, binary, etc.)
    public string Tags { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;  // Timestamp of record creation
    public ReturnedCat(object Cat)
    {
        if (Cat == null)
            return;
        CatId = (string)(Cat.GetType().GetProperty("CatId").GetValue(Cat,null));
        Width = (int)(Cat.GetType().GetProperty("Width").GetValue(Cat,null));
        Height = (int)(Cat.GetType().GetProperty("Height").GetValue(Cat,null));
        Image = (string)(Cat.GetType().GetProperty("Image").GetValue(Cat,null));
        Tags = (string)(Cat.GetType().GetProperty("Tags").GetValue(Cat,null));
        Created = (DateTime)(Cat.GetType().GetProperty("Created").GetValue(Cat,null));
    }
}
