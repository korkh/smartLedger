using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Storage
{
    public class Seed
    {
        public static async Task SeedData(DataContext context, UserManager<User> userManager)
        {
            // 1. Create Users (Managers) - без изменений
            if (!userManager.Users.Any())
            {
                var users = new List<User>
                {
                    new User
                    {
                        FirstName = "Igor",
                        LastName = "Junior",
                        UserName = "igor",
                        Email = "igor@test.com",
                        Position = "Junior Accountant",
                    },
                    new User
                    {
                        FirstName = "Anna",
                        LastName = "Accountant",
                        UserName = "anna",
                        Email = "anna@test.com",
                        Position = "Senior Accountant",
                    },
                    new User
                    {
                        FirstName = "Admin",
                        LastName = "System",
                        UserName = "admin",
                        Email = "admin@test.com",
                        Position = "Administrator",
                    },
                };

                foreach (var user in users)
                {
                    await userManager.CreateAsync(user, "Pa$$w0rd");
                    if (user.UserName == "admin")
                        await userManager.AddToRoleAsync(user, "Admin");
                    else if (user.UserName == "anna")
                        await userManager.AddToRoleAsync(user, "Senior_Accountant");
                    else
                        await userManager.AddToRoleAsync(user, "Junior_Accountant");
                }
            }

            // 2. Service Directory - без изменений
            if (!context.ServiceReferences.Any())
            {
                var services = new List<ServiceReference>
                {
                    new ServiceReference
                    {
                        Name = "Consultation",
                        BasePrice = 5000,
                        AffectsNdsThreshold = false,
                        StandardTimeMinutes = 30,
                    },
                    new ServiceReference
                    {
                        Name = "Tax Filing",
                        BasePrice = 15000,
                        AffectsNdsThreshold = true,
                        StandardTimeMinutes = 120,
                    },
                    new ServiceReference
                    {
                        Name = "Bookkeeping",
                        BasePrice = 25000,
                        AffectsNdsThreshold = true,
                        StandardTimeMinutes = 480,
                    },
                };
                context.ServiceReferences.AddRange(services);
                await context.SaveChangesAsync();
            }

            // 3. Create Specific Clients from Excel data
            if (!context.Clients.Any())
            {
                var serviceList = await context.ServiceReferences.ToListAsync();

                // Данные на основе вашего списка паролей и структуры Excel
                var clientData = new List<Client>
                {
                    new Client
                    {
                        FirstName = "Sirius",
                        LastName = "Logistics TOO",
                        BinIin = "880512300123",
                        TaxRegime = "ОУР",
                        NdsStatus = "Taxpayer",
                        TaxRiskLevel = "Low",
                        EcpPassword = "Sirius777",
                        EsfPassword = "Sirius77",
                        BankingPasswords = "Almaty2023!",
                        Address = "Almaty, Dostyk 12",
                        ResponsiblePersonContact = "anna",
                    },
                    new Client
                    {
                        FirstName = "Artur",
                        LastName = "IP",
                        BinIin = "920101400567",
                        TaxRegime = "УР",
                        NdsStatus = "Non-taxpayer",
                        TaxRiskLevel = "Medium",
                        EcpPassword = "Artur@2024",
                        EsfPassword = "UdSuCw736x",
                        BankingPasswords = "Aa123456-",
                        Address = "Astana, Mangilik El 5",
                        ResponsiblePersonContact = "igor",
                    },
                    new Client
                    {
                        FirstName = "Edelveis",
                        LastName = "Trade",
                        BinIin = "150640012345",
                        TaxRegime = "ОУР",
                        NdsStatus = "Taxpayer",
                        TaxRiskLevel = "High",
                        EcpPassword = "Edelveis__007",
                        EsfPassword = "QazTrade*2030",
                        BankingPasswords = "Aa1234IP",
                        Address = "Karaganda, Bukhar-Zhyrau 45",
                        ResponsiblePersonContact = "anna",
                    },
                    new Client
                    {
                        FirstName = "Kontik",
                        LastName = "Kazakhstan",
                        BinIin = "100240055667",
                        TaxRegime = "УР",
                        NdsStatus = "Taxpayer",
                        TaxRiskLevel = "Low",
                        EcpPassword = "Kontikazakhstan2024",
                        EsfPassword = "Almaty2023!",
                        BankingPasswords = "Aa12345688",
                        Address = "Almaty, Rozybakieva 100",
                        ResponsiblePersonContact = "anna",
                    },
                    new Client
                    {
                        FirstName = "Amirzhan",
                        LastName = "Sultanov IP",
                        BinIin = "850303300111",
                        TaxRegime = "УР",
                        NdsStatus = "Non-taxpayer",
                        TaxRiskLevel = "Low",
                        EcpPassword = "Amirzhan2012",
                        EsfPassword = "Aa123456",
                        BankingPasswords = "Abzal2014",
                        Address = "Shymkent, Kunayev 12",
                        ResponsiblePersonContact = "igor",
                    },
                    new Client
                    {
                        FirstName = "Fedorova",
                        LastName = "Consulting",
                        BinIin = "780909400222",
                        TaxRegime = "ОУР",
                        NdsStatus = "Non-taxpayer",
                        TaxRiskLevel = "Medium",
                        EcpPassword = "Fedorova888",
                        EsfPassword = "Fedorova888",
                        BankingPasswords = "Аа1234",
                        Address = "Almaty, Abaya 52",
                        ResponsiblePersonContact = "anna",
                    },
                    new Client
                    {
                        FirstName = "Shynar",
                        LastName = "Beauty TOO",
                        BinIin = "200140088990",
                        TaxRegime = "УР",
                        NdsStatus = "Taxpayer",
                        TaxRiskLevel = "Low",
                        EcpPassword = "Shyn-ar1234@",
                        EsfPassword = "Аа1234",
                        BankingPasswords = "Sirius77",
                        Address = "Aktau, 12-25",
                        ResponsiblePersonContact = "igor",
                    },
                    new Client
                    {
                        FirstName = "Keruen",
                        LastName = "Group",
                        BinIin = "180540011223",
                        TaxRegime = "ОУР",
                        NdsStatus = "Taxpayer",
                        TaxRiskLevel = "Low",
                        EcpPassword = "Keruen$Ada25!",
                        EsfPassword = "AdaNagima25",
                        BankingPasswords = "Aa1234",
                        Address = "Astana, Turan 18",
                        ResponsiblePersonContact = "anna",
                    },
                    new Client
                    {
                        FirstName = "Diaservice",
                        LastName = "TOO",
                        BinIin = "120819780123",
                        TaxRegime = "ОУР",
                        NdsStatus = "Taxpayer",
                        TaxRiskLevel = "High",
                        EcpPassword = "Diaservice2020",
                        EsfPassword = "Vv12081978",
                        BankingPasswords = "Asd123123",
                        Address = "Atyrau, Satpayeva 2",
                        ResponsiblePersonContact = "anna",
                    },
                    new Client
                    {
                        FirstName = "Ainura",
                        LastName = "Sadykova IP",
                        BinIin = "820505400987",
                        TaxRegime = "УР",
                        NdsStatus = "Non-taxpayer",
                        TaxRiskLevel = "Low",
                        EcpPassword = "Ainura2022",
                        EsfPassword = "Aa1234",
                        BankingPasswords = "Ainura2022",
                        Address = "Almaty, Sain 15",
                        ResponsiblePersonContact = "igor",
                    },
                };

                foreach (var client in clientData)
                {
                    // Добавляем тариф
                    client.CurrentTariff = new ClientTariff
                    {
                        MonthlyFee = 35000,
                        OperationsLimit = 100,
                        CommunicationMinutesLimit = 200,
                        ContractDate = DateTime.UtcNow.AddMonths(-2),
                        IsActive = true,
                    };

                    // Добавляем тестовые транзакции
                    var service = serviceList[0];
                    client.Transactions.Add(
                        new Transaction
                        {
                            Date = DateTime.UtcNow.AddDays(-5),
                            OperationsCount = 2,
                            ActualTimeMinutes = 60,
                            Status = "Completed",
                            ServiceId = service.Id,
                            PerformerName = client.ResponsiblePersonContact,
                        }
                    );

                    context.Clients.Add(client);
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
