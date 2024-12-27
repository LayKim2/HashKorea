using System.ComponentModel.DataAnnotations;

namespace HashKorea.DTOs.User;

public class PostRequestDto
{
    [Required]
    public string Category { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    // TO DO: max size and file type
    public List<IFormFile>? ImageFiles { get; set; }

    //[DataType(DataType.Upload)]
    ////[MaxFileSize(5 * 1024 * 1024)]
    ////[AllowedExtensions(new string[] { ".jpg", ".png", ".gif" })]
    //public IFormFile MainImage { get; set; }
}