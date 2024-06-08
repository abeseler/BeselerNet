using System.Security.Claims;

namespace Beseler.Shared;
public static class AppClaims
{
    public const string EmailVerified = "email_verified";
    public static Claim EmailVerifiedClaim(bool isVerified) => new(EmailVerified, isVerified.ToString());

    public static string ConfirmEmail(string audience) => $"{audience}/confirm_email";
    public static Claim ConfirmEmailClaim(string audience, string email) => new(ConfirmEmail(audience), email);

    public static string ResetPassword(string audience) => $"{audience}/reset_password";
    public static Claim ResetPasswordClaim(string audience) => new(ResetPassword(audience), true.ToString());

    public static bool HasClaim(this ClaimsPrincipal principal, string claim) => principal.HasClaim(c => c.Type == claim);
}
