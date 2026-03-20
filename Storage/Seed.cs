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

            // 2. Service Reference (ГЕНЕРАТОР)
            if (!context.ServiceReferences.Any())
            {
                var random = new Random();

                // Возможные налоговые режимы
                string[] taxRegimes = { "OUR", "USN", "Patent" };

                // Возможные иконки
                string[] icons =
                {
                    "/icons/default.png",
                    "/icons/doc.png",
                    "/icons/tax.png",
                    "/icons/stat.png",
                    "/icons/hr.png",
                    "/icons/bank.png",
                    "/icons/ops.png",
                };

                var statuses = Enum.GetValues<ServiceStatus>();

                var categories = Enum.GetValues<ServiceCategory>();

                var serviceRefs = new List<ServiceReference>();

                foreach (var category in categories)
                {
                    if (category == ServiceCategory.None)
                        continue;

                    var isTax = category == ServiceCategory.TaxReport;
                    var isStat = category == ServiceCategory.StatisticalReport;

                    var service = new ServiceReference
                    {
                        Id = Guid.NewGuid(),
                        Name = category.ToString(),
                        Description = $"Автоматически сгенерированная услуга категории {category}",
                        MonthNumber = isTax ? random.Next(1, 13) : 1,
                        PlannedOperationsPerMonth = isTax
                            ? random.Next(1, 10)
                            : random.Next(20, 200),
                        PlannedMinutesPerMonth = isTax
                            ? random.Next(60, 300)
                            : random.Next(100, 900),

                        Status = statuses[random.Next(statuses.Length)],
                        StandardTimeMinutes = isTax ? random.Next(20, 90) : random.Next(5, 30),
                        BasePrice = isTax ? random.Next(15000, 60000) : random.Next(3000, 15000),
                        DefaultPerformerName = isTax ? "Tax Specialist" : "Accountant",
                        Category = category,
                        ApplicableTaxRegimes = string.Join(
                            ",",
                            taxRegimes.Where(_ => random.Next(0, 2) == 1)
                        ),
                        AffectsNdsThreshold = category == ServiceCategory.Snt || isTax,
                        IsExtraService = random.Next(0, 4) == 1,
                        IconUrl = icons[random.Next(icons.Length)],
                        CreatedBy = "Seed",
                        CreatedAt = DateTime.UtcNow,
                    };

                    serviceRefs.Add(service);
                }

                context.ServiceReferences.AddRange(serviceRefs);
                await context.SaveChangesAsync();
            }

            // 3. Clients generation
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

                string[] riskLevels = { "Low", "Medium", "High" };

                string[] personalNotes =
                {
                    "Предпочитает общение в WhatsApp",
                    "Не любит звонки, только текст",
                    "Просит отправлять отчёты заранее",
                    "Часто задерживает документы",
                    "Нуждается в напоминаниях о сроках",
                };

                string[] strategicNotes =
                {
                    "Планирует переход на НДС в следующем году",
                    "Рассматривает расширение штата",
                    "Планирует открыть филиал",
                    "Готовится к автоматизации процессов",
                };

                string[] managerNotes =
                {
                    "Нуждается в повышенном внимании",
                    "Просит еженедельные отчёты",
                    "Часто меняет требования",
                    "Редко выходит на связь",
                };

                var taxRegimes = Enum.GetValues<TaxRegime>();

                for (int i = 1; i <= 30; i++)
                {
                    var responsibleUser = allUsers[random.Next(allUsers.Count)];
                    var regime = taxRegimes[random.Next(taxRegimes.Length)];
                    var companyName = companyNames[random.Next(companyNames.Length)] + " " + i;

                    var clientId = Guid.NewGuid();

                    var client = new Client
                    {
                        Id = clientId,
                        FirstName = companyName,
                        LastName = i % 3 == 0 ? "ТОО" : "ИП",
                        BinIin = (100000000000 + random.NextInt64(899999999999)).ToString(),
                        Address = $"г. Алматы, Район {random.Next(1, 8)}, дом {i}",
                        TaxRegime = regime,
                        NdsStatus = i % 4 == 0 ? "Плательщик НДС" : "Не плательщик",
                        TaxRiskLevel = riskLevels[random.Next(riskLevels.Length)],
                        Oked = random.Next(10000, 99999).ToString(),
                        EmployeesCount = random.Next(5, 50),
                        EcpExpiryDate = DateTime.UtcNow.AddDays(random.Next(10, 300)),
                        TotalDebt = 0,
                        CreatedBy = "Seed",
                        CreatedAt = DateTime.UtcNow,
                    };

                    // Level 2 – Internal
                    client.Internal = new ClientInternal
                    {
                        ClientId = clientId,
                        ResponsiblePersonContact = responsibleUser.UserName,
                        BankManagerContact =
                            "+7 707 "
                            + random.Next(100, 999)
                            + " "
                            + random.Next(10, 99)
                            + " "
                            + random.Next(10, 99),
                        ManagerNotes = managerNotes[random.Next(managerNotes.Length)],
                        CreatedBy = "Seed",
                        CreatedAt = DateTime.UtcNow,
                    };

                    // Level 3 – Sensitive
                    client.Sensitive = new ClientSensitive
                    {
                        ClientId = clientId,
                        EcpPassword = "EcpPassword" + i,
                        EsfPassword = "EsfSecret" + i,
                        BankingPasswords = "BankAuth" + i,
                        StrategicNotes = strategicNotes[random.Next(strategicNotes.Length)],
                        PersonalInfo = personalNotes[random.Next(personalNotes.Length)],
                        CreatedBy = "Seed",
                        CreatedAt = DateTime.UtcNow,
                    };

                    // Tariff
                    var tariff = new ClientTariff
                    {
                        Id = Guid.NewGuid(),
                        ClientId = clientId,
                        Package = TariffPackage.Standard, // или что у тебя там по умолчанию
                        MonthlyFee = regime == TaxRegime.OUR ? 250000 : 75000,
                        TailAmount = 0,
                        OperationsLimit = regime == TaxRegime.OUR ? 500 : 100,
                        CommunicationMinutesLimit = 300,
                        StatisticalReportsLimit = 12,
                        MonthlyTaxReportsLimit = 12,
                        QuarterlyTaxReportsLimit = 4,
                        SemiAnnualTaxReportsLimit = 2,
                        AnnualTaxReportsLimit = 1,
                        EmployeeCountLimit = 50,
                        IncludesHR = true,
                        IncludesMonthlyReports = true,
                        IncludesQuarterlyReports = true,
                        IncludesSemiAnnualReports = true,
                        IncludesAnnualReports = true,
                        ContractDate = DateTime.UtcNow.AddMonths(-random.Next(1, 12)),
                        IsActive = true,
                        CreatedBy = "Seed",
                        CreatedAt = DateTime.UtcNow,
                    };

                    client.ClientTariffs.Add(tariff);

                    // Transactions + TariffHistory
                    decimal calculatedDebt = 0;
                    bool isDebtor = random.Next(1, 100) <= 40;

                    var now = DateTime.UtcNow;

                    for (int monthOffset = 0; monthOffset < 6; monthOffset++)
                    {
                        var targetDate = now.AddMonths(-monthOffset);
                        int year = targetDate.Year;
                        int month = targetDate.Month;

                        int monthlyTCount = random.Next(2, 5);

                        int usedOps = 0;
                        int usedMinutes = 0;
                        decimal extraServicesAmount = 0;
                        int overusedOps = 0;
                        int overusedMinutes = 0;
                        decimal overusedOpsCost = 0;
                        decimal overusedMinutesCost = 0;

                        for (int t = 0; t < monthlyTCount; t++)
                        {
                            var srv = services[random.Next(services.Count)];
                            bool isExtra = t % 3 == 0;
                            decimal extraAmount = isExtra ? srv.BasePrice : 0;

                            if (isDebtor)
                                calculatedDebt += extraAmount;

                            int ops = random.Next(1, 10);
                            int mins = random.Next(15, 120);

                            usedOps += ops;
                            usedMinutes += mins;
                            if (isExtra)
                                extraServicesAmount += extraAmount;

                            client.Transactions.Add(
                                new Transaction
                                {
                                    Id = Guid.NewGuid(),
                                    ClientId = clientId,
                                    Date = targetDate.AddDays(-random.Next(1, 25)),
                                    ServiceId = srv.Id,
                                    Category = srv.Category,
                                    IsExtraService = isExtra,
                                    ExtraServiceAmount = extraAmount,
                                    NdsBaseAmount = isExtra ? 0 : srv.BasePrice,
                                    OperationsCount = ops,
                                    ActualTimeMinutes = mins,
                                    Status = "Completed",
                                    PerformerName = responsibleUser.DisplayedName,
                                    CreatedBy = "Seed",
                                    CreatedAt = DateTime.UtcNow,
                                }
                            );
                        }

                        // простая модель перерасхода
                        int opsLimit = tariff.TotalOperationsLimit;
                        int minLimit = tariff.TotalMinutesLimit;

                        if (usedOps > opsLimit)
                        {
                            overusedOps = usedOps - opsLimit;
                            overusedOpsCost = overusedOps * 500; // условная цена за операцию
                        }

                        if (usedMinutes > minLimit)
                        {
                            overusedMinutes = usedMinutes - minLimit;
                            overusedMinutesCost = overusedMinutes * 50; // условная цена за минуту
                        }

                        int riskScore = 0;
                        if (overusedOps > 0 || overusedMinutes > 0)
                            riskScore += 20;
                        if (calculatedDebt > 300_000)
                            riskScore += 30;

                        string riskLevel;
                        string riskColor;

                        if (riskScore <= 25)
                        {
                            riskLevel = "Low";
                            riskColor = "Green";
                        }
                        else if (riskScore <= 50)
                        {
                            riskLevel = "Medium";
                            riskColor = "Yellow";
                        }
                        else if (riskScore <= 75)
                        {
                            riskLevel = "High";
                            riskColor = "Orange";
                        }
                        else
                        {
                            riskLevel = "Critical";
                            riskColor = "Red";
                        }

                        string riskRecommendations =
                            riskScore == 0
                                ? "Риски минимальны."
                                : "Проверить перерасход и задолженность, обсудить условия с клиентом.";

                        var history = new TariffHistory
                        {
                            Id = Guid.NewGuid(),
                            ClientId = clientId,
                            TariffId = tariff.Id,
                            Year = year,
                            Month = month,
                            UsedOperations = usedOps,
                            UsedMinutes = usedMinutes,
                            OverusedOperations = overusedOps,
                            OverusedMinutes = overusedMinutes,
                            OverusedOperationsCost = overusedOpsCost,
                            OverusedMinutesCost = overusedMinutesCost,
                            TariffAmount = tariff.MonthlyFee,
                            ExtraServicesAmount = extraServicesAmount,
                            TotalToPay =
                                tariff.MonthlyFee
                                + extraServicesAmount
                                + overusedOpsCost
                                + overusedMinutesCost,
                            RiskScore = riskScore,
                            RiskLevel = riskLevel,
                            RiskColor = riskColor,
                            RiskRecommendations = riskRecommendations,
                            CreatedBy = "Seed",
                            CreatedAt = DateTime.UtcNow,
                        };

                        context.TariffHistories.Add(history);
                    }

                    client.TotalDebt = calculatedDebt;

                    context.Clients.Add(client);
                }

                await context.SaveChangesAsync();

                // Tasks
                var someClients = await context.Clients.Take(10).ToListAsync();
                foreach (var c in someClients)
                {
                    context.Tasks.Add(
                        new UserTask
                        {
                            Id = Guid.NewGuid(),
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
