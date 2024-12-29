using System.ComponentModel.DataAnnotations;

namespace HashKorea.DTOs.Common;

public class GetCommonCodeResponseDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}