using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HashKorea.Models;

public class SystemLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(50)]
    public string LogLevel { get; set; } = "Error"; // ��: "Error", "Warning", "Information"

    [StringLength(255)]
    public string Message { get; set; } = string.Empty;

    public string? Exception { get; set; } // ���� ������ ���� ��� ����

    public string? AdditionalData { get; set; } // �߰����� �����ͳ� ���ؽ�Ʈ ���� ( ex: UserId: 12345, IP: 192.168.1.1 )
}
