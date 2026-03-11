"use client";

import AiInsightCard from "@/app/components/dashboard/AiInsightCard";
import { useDashboard } from "@/app/hooks/useDashboard";
import { useDashboardStore } from "@/app/hooks/useDashboardStore";
import { Badge, Card, Progress, Tooltip } from "flowbite-react";
import { use } from "react";
import {
  HiClock,
  HiCurrencyDollar,
  HiDocumentText,
  HiExclamationCircle,
  HiIdentification,
} from "react-icons/hi";

/**
 * Основная страница аналитики клиента.
 * Включает финансовые показатели, лимиты операций и статус комлпаенса.
 */
export default function DashboardPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  useDashboard(id);
  const { data, isLoading } = useDashboardStore();

  if (isLoading)
    return (
      <div className="flex justify-center p-10">Загрузка аналитики...</div>
    );
  if (!data) return <div className="p-6 text-red-500">Данные не найдены</div>;

  // Расчет цвета для ЭЦП (предупреждение за 30 дней)
  const ecpColor =
    data.daysUntilEcpExpires < 14
      ? "failure"
      : data.daysUntilEcpExpires < 30
        ? "warning"
        : "success";

  console.log(data);

  return (
    <div className="p-6 space-y-6 bg-gray-50 dark:bg-gray-900 min-h-screen">
      {/* Шапка: Имя и Статусы */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-3xl font-extrabold text-gray-900 dark:text-white">
            {data.firstName} {data.lastName}
          </h1>
          <p className="text-gray-500 dark:text-gray-400">
            Режим: {data.taxRegime}
          </p>
        </div>
        <div className="flex gap-2">
          <Badge size="lg" color={ecpColor} icon={HiClock}>
            ЭЦП: {data.daysUntilEcpExpires} дн.
          </Badge>
          <Badge
            size="lg"
            color={data.taxRiskLevel === "High" ? "failure" : "success"}
          >
            Риск: {data.taxRiskLevel}
          </Badge>
        </div>
      </div>

      {/* AI Аналитика */}
      <AiInsightCard />

      {/* Финансовый блок и Хвосты */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card className="bg-linear-to-br from-blue-50 to-white">
          <div className="flex items-center justify-between">
            <h5 className="text-sm font-medium text-gray-500 uppercase">
              К оплате (Тариф)
            </h5>
            <HiCurrencyDollar className="text-blue-500 text-2xl" />
          </div>
          <div className="mt-2 text-2xl font-bold">
            {data.tariffAmount.toLocaleString()} ₸
          </div>
        </Card>

        <Card className="bg-linear-to-br from-orange-50 to-white">
          <div className="flex items-center justify-between">
            <h5 className="text-sm font-medium text-gray-500 uppercase">
              Доп. услуги (Хвосты)
            </h5>
            <HiExclamationCircle className="text-orange-500 text-2xl" />
          </div>
          <div className="mt-2 text-2xl font-bold text-orange-600">
            +{(data.extraServicesAmount ?? 0).toLocaleString()} ₸
          </div>
          <p className="text-xs text-gray-400">
            Начислено сверх пакета в текущем месяце
          </p>
        </Card>

        <Card className="bg-linear-to-br from-red-50 to-white border-red-100">
          <div className="flex items-center justify-between">
            <h5 className="text-sm font-medium text-gray-500 uppercase">
              Итого к оплате
            </h5>
            <HiIdentification className="text-red-500 text-2xl" />
          </div>
          <div className="mt-2 text-2xl font-bold text-red-700">
            {(data.totalToPay ?? 0).toLocaleString()} ₸
          </div>
        </Card>
      </div>

      {/* Прогресс-бары: НДС и Операции */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card>
          <div className="flex justify-between items-center mb-4">
            <h5 className="text-lg font-bold">Порог НДС (30 000 МРП)</h5>
            <Tooltip content="Следите за оборотом, чтобы вовремя встать на учет по НДС">
              <HiExclamationCircle className="text-gray-400 cursor-help" />
            </Tooltip>
          </div>
          <div className="flex justify-between mb-2">
            <span className="text-sm font-medium">
              {data.currentYearTurnover.toLocaleString()} / 30 000 МРП
            </span>
            <span className="text-sm font-medium">
              {data.ndsProgressPercentage.toFixed(1)}%
            </span>
          </div>
          <Progress
            progress={data.ndsProgressPercentage}
            color={data.ndsProgressPercentage > 85 ? "red" : "blue"}
            size="lg"
          />
        </Card>

        <Card>
          <h5 className="text-lg font-bold mb-4">Лимит операций по тарифу</h5>
          <div className="flex justify-between mb-2">
            <span className="text-sm font-medium">
              Использовано: {data.operationsActual} из {data.operationsLimit}
            </span>
            <span className="text-sm font-medium">
              {Math.round((data.operationsActual / data.operationsLimit) * 100)}
              %
            </span>
          </div>
          <Progress
            progress={(data.operationsActual / data.operationsLimit) * 100}
            color={
              data.operationsActual > data.operationsLimit ? "red" : "yellow"
            }
            size="lg"
          />
          {data.operationsRemaining < 0 && (
            <p className="mt-2 text-xs text-red-600 font-semibold">
              Превышение на {Math.abs(data.operationsRemaining)} операций
            </p>
          )}
        </Card>
      </div>

      {/* Статистика по отчетам и кадрам */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <ReportMetric label="Стат. отчеты" value={data.statReportsCount} />
        <ReportMetric label="Налоговые (мес)" value={data.monthlyTaxReports} />
        <ReportMetric label="Налоговые (кв)" value={data.quarterlyTaxReports} />
        <ReportMetric
          label="Сотрудники"
          value={data.personnelCount}
          icon={<HiDocumentText />}
        />
      </div>
    </div>
  );
}

/**
 * Вспомогательный компонент для мини-метрик
 */
function ReportMetric({
  label,
  value,
  icon,
}: {
  label: string;
  value: number;
  icon?: React.ReactNode;
}) {
  return (
    <Card className="flex flex-col items-center justify-center p-2 text-center">
      <div className="text-gray-500 text-xs mb-1 uppercase tracking-wider font-semibold">
        {label}
      </div>
      <div className="text-xl font-bold text-gray-800 dark:text-white flex items-center gap-2">
        {icon} {value}
      </div>
    </Card>
  );
}
