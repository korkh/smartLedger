using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Services
{
    public class ClientSensitiveConfiguration : IEntityTypeConfiguration<ClientSensitive>
    {
        private readonly IEncryptionService _encryption;

        public ClientSensitiveConfiguration(IEncryptionService encryption)
        {
            _encryption = encryption;
        }

        public void Configure(EntityTypeBuilder<ClientSensitive> builder)
        {
            var converter = new EncryptionConverter(_encryption);

            builder.Property(x => x.EcpPassword).HasConversion(converter);
            builder.Property(x => x.EsfPassword).HasConversion(converter);
            builder.Property(x => x.BankingPasswords).HasConversion(converter);
            builder.Property(x => x.StrategicNotes).HasConversion(converter);
            builder.Property(x => x.PersonalInfo).HasConversion(converter);
        }
    }
}
