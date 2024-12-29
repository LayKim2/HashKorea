using HashKorea.Extensions;

namespace HashKorea.DTOs.Common;

public class GetPostsResponseDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string RelativeTime => CreatedDate.ToRelativeTimeString();

}