using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

public class User : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public ICollection<Wallet> Wallets { get; set; }
    public string? GoogleAuthKey { get; set; }
    public string? ResetPasswordCode { get; set; }
    public DateTime? ResetPasswordCodeExpireTime { get; set; }
    public ICollection<Category>? UserCategories { get; set; }

    [Column(TypeName = "varbinary(max)")]
    public byte[]? ProfilePicture { get; set; }
}
