using TaskFlow.Api.Services;
using Xunit;

namespace TaskFlow.Tests;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_is_not_plaintext_and_is_salted()
    {
        var h1 = _hasher.Hash("secret123");
        var h2 = _hasher.Hash("secret123");

        Assert.NotEqual("secret123", h1);
        Assert.NotEqual(h1, h2); // different salt each time
    }

    [Fact]
    public void Verify_accepts_correct_password()
    {
        var hash = _hasher.Hash("correct horse battery staple");
        Assert.True(_hasher.Verify("correct horse battery staple", hash));
    }

    [Fact]
    public void Verify_rejects_wrong_password()
    {
        var hash = _hasher.Hash("correct horse battery staple");
        Assert.False(_hasher.Verify("wrong password", hash));
    }

    [Fact]
    public void Verify_returns_false_on_garbage_hash()
    {
        Assert.False(_hasher.Verify("whatever", "not-a-bcrypt-hash"));
    }
}
