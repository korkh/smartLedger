import { auth } from "@/auth";
import { redirect } from "next/navigation";
import {
  HiOutlineCash,
  HiOutlineLightningBolt,
  HiOutlineTrendingUp,
} from "react-icons/hi";
import { getData } from "../actions/clientsActions";
import { aggregateDashboardData } from "../actions/dashboardActions";
import ServicesPieChart from "../components/dashboard/ServicesPieChart";
import StatCard from "../components/dashboard/StatCard";

export default async function ServicesPage() {
  const session = await auth();
  const role = (session?.user as any)?.role;

  // LEVEL 2+ CHECK: Если не Senior и не Admin — уходим на главную или 403
  if (role !== "Admin" && role !== "Senior_Accountant") {
    redirect("/");
  }
  const result = await getData("?pageSize=1000");
  if (!result) return <div>Ошибка загрузки</div>;

  const { services, revenueHistory } = aggregateDashboardData(result.items);

  // Считаем общую выручку по доп. услугам из истории
  const totalExtraRevenue = revenueHistory.reduce(
    (acc, curr) => acc + curr.extra,
    0,
  );
  // Находим самую популярную услугу
  const topService = [...services].sort((a, b) => b.value - a.value)[0];

  return (
    <div className="p-6 space-y-6 bg-gray-50 dark:bg-gray-900 min-h-screen">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold dark:text-white">Аналитика услуг</h1>
      </div>

      {/* Верхние карточки с метриками услуг */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <StatCard
          title="Всего оказано услуг"
          value={services.reduce((acc, s) => acc + s.value, 0).toString()}
          icon={HiOutlineLightningBolt}
          trend="За весь период"
          color="blue"
        />
        <StatCard
          title="Выручка (Доп. услуги)"
          value={`${totalExtraRevenue.toLocaleString()} ₸`}
          icon={HiOutlineCash}
          trend="Помимо тарифов"
          color="green"
        />
        <StatCard
          title="Лидер спроса"
          value={topService?.name || "Нет данных"}
          icon={HiOutlineTrendingUp}
          trend={`${topService?.value} операций`}
          color="yellow"
        />
      </div>

      <div className="grid grid-cols-1 xl:grid-cols-3 gap-6">
        {/* График распределения */}
        <div className="xl:col-span-1 bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
          <h3 className="text-lg font-bold mb-4 dark:text-white">
            Доля услуг в портфеле
          </h3>
          <div className="h-80">
            <ServicesPieChart data={services} />
          </div>
        </div>

        {/* Таблица детализации */}
        <div className="xl:col-span-2 bg-white dark:bg-gray-800 p-6 rounded-xl shadow-sm border border-gray-200 dark:border-gray-700">
          <h3 className="text-lg font-bold mb-4 dark:text-white">
            Детальный отчет по типам
          </h3>
          <div className="overflow-x-auto">
            <table className="w-full text-left">
              <thead>
                <tr className="text-gray-400 border-b border-gray-100 dark:border-gray-700">
                  <th className="pb-3 font-medium">Наименование услуги</th>
                  <th className="pb-3 font-medium text-center">Количество</th>
                  <th className="pb-3 font-medium text-right">Популярность</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50 dark:divide-gray-700">
                {services
                  .sort((a, b) => b.value - a.value)
                  .map((service, index) => (
                    <tr
                      key={index}
                      className="hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors"
                    >
                      <td className="py-4 font-medium dark:text-gray-200">
                        {service.name}
                      </td>
                      <td className="py-4 text-center dark:text-gray-300">
                        {service.value}
                      </td>
                      <td className="py-4 text-right">
                        <div className="flex items-center justify-end gap-2">
                          <div className="w-24 bg-gray-100 dark:bg-gray-700 rounded-full h-2">
                            <div
                              className="bg-blue-500 h-2 rounded-full"
                              style={{
                                width: topService?.value
                                  ? `${(service.value / topService.value) * 100}%`
                                  : "0%",
                              }}
                            />
                          </div>
                          <span className="text-sm text-gray-500">
                            {Math.round(
                              (service.value /
                                services.reduce((acc, s) => acc + s.value, 0)) *
                                100,
                            )}
                            %
                          </span>
                        </div>
                      </td>
                    </tr>
                  ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  );
}
function getServerSession(authOptions: any) {
  throw new Error("Function not implemented.");
}
