import { fetchWrapper } from "@/lib/fetchWrapper";
import { Client, DashboardData, getServiceTypeName } from "@/Types";

export async function getAiInsightDashboard(
  id: string,
  data: DashboardData,
): Promise<{ insight: string }> {
  // Передаем id в URL, а данные в теле
  return fetchWrapper.post(`dashboard/${id}/analyze`, data);
}

// Агрегируем данные по клиентам для дашборда
export function aggregateDashboardData(clients: Client[]) {
  const topRevenue: Record<string, number> = {};
  const topOps: Record<string, number> = {};
  const serviceDistribution: Record<string, number> = {};
  const risks: Record<string, number> = { High: 0, Medium: 0, Low: 0 };
  const billingStats = { Paid: 0, Pending: 0, Overdue: 0 };

  const criticalActions: any[] = [];

  let totalMinutes = 0;
  let totalDebt = 0;
  let totalTurnover = 0;

  clients.forEach((client) => {
    const clientName = `${client.lastName} ${client.firstName}`;
    const debt = client.totalDebt || 0;
    totalDebt += debt;

    // Подсчет Критические действия
    if (debt === 0) {
      billingStats.Paid += 1;
    } else if (debt > 50000) {
      billingStats.Overdue += 1;
    } else {
      billingStats.Pending += 1;
    }

    // Считаем выручку и операции по транзакциям
    client.transactions?.forEach((t) => {
      // 1. Топ по выручке
      topRevenue[clientName] =
        (topRevenue[clientName] || 0) + (t.ndsBaseAmount || 0);

      // 2. Топ по операциям
      topOps[clientName] = (topOps[clientName] || 0) + (t.operationsCount || 0);

      // 3. Распределение услуг
      const typeName = getServiceTypeName(t.serviceType);
      serviceDistribution[typeName] = (serviceDistribution[typeName] || 0) + 1;

      // 4. Общее время
      totalMinutes += t.actualTimeMinutes || 0;

      // 5. Риск
      const level = client.taxRiskLevel || "Low";
      risks[level] = (risks[level] || 0) + 1;
    });
  });

  // Форматируем для графика
  const formattedBilling = [
    { name: "Оплачено", value: billingStats.Paid, color: "#10b981" },
    { name: "Ожидает", value: billingStats.Pending, color: "#3b82f6" },
    { name: "Просрочено", value: billingStats.Overdue, color: "#f43f5e" },
  ];

  // Форматируем для графика с привязкой цветов
  const formattedRisks = [
    { name: "Высокий", value: risks.High, color: "#ef4444" },
    { name: "Средний", value: risks.Medium, color: "#f59e0b" },
    { name: "Низкий", value: risks.Low, color: "#10b981" },
  ];

  // Форматируем для Recharts и берем Топ-15
  const formattedRevenue = Object.entries(topRevenue)
    .map(([name, value]) => ({ name, value }))
    .sort((a, b) => b.value - a.value)
    .slice(0, 15);

  const formattedOps = Object.entries(topOps)
    .map(([name, value]) => ({ name, value }))
    .sort((a, b) => b.value - a.value)
    .slice(0, 15);

  const formattedServices = Object.entries(serviceDistribution).map(
    ([name, value]) => ({ name, value }),
  );

  clients.forEach((client) => {
    const clientName = `${client.lastName} ${client.firstName}`;

    // 1. Проверка ЭЦП
    if (client.daysUntilEcpExpires <= 14) {
      criticalActions.push({
        id: `ecp-${client.id}`,
        type: client.daysUntilEcpExpires <= 3 ? "danger" : "warning",
        task: `Истекает ЭЦП (${client.daysUntilEcpExpires} дн.)`,
        client: clientName,
        category: "ecp",
        link: `/clients/${client.id}`,
      });
    }

    // 2. Проверка долга
    if (client.totalDebt > 100000) {
      criticalActions.push({
        id: `debt-${client.id}`,
        type: "warning",
        task: `Задолженность: ${client.totalDebt.toLocaleString()} ₸`,
        client: clientName,
        category: "debt",
        link: `/clients/${client.id}`,
      });
    }

    // 3. Риск уровня High (ИИ или налоговый)
    if (client.taxRiskLevel === "High") {
      criticalActions.push({
        id: `risk-${client.id}`,
        type: "danger",
        task: "Критический уровень налогового риска",
        client: clientName,
        category: "risk",
        link: `/dashboard/${client.id}`,
      });
    }
  });

  //Revenue Charts
  const monthlyData: Record<
    string,
    { name: string; base: number; extra: number }
  > = {};

  // Инициализируем последние 6 месяцев (пример)
  const monthNames = [
    "Янв",
    "Фев",
    "Мар",
    "Апр",
    "Май",
    "Июн",
    "Июл",
    "Авг",
    "Сен",
    "Окт",
    "Ноя",
    "Дек",
  ];

  clients.forEach((client) => {
    client.transactions?.forEach((t) => {
      const date = new Date(t.date);
      const monthKey = `${date.getFullYear()}-${date.getMonth()}`;
      const monthName = monthNames[date.getMonth()];

      if (!monthlyData[monthKey]) {
        monthlyData[monthKey] = { name: monthName, base: 0, extra: 0 };
      }

      if (t.isExtraService) {
        // Если это доп. услуга, берем ее стоимость
        monthlyData[monthKey].extra += t.extraServiceAmount || 0;
      } else {
        // Если это основная работа, берем либо стоимость из справочника, либо фикс
        // В твоем случае можно использовать ndsBaseAmount как доход от тарифа
        monthlyData[monthKey].base += t.ndsBaseAmount || 0;
      }
    });
  });

  // Сортируем по дате и превращаем в массив
  const formattedMonthly = Object.entries(monthlyData)
    .sort(([a], [b]) => a.localeCompare(b))
    .map(([_, val]) => val)
    .slice(-6); // Берем последние 6 месяцев

  return {
    topRevenue: formattedRevenue,
    topOps: formattedOps,
    services: formattedServices,
    riskDistribution: formattedRisks,
    criticalActions: criticalActions
      .sort((a, b) => (a.type === "danger" ? -1 : 1))
      .slice(0, 10),
    revenueHistory: formattedMonthly,
    billingStatus: formattedBilling,
    metrics: {
      totalDebt,
      totalMinutes,
      totalClients: clients.length,
    },
  };
}
