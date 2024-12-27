using System.ComponentModel.DataAnnotations;

namespace HashKorea.DTOs.Common;

public class GetPostsResponseDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}