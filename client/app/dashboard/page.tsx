import { Client } from "@/Types";
import { HiClock, HiExclamationCircle, HiUsers } from "react-icons/hi";
import { getData } from "../actions/clientsActions";
import { aggregateDashboardData } from "../actions/dashboardActions";
import BillingPieChart from "../components/dashboard/BillingPieChart";
import CriticalActions from "../components/dashboard/CriticalActions";
import RevenueChart from "../components/dashboard/RevenueChart";
import RiskDistribution from "../components/dashboard/RiskDistribution";
import ServicesPieChart from "../components/dashboard/ServicesPieChart";
import StatCard from "../components/dashboard/StatCard";
import TopClientsChart from "../components/dashboard/TopClientsChart";

export default async function DashboardPage() {
  const result = await getData("?pageSize=1000");
  if (!result) return <div>Ошибка загрузки данных</div>;
  const clients: Client[] = result.items || [];

  const {
    topRevenue,
    topOps,
    services,
    metrics,
    criticalActions,
    revenueHistory,
    riskDistribution,
    billingStatus,
  } = aggregateDashboardData(result.items);

  const expiringEcpCount = clients.filter(
    (c) => c.daysUntilEcpExpires < 15,
  ).length;

  return (
    <div className="p-6 space-y-6 bg-gray-50 dark:bg-gray-900 min-h-screen">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          title="Всего клиентов"
          value={metrics.totalClients.toString()}
          icon={HiUsers}
          trend="В системе"
          color="blue"
        />
        <StatCard
          title="Общий долг"
          value={`${(metrics.totalDebt / 1000000).toFixed(1)}M ₸`}
          icon={HiExclamationCircle}
          trend="Дебиторка"
          color="red"
        />
        <StatCard
          title="Время работы"
          value={`${Math.round(metrics.totalMinutes / 60)} ч.`}
          icon={HiClock}
          trend="Всего за период"
          color="green"
        />
        <StatCard
          title="ЭЦП истекает"
          value={expiringEcpCount.toString()}
          icon={HiExclamationCircle}
          trend="Срочно"
          color="yellow"
        />
      </div>

      {/* LEVEL 2: Основная аналитика */}
      <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
        <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
          <TopClientsChart
            title="Топ-15 клиентов по доходу (₸)"
            data={topRevenue}
            color="#10b981"
          />
        </div>
        <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
          <TopClientsChart
            title="Топ-15 клиентов по операциям (шт)"
            data={topOps}
            color="#3b82f6"
          />
        </div>
      </div>

      {/* LEVEL 3: Детализация услуг и времени */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Пирог услуг */}
        <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
          <h3 className="text-lg font-bold mb-4 dark:text-white">
            Количество услуг (в шт)
          </h3>
          <ServicesPieChart data={services} />
        </div>
        <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
          <h3 className="text-lg font-bold mb-4 dark:text-white">
            Налоговые риски (ИИ)
          </h3>
          <div className="h-87.5">
            <RiskDistribution data={riskDistribution} />
          </div>
        </div>
        <div className="bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
          <h3 className="text-sm font-bold mb-4 dark:text-gray-300">
            Статус оплат
          </h3>
          <div className="h-75">
            <BillingPieChart data={billingStatus} />
          </div>
        </div>
        {/* Динамика выручки (RevenueChart) */}
        <div className="lg:col-span-2 bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
          <h3 className="text-lg font-bold mb-4 dark:text-white">
            Сумма по тарифам vs Доп. услуги
          </h3>
          <div className="h-75 w-full">
            <RevenueChart data={revenueHistory} />
          </div>
        </div>
      </div>

      {/* LEVEL 4: Критические задачи */}
      <div className="bg-white dark:bg-gray-800 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
        <CriticalActions actions={criticalActions} />
      </div>
    </div>
  );
}
