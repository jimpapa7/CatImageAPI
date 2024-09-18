using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class CatEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }  // Auto-incremental unique integer for the cat in your database
    [Key]
    public string CatId { get; set; }  // The id returned by the Cat API
    [Required]
    public int Width { get; set; }  // Image width returned by the Cat API
    [Required]
    public int Height { get; set; }  // Image height returned by the Cat API
    public byte[] Image { get; set; }  // You will decide how to store the image (URL, binary, etc.)
    public DateTime Created { get; set; } = DateTime.Now;  // Timestamp of record creation
}
