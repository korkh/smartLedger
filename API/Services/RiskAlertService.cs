using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Email;

public class RiskAlertService : IRiskAlertService
{
    private readonly EmailSender _email;

    public RiskAlertService(EmailSender email)
    {
        _email = email;
    }

    public async Task CheckAndNotifyAsync(Client client, RiskAnalysisResult risk)
    {
        if (risk.RiskLevel == "High" || risk.RiskLevel == "Critical")
        {
            await _email.SendEmailAsync(
                client.Internal.ResponsiblePersonContact,
                $"⚠️ Высокий риск по клиенту {client.FirstName} {client.LastName}",
                $"Уровень риска: {risk.RiskLevel}\n" + $"Рекомендации: {risk.Recommendations}"
            );
        }
    }
}
