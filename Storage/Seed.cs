using Domain.Constants;
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

            // 2. Service Reference (Используем Enum ServiceCategory вместо строк)
            if (!context.ServiceReferences.Any())
            {
                var serviceRefs = new List<ServiceReference>
                {
                    new ServiceReference
                    {
                        Name = "ЭАВР / СНТ",
                        Category = ServiceCategory.Snt,
                        BasePrice = 5000,
                        AffectsNdsThreshold = true,
                        CreatedBy = "Seed",
                    },
                    new ServiceReference
                    {
                        Name = "Приём на работу / ЗП",
                        Category = ServiceCategory.Hiring,
                        BasePrice = 7000,
                        AffectsNdsThreshold = false,
                        CreatedBy = "Seed",
                    },
                    new ServiceReference
                    {
                        Name = "Разноска выписки банка",
                        Category = ServiceCategory.BankStatement,
                        BasePrice = 10000,
                        AffectsNdsThreshold = false,
                        CreatedBy = "Seed",
                    },
                    new ServiceReference
                    {
                        Name = "Месячный налоговый отчет",
                        Category = ServiceCategory.TaxReport,
                        BasePrice = 12000,
                        AffectsNdsThreshold = false,
                        CreatedBy = "Seed",
                    },
                    new ServiceReference
                    {
                        Name = "Квартальный налоговый отчет",
                        Category = ServiceCategory.,
                        BasePrice = 35000,
                        AffectsNdsThreshold = true,
                        CreatedBy = "Seed",
                    },
                    new ServiceReference
                    {
                        Name = "Полугодовой налоговый отчет",
                        Category = ServiceCategory.SemiAnnualTaxReport,
                        BasePrice = 25000,
                        AffectsNdsThreshold = true,
                        CreatedBy = "Seed",
                    },
                    new ServiceReference
                    {
                        Name = "Годовой налоговый отчет",
                        Category = ServiceCategory.AnnualTaxReport,
                        BasePrice = 60000,
                        AffectsNdsThreshold = true,
                        CreatedBy = "Seed",
                    },
                    new ServiceReference
                    {
                        Name = "Стат. отчет",
                        Category = ServiceCategory.StatReport,
                        BasePrice = 8000,
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
                    "Ainalayin",
                    "Buratino & Co",
                    "Zharyk NRG",
                    "Alpha Group",
                    "KazRefTrans",
                    "Impec Finance",
                };
                string[] regimes = { "ОУР", "Упрощенка", "Розничный налог" };
                string[] riskLevels = { "Low", "Medium", "High" };

                // Данные для личных заметок
                string[] personalNotes =
                {
                    "Предпочитает общение в WhatsApp",
                    "Доступ в Каспи через Ксению",
                    "Всегда просит акты сверки до 5 числа",
                    "Очень пунктуальный клиент",
                    "Сложный характер, согласовывать всё письменно",
                };
                string[] strategicNotes =
                {
                    "Планирует переход на НДС в следующем году",
                    "Нужна оптимизация по зарплатным налогам",
                    "Интересуется аудитом за 2024 год",
                    "Возможен переезд офиса, потребуется смена юр. адреса",
                    "Активно масштабируется",
                };
                string[] managerNotes =
                {
                    "Планирует переход на НДС в следующем году",
                    "Нужна оптимизация по зарплатным налогам",
                    "Интересуется аудитом за 2024 год",
                    "Возможен переезд офиса, потребуется смена юр. адреса",
                    "Активно масштабируется",
                };

                for (int i = 1; i <= 30; i++)
                {
                    var responsibleUser = allUsers[random.Next(allUsers.Count)];
                    var regime = regimes[random.Next(regimes.Length)];

                    var companyName = companyNames[random.Next(companyNames.Length)] + " " + i;

                    var client = new Client
                    {
                        FirstName = companyName,
                        LastName = i % 3 == 0 ? "ТОО" : "ИП",
                        BinIin = (100000000000 + random.NextInt64(899999999999)).ToString(),
                        Address = $"г. Алматы, Район {random.Next(1, 8)}, дом {i}",
                        TaxRegime = regime,
                        NdsStatus = i % 4 == 0 ? "Плательщик НДС" : "Не плательщик",
                        TaxRiskLevel = riskLevels[random.Next(riskLevels.Length)],
                        Oked = random.Next(10000, 99999).ToString(),
                        EmployeesCount = random.Next(5, 50), // Не 0
                        EcpExpiryDate = DateTime.UtcNow.AddDays(random.Next(10, 300)),
                        //Sensitive
                       Sensitive = new ClientSensitive
                        {
                StrategicNotes = strategicNotes[random.Next(strategicNotes.Length)],
                PersonalInfo = personalNotes[random.Next(personalNotes.Length)],
                EcpPassword = "EcpPassword" + i,
                EsfPassword = "EsfSecret" + i,
                BankingPasswords = "BankAuth" + i

                       },

                        // INTERNAL
            Internal = new ClientInternal
            {
                ResponsiblePersonContact = responsibleUser.UserName,
                ManagerNotes = managerNotes[random.Next(managerNotes.Length)],
                BankManagerContact =
                    "+7 707 " + random.Next(100, 999) + " " + random.Next(10, 99) + " " + random.Next(10, 99),
            },


                        CreatedBy = "Seed",
                        CreatedAt = DateTime.UtcNow,

                        CurrentTariff = new ClientTariff
            {
                MonthlyFee = regime == "ОУР" ? 250000 : 75000,
                OperationsLimit = regime == "ОУР" ? 500 : 100,
                CommunicationMinutesLimit = 300,
                ContractDate = DateTime.UtcNow.AddMonths(-random.Next(1, 12)),
                IsActive = true,
                CreatedBy = "Seed",
                CreatedAt = DateTime.UtcNow,
            },

                        Transactions = new List<Transaction>(),
                    };

                    // Генерируем транзакции
                    // Для каждого клиента создаем 8-12 транзакций, чтобы превысить лимиты
                    int transactionCount = random.Next(8, 15);
                    decimal calculatedDebt = 0;
                    // Решаем: будет ли этот клиент должником? (например, 40% вероятность долга)
                    bool isDebtor = random.Next(1, 100) <= 40;

                    for (int monthOffset = 0; monthOffset < 6; monthOffset++)
                    {
                        // Для каждого месяца создаем 2-4 транзакции
                        int monthlyTCount = random.Next(2, 5);

                        for (int t = 0; t < monthlyTCount; t++)
                        {
                            var srv = services[random.Next(services.Count)];
                            bool isExtra = t % 3 == 0; // Чуть больше доп. услуг для красоты графика
                            decimal extraAmount = isExtra ? srv.BasePrice : 0;

                            if (isDebtor)
                            {
                                calculatedDebt += extraAmount;
                            }

                            decimal ndsTurnover = srv.AffectsNdsThreshold
                                ? random.Next(500_000, 5_000_000)
                                : 0;

                            client.Transactions.Add(
                                new Transaction
                                {
                                    // Устанавливаем дату в зависимости от месяца
                                    Date = DateTime
                                        .UtcNow.AddMonths(-monthOffset)
                                        .AddDays(-random.Next(1, 25)),
                                    ServiceId = srv.Id,
                                    ServiceCategory = srv.ServiceCategory,
                                    IsExtraService = isExtra,
                                    ExtraServiceAmount = extraAmount,
                                    NdsBaseAmount = isExtra ? 0 : srv.BasePrice,
                                    OperationsCount = random.Next(1, 10),
                                    ActualTimeMinutes = random.Next(15, 120),
                                    Status = "Completed",
                                    PerformerName = responsibleUser.DisplayedName,
                                    CreatedBy = "Seed",
                                    CreatedAt = DateTime.UtcNow,
                                }
                            );
                        }
                    }

                    // Присваиваем накопленный долг клиенту
                    client.TotalDebt = calculatedDebt;

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
