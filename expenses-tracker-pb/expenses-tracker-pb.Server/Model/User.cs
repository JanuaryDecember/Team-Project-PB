using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace expenses_tracker_pb.Server.Model;

public class User : IdentityUser
{
    [MaxLength(50)] public string? FirstName { get; set; }
    [MaxLength(50)] public string? LastName { get; set; }
    public ICollection<Wallet> Wallets { get; set; } = [];
    public bool EmailTwoFactorAuthenticationEnabled { get; set; }
    [MaxLength(255)] public string? EmailTwoFactorAuthenticationCode { get; set; }
    public DateTime? EmailTwoFactorAuthenticationExpiryTime { get; set; }
    public DateTime? LastEmailTwoFactorAuthenticationCodeSent { get; set; }
    [MaxLength(255)] public string? GoogleAuthKey { get; set; }
    [MaxLength(255)] public string? ResetPasswordCode { get; set; }
    public DateTime? ResetPasswordCodeExpireTime { get; set; }
    public ICollection<Category>? UserCategories { get; set; }
    public SecurityQuestion? SecurityQuestion { get; set; }
    [MaxLength(500)] public string? SecurityQuestionAnswer { get; set; }

    [Column(TypeName = "varbinary(max)")]
    //w postgersql nie ma typu varbinary(max) zamiast tego jest bytea, na potrzeby testów zamieniłem ten kod na bytea
    // [Column(TypeName = "bytea")]
    public byte[]? ProfilePicture { get; set; }
}