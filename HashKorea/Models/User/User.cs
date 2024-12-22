using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using HashKorea.Common.Constants;

namespace HashKorea.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    //[Required]
    //public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(5)]
    public string Gender { get; set; } = string.Empty; // ����

    [StringLength(50)]
    public string Country { get; set; } = string.Empty; // ����

    [StringLength(255)]
    public string StoragePath { get; set; } = string.Empty; // ���� ���� ��� (��: "actors/12345.jpg")
    public string PublicUrl { get; set; } = string.Empty;   // ���� ���� ������ URL

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
    public bool IsDeleted { get; set; } = false;

    public virtual UserAuth UserAuth { get; set; } = new UserAuth();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
    public virtual ICollection<Term> TermsAgreements { get; set; } = new HashSet<Term>();
}
