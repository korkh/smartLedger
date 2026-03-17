using Microsoft.AspNetCore.DataProtection;

namespace Infrastructure.Services
{
    public interface IEncryptionService
    {
        string Encrypt(string text);
        string Decrypt(string encryptedText);
    }

    public class EncryptionService(IDataProtectionProvider provider) : IEncryptionService
    {
        private readonly IDataProtector _protector = provider.CreateProtector(
            "ClientSensitiveDataV1"
        );

        public string Encrypt(string text) =>
            string.IsNullOrEmpty(text) ? text : _protector.Protect(text);

        public string Decrypt(string encryptedText) =>
            string.IsNullOrEmpty(encryptedText)
                ? encryptedText
                : _protector.Unprotect(encryptedText);
    }
}
