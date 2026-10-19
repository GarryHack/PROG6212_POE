using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace RACE_DAY_.Services;

/// One-way password hashing. We store the hash, never the password.
public static class Passwords
{
    private static readonly PasswordHasher<object> Hasher = new();
    private static readonly object Dummy = new();
    public static string Hash(string password) => Hasher.HashPassword(Dummy, password);
    public static bool Verify(string hash, string password) =>
        Hasher.VerifyHashedPassword(Dummy, hash, password) != PasswordVerificationResult.Failed;
}

public static class ClaimsExtensions
{
    /// The logged-in user's id (read from the session cookie).
    public static int UserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
