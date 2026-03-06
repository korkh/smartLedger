using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Storage
{
    public class Seed
    {
        public static async Task SeedData(DataContext context, UserManager<User> userManager)
        {
            // 1. Создание пользователей (Менеджеров)
            if (!userManager.Users.Any())
            {
                var users = new List<User>
                {
                    new User
                    {
                        FirstName = "Anna",
                        LastName = "Accountant",
                        UserName = "anna",
                        Email = "anna@test.com",
                        EmailConfirmed = true,
                        Position = "Senior Accountant",
                    },
                    new User
                    {
                        FirstName = "Igor",
                        LastName = "Junior",
                        UserName = "igor",
                        Email = "igor@test.com",
                        EmailConfirmed = true,
                        Position = "Junior Accountant",
                    },
                    new User
                    {
                        FirstName = "Admin",
                        LastName = "System",
                        UserName = "admin",
                        Email = "admin@test.com",
                        EmailConfirmed = true,
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

            // 2. Справочник услуг
            if (!context.ServiceReferences.Any())
            {
                var services = new List<ServiceReference>
                {
                    new ServiceReference
                    {
                        Name = "Consultation",
                        BasePrice = 5000,
                        AffectsNdsThreshold = false,
                    },
                    new ServiceReference
                    {
                        Name = "Tax Filing",
                        BasePrice = 15000,
                        AffectsNdsThreshold = true,
                    },
                    new ServiceReference
                    {
                        Name = "Bookkeeping",
                        BasePrice = 25000,
                        AffectsNdsThreshold = true,
                    },
                };
                context.ServiceReferences.AddRange(services);
                await context.SaveChangesAsync();
            }

            // 3. Создание 20 клиентов
            if (!context.Clients.Any())
            {
                var serviceList = context.ServiceReferences.ToList();
                for (int i = 1; i <= 20; i++)
                {
                    var client = new Client
                    {
                        FirstName = $"ClientFirstName_{i}",
                        LastName = $"Company_{i}",
                        BinIin = $"1234567890{i:D2}",
                        TaxRegime = i % 2 == 0 ? "УР" : "ОУР",
                        NdsStatus = i > 15 ? "Плательщик НДС" : "Не плательщик", // 5 плательщиков для теста
                        ResponsiblePersonContact = i % 2 == 0 ? "anna" : "igor", // Распределяем между менеджерами
                        CurrentTariff = new ClientTariff
                        {
                            MonthlyFee = 30000,
                            OperationsLimit = 50,
                            CommunicationMinutesLimit = 120,
                            CarriedOverOperations = 0,
                            CarriedOverMinutes = 0,
                        },
                    };

                    // Генерируем транзакции для каждого клиента (имитация данных из Excel)
                    for (int t = 1; t <= 10; t++)
                    {
                        client.Transactions.Add(
                            new Transaction
                            {
                                Date = DateTime.Now.AddDays(-t * 3),
                                OperationsCount = 2,
                                CommunicationTimeMinutes = 15,
                                ExtraServiceAmount = 10000 * t, // Для проверки порога НДС
                                Status = "Завершен",
                                Service = serviceList[t % serviceList.Count],
                                PerformerName = client.ResponsiblePersonContact,
                            }
                        );
                    }
                    context.Clients.Add(client);
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
