using Microsoft.AspNetCore.Identity;

public class User : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public ICollection<Wallet> Wallets { get; set; }

    // Field for authentication throw email
    public bool? EmailTwoFactorAuthenticationEnabled {  get; set; }
    public string? EmailTwoFactorAuthenticationCode { get; set; }
    public DateTime? EmailTwoFactorAuthenticationExpiryTime { get; set; }
    public DateTime? LastEmailTwoFactorAuthenticationCodeSent { get; set; }

    public string? GoogleAuthKey { get; set; }
    public string? ResetPasswordCode { get; set; }
    public DateTime? ResetPasswordCodeExpireTime { get; set; }
    public ICollection<Category>? UserCategories { get; set; }
}
