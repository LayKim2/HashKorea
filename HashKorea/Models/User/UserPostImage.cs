using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using HashKorea.Common.Constants;
using System.Text.Json.Serialization;

namespace HashKorea.Models;

public class UserPostImage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey("UserPost")]
    public int PostId { get; set; }
    [JsonIgnore]
    public virtual UserPost UserPost { get; set; } = null!;

    [ForeignKey("User")]
    public int UserId { get; set; }
    [JsonIgnore]
    public virtual User User { get; set; } = null!;

    public string StoragePath { get; set; } = string.Empty; // 실제 저장 경로 (예: "actors/12345.jpg")
    public string PublicUrl { get; set; } = string.Empty;   // 공개 접근 가능한 URL

    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;
    [StringLength(10)]
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; } = 0;
    public DateTime CreatedDate { get; set; } = DateTime.Now;

}
