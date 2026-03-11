using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Storage
{
    public class Seed
    {
        public static async Task SeedData(DataContext context, UserManager<User> userManager)
        {
            // 1. Users (Identity)
            if (!userManager.Users.Any())
            {
                var users = new List<User>
                {
                    new User
                    {
                        FirstName = "Igor",
                        LastName = "Junior",
                        UserName = "igor",
                        DisplayedName = "Игорь",
                        Email = "igor@test.com",
                    },
                    new User
                    {
                        FirstName = "Anna",
                        LastName = "Accountant",
                        UserName = "anna",
                        DisplayedName = "Анна",
                        Email = "anna@test.com",
                    },
                    new User
                    {
                        FirstName = "Admin",
                        LastName = "System",
                        UserName = "admin",
                        DisplayedName = "Администратор",
                        Email = "admin@test.com",
                    },
                };

                foreach (var user in users)
                {
                    await userManager.CreateAsync(user, "Pa$$w0rd");
                    await userManager.AddToRoleAsync(
                        user,
                        user.UserName == "admin"
                            ? "Admin"
                            : (user.UserName == "anna" ? "Senior_Accountant" : "Junior_Accountant")
                    );
                }
            }

            // 2. Service Reference (Based on your Excel "Справочник")
            if (!context.ServiceReferences.Any())
            {
                var serviceRefs = new List<ServiceReference>
                {
                    new ServiceReference
                    {
                        Name = "СНТ (Сопроводительная накладная)",
                        BasePrice = 5000,
                        AffectsNdsThreshold = true,
                        CreatedBy = "Seed",
                    },
                    new ServiceReference
                    {
                        Name = "ЭАВР (Акт выполненных работ)",
                        BasePrice = 3000,
                        AffectsNdsThreshold = true,
                        CreatedBy = "Seed",
                    },
                    new ServiceReference
                    {
                        Name = "Приём на работу сотрудника",
                        BasePrice = 7000,
                        AffectsNdsThreshold = false,
                        CreatedBy = "Seed",
                    },
                    new ServiceReference
                    {
                        Name = "Разноска выписки банка (Каспи/БЦК)",
                        BasePrice = 10000,
                        AffectsNdsThreshold = false,
                        CreatedBy = "Seed",
                    },
                    new ServiceReference
                    {
                        Name = "910.00 Упрощенная декларация",
                        BasePrice = 25000,
                        AffectsNdsThreshold = true,
                        CreatedBy = "Seed",
                    },
                    new ServiceReference
                    {
                        Name = "100.00 Декларация по КПН",
                        BasePrice = 55000,
                        AffectsNdsThreshold = true,
                        CreatedBy = "Seed",
                    },
                    new ServiceReference
                    {
                        Name = "Стат отчет 1-Т",
                        BasePrice = 8000,
                        AffectsNdsThreshold = false,
                        CreatedBy = "Seed",
                    },
                    new ServiceReference
                    {
                        Name = "Регистрация в ИС ЭСФ",
                        BasePrice = 15000,
                        AffectsNdsThreshold = false,
                        CreatedBy = "Seed",
                    },
                };
                context.ServiceReferences.AddRange(serviceRefs);
                await context.SaveChangesAsync();
            }

            // 3. Clients Generation (30 clients)
            if (!context.Clients.Any())
            {
                var services = await context.ServiceReferences.ToListAsync();
                var allUsers = await userManager.Users.ToListAsync();
                var random = new Random();

                string[] companyNames =
                {
                    "Alash Provision",
                    "Business Networking",
                    "Ainalayin",
                    "Buratino & Co",
                    "Zharyk NRG",
                    "Alpha Group",
                    "KazRefTrans",
                    "Impec Finance",
                    "Green Valley",
                    "Nomad Logistics",
                };
                string[] regimes = { "ОУР", "Упрощенка", "Розничный налог" };
                string[] riskLevels = { "Low", "Medium", "High" };

                for (int i = 1; i <= 30; i++)
                {
                    var responsibleUser = allUsers[random.Next(allUsers.Count)];
                    var regime = regimes[random.Next(regimes.Length)];
                    var companyName = companyNames[random.Next(companyNames.Length)] + " " + i;
                    // Генерируем случайный долг для Dashboard ("Хвосты")
                    decimal initialDebt = i % 4 == 0 ? random.Next(5000, 50000) : 0;

                    var client = new Client
                    {
                        FirstName = companyName,
                        LastName = i % 3 == 0 ? "ТОО" : "ИП",
                        BinIin = (100000000000 + random.NextInt64(899999999999)).ToString(), // Random 12 digits
                        Address = $"г. Алматы, Район {random.Next(1, 8)}, дом {i}",
                        TaxRegime = regime,
                        NdsStatus = i % 5 == 0 ? "Плательщик НДС" : "Не плательщик",
                        TaxRiskLevel = riskLevels[random.Next(riskLevels.Length)],
                        Oked = random.Next(10000, 99999).ToString(),
                        EmployeesCount = random.Next(1, 50),
                        EcpExpiryDate = DateTime.UtcNow.AddDays(random.Next(10, 300)),
                        ResponsiblePersonContact = responsibleUser.UserName,
                        TotalDebt = initialDebt,
                        BankManagerContact =
                            "+7 707 "
                            + random.Next(100, 999)
                            + " "
                            + random.Next(10, 99)
                            + " "
                            + random.Next(10, 99),
                        EcpPassword = "EcpPassword" + i,
                        EsfPassword = "EsfSecret" + i,
                        BankingPasswords = "BankAuth" + i,
                        ManagerNotes = "Auto-generated seed client",
                        CreatedBy = "Seed",
                        CreatedAt = DateTime.UtcNow,

                        CurrentTariff = new ClientTariff
                        {
                            MonthlyFee = regime == "ОУР" ? 250000 : 75000,
                            OperationsLimit = regime == "ОУР" ? 1000 : 200,
                            CommunicationMinutesLimit = 300,
                            ContractDate = DateTime.UtcNow.AddMonths(-random.Next(1, 12)),
                            IsActive = true,
                            CreatedBy = "Seed",
                            CreatedAt = DateTime.UtcNow,
                        },
                    };

                    // Add Transactions for each client
                    for (int t = 0; t < 5; t++)
                    {
                        var srv = services[random.Next(services.Count)];
                        bool isExtra = srv.IsExtraService || t > 5; // Делаем часть услуг разовыми
                        decimal extraAmount = isExtra ? srv.BasePrice : 0;
                        client.Transactions.Add(
                            new Transaction
                            {
                                Date = DateTime.UtcNow.AddDays(-random.Next(0, 28)), // Текущий месяц для Dashboard
                                ServiceId = srv.Id,
                                ServiceType = srv.ServiceType,
                                IsExtraService = isExtra,
                                ExtraServiceAmount = extraAmount,
                                OperationsCount = 1,
                                ActualTimeMinutes = random.Next(10, 60),
                                CommunicationTimeMinutes = random.Next(0, 15),
                                Status = "Completed",
                                PerformerName = responsibleUser.DisplayedName,
                                CreatedBy = "Seed",
                                CreatedAt = DateTime.UtcNow,
                            }
                        );
                    }

                    context.Clients.Add(client);
                }
                await context.SaveChangesAsync();

                // 4. Tasks for random clients
                var someClients = await context.Clients.Take(10).ToListAsync();
                foreach (var c in someClients)
                {
                    context.Tasks.Add(
                        new UserTask
                        {
                            Title = $"Проверить оплату хвостов {c.FirstName}",
                            Deadline = DateTime.UtcNow.AddDays(random.Next(-2, 5)),
                            IsCompleted = false,
                            ClientId = c.Id,
                            CreatedBy = "Seed",
                            CreatedAt = DateTime.UtcNow,
                        }
                    );
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
