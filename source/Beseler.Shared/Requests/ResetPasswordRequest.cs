namespace Beseler.Shared.Requests;
public sealed record ResetPasswordRequest(string Token, string Password);
