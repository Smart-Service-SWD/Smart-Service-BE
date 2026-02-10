using Microsoft.Extensions.Options;
using SmartService.Application.Abstractions.Auth;
using System.Security.Cryptography;
using System.Text;

namespace SmartService.Infrastructure.Auth;

/// <summary>
/// AES encryption service for encrypting/decrypting refresh tokens.
/// Based on CodeMaze pattern.
/// </summary>
public class AesEncryptionService
{
    private readonly TokenConfiguration _config;

    public AesEncryptionService(IOptions<TokenConfiguration> config)
    {
        _config = config.Value;
    }

    private byte[] GetKey() => Encoding.UTF8.GetBytes(_config.EncryptionKey.PadRight(32)[..32]);
    private byte[] GetIV() => Encoding.UTF8.GetBytes(_config.EncryptionIV.PadRight(16)[..16]);

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = GetKey();
        aes.IV = GetIV();

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
            sw.Write(plainText);

        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = GetKey();
        aes.IV = GetIV();

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}
