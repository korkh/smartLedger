using Infrastructure.Services;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class EncryptionConverter : ValueConverter<string, string>
{
    public EncryptionConverter(IEncryptionService encryption)
        : base(v => encryption.Encrypt(v), v => encryption.Decrypt(v)) { }
}
