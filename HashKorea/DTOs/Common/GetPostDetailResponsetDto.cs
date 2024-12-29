using System.ComponentModel.DataAnnotations;

namespace HashKorea.DTOs.Common;

public class GetPostDetailResponsetDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserCountry { get; set; } = string.Empty;
    public bool IsOwner { get; set; } = false;
}