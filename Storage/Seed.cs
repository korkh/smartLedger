using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Storage
{
    public class Seed
    {
        public static async Task SeedData(DataContext context, UserManager<User> userManager)
        {
            // 1. Create Users (Managers)
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
                        EmailConfirmed = true,
                        Position = "Junior Accountant",
                    },
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
                    // Roles should be created in a separate Identity Seed or check if they exist
                    if (user.UserName == "admin")
                        await userManager.AddToRoleAsync(user, "Admin");
                    else if (user.UserName == "anna")
                        await userManager.AddToRoleAsync(user, "Senior_Accountant");
                    else
                        await userManager.AddToRoleAsync(user, "Junior_Accountant");
                }
            }

            // 2. Service Directory
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

            // 3. Create 20 Clients with Tariffs and Transactions
            if (!context.Clients.Any())
            {
                var serviceList = await context.ServiceReferences.ToListAsync();
                var clients = new List<Client>();

                for (int i = 1; i <= 20; i++)
                {
                    var responsible = i % 2 == 0 ? "anna" : "igor";

                    var client = new Client
                    {
                        // Id will be generated automatically as Guid
                        FirstName = $"ClientFirstName_{i}",
                        LastName = $"Company_{i}",
                        BinIin = $"1234567890{i:D2}",
                        TaxRegime = i % 2 == 0 ? "УР" : "ОУР",
                        NdsStatus = i > 15 ? "Taxpayer" : "Non-taxpayer",
                        ResponsiblePersonContact = responsible,
                        Address = $"Street {i}, Oslo, Norway",
                        CurrentTariff = new ClientTariff
                        {
                            MonthlyFee = 30000,
                            OperationsLimit = 50,
                            CommunicationMinutesLimit = 120,
                            ContractDate = DateTime.UtcNow.AddMonths(-1),
                        },
                    };

                    // Generate transactions for each client
                    for (int t = 1; t <= 5; t++)
                    {
                        var service = serviceList[t % serviceList.Count];
                        client.Transactions.Add(
                            new Transaction
                            {
                                Date = DateTime.UtcNow.AddDays(-t * 2),
                                OperationsCount = 1,
                                ActualTimeMinutes = service.StandardTimeMinutes,
                                CommunicationTimeMinutes = 10,
                                ExtraServiceAmount = i > 10 ? 5000 : 0,
                                Status = "Completed",
                                ServiceId = service.Id, // Link by Guid
                                PerformerName = responsible,
                            }
                        );
                    }
                    clients.Add(client);
                }

                context.Clients.AddRange(clients);
                await context.SaveChangesAsync();
            }
        }
    }
}
