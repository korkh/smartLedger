"use client";

import AiInsightCard from "@/app/components/AiInsightCard";
import DateFilter from "@/app/components/DateFilter";
import { DashboardFilters, useDashboard } from "@/app/hooks/useDashboard";
import { useDashboardStore } from "@/app/hooks/useDashboardStore";
import { Badge, Card, Progress, Tooltip } from "flowbite-react";
import { use, useState } from "react";
import {
  HiClock,
  HiCurrencyDollar,
  HiExclamationCircle,
  HiIdentification,
  HiTrendingUp,
  HiUsers,
} from "react-icons/hi";

export default function DashboardPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);

  const [filters, setFilters] = useState<DashboardFilters>({
    periodType: "month",
    month: new Date().getMonth() + 1,
    year: new Date().getFullYear(),
  });

  useDashboard(id, filters);

  const handlePeriodChange = (newFilters: DashboardFilters) => {
    setFilters(newFilters);
  };

  const { data, isLoading } = useDashboardStore();

  console.log(data);

  if (isLoading)
    return (
      <div className="flex justify-center p-10 text-gray-500">
        Загрузка аналитики...
      </div>
    );
  if (!data)
    return (
      <div className="p-6 text-red-500 text-center">Данные не найдены</div>
    );

  // Логика цвета для ЭЦП
  const ecpColor =
    data.daysUntilEcpExpires < 14
      ? "failure"
      : data.daysUntilEcpExpires < 30
        ? "warning"
        : "success";

  return (
    <div className="p-6 space-y-6 bg-gray-50 dark:bg-gray-900 min-h-screen">
      {/* Шапка: Имя и Статусы */}
      <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
        <div>
          <h1 className="text-3xl font-extrabold text-gray-900 dark:text-white">
            {data.firstName} {data.lastName}
          </h1>
          <p className="text-gray-500 dark:text-gray-400 font-medium">
            Режим: <span className="text-blue-600">{data.taxRegime}</span>
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

      <div className="flex justify-between items-center">
        <DateFilter filters={filters} onPeriodChange={handlePeriodChange} />
      </div>

      <AiInsightCard />

      {/* Финансовый блок: Тариф, Хвосты, Долг */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <Card className="bg-white border-l-4 border-blue-500">
          <div className="flex items-center justify-between">
            <h5 className="text-xs font-bold text-gray-400 uppercase">
              Тариф за месяц
            </h5>
            <HiCurrencyDollar className="text-blue-500 text-xl" />
          </div>
          <div className="mt-1 text-2xl font-black text-gray-900">
            {data.tariffAmount.toLocaleString()} ₸
          </div>
        </Card>

        <Card className="bg-white border-l-4 border-orange-400">
          <div className="flex items-center justify-between">
            <h5 className="text-xs font-bold text-gray-400 uppercase">
              Доп. услуги (Хвосты)
            </h5>
            <HiTrendingUp className="text-orange-400 text-xl" />
          </div>
          <div className="mt-1 text-2xl font-black text-orange-600">
            +{(data.monthlyExtraServicesAmount ?? 0).toLocaleString()} ₸
          </div>
        </Card>

        <Card className="bg-slate-50 border-l-4 border-gray-800">
          <div className="flex items-center justify-between">
            <h5 className="text-xs font-bold text-gray-400 uppercase">
              Общая задолженность
            </h5>
            <HiExclamationCircle className="text-gray-700 text-xl" />
          </div>
          <div className="mt-1 text-2xl font-black text-gray-800">
            {data.totalOutstandingDebt.toLocaleString()} ₸
          </div>
        </Card>

        <Card className="bg-red-50 border-l-4 border-red-600">
          <div className="flex items-center justify-between">
            <h5 className="text-xs font-bold text-red-400 uppercase">
              Итого к оплате (Месяц)
            </h5>
            <HiIdentification className="text-red-600 text-xl" />
          </div>
          <div className="mt-1 text-2xl font-black text-red-700">
            {(data.totalToPay ?? 0).toLocaleString()} ₸
          </div>
        </Card>
      </div>

      {/* Прогресс-бары: НДС и Операции */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card>
          <div className="flex justify-between items-center mb-4">
            <div>
              <h5 className="text-lg font-bold">Оборот по НДС</h5>
              <p className="text-xs text-gray-400">
                Нарастающим итогом за {filters.year} год
              </p>
            </div>
            <Tooltip
              content={`Порог НДС: ${data.ndsThreshold.toLocaleString()} ₸`}
            >
              <HiExclamationCircle className="text-gray-400 cursor-help" />
            </Tooltip>
          </div>

          <div className="flex justify-between mb-2 items-end">
            <span className="text-sm font-bold text-gray-700">
              {data.currentYearTurnover.toLocaleString()} ₸
            </span>
            <span className="text-sm font-black text-blue-700">
              {data.ndsProgressPercentage?.toFixed(1) ?? "0.0"}%
            </span>
          </div>

          <Progress
            progress={data.ndsProgressPercentage || 0}
            color={data.ndsProgressPercentage > 85 ? "red" : "blue"}
            size="lg"
          />

          <div className="mt-2 flex justify-between text-[10px] text-gray-400 uppercase font-bold">
            <span>0 ₸</span>
            <span>Порог: {data.ndsThreshold.toLocaleString()} ₸</span>
          </div>
        </Card>

        <Card>
          <div className="flex justify-between items-center mb-4">
            <div>
              <h5 className="text-lg font-bold">Лимит операций</h5>
              <p className="text-xs text-gray-400">
                Включено в пакет + перенос
              </p>
            </div>
          </div>
          <div className="flex justify-between mb-2 items-end">
            <span className="text-sm font-bold text-gray-700">
              Факт: {data.operationsActual} / {data.operationsLimit}
            </span>
            <span
              className={`text-sm font-black ${data.operationsRemaining < 0 ? "text-red-600" : "text-green-600"}`}
            >
              Остаток: {data.operationsRemaining}
            </span>
          </div>
          <Progress
            progress={Math.min(
              100,
              (data.operationsActual / data.operationsLimit) * 100,
            )}
            color={
              data.operationsActual > data.operationsLimit ? "red" : "yellow"
            }
            size="lg"
          />
          {data.operationsRemaining < 0 && (
            <p className="mt-2 text-xs text-red-600 font-bold flex items-center gap-1">
              <HiExclamationCircle /> Сверх лимита:{" "}
              {Math.abs(data.operationsRemaining)} оп.
            </p>
          )}
        </Card>
      </div>

      {/* Блок отчетности и задач */}
      <div className="grid grid-cols-2 md:grid-cols-6 gap-4">
        <ReportMetric label="Стат. отчеты" value={data.statReportsCount} />
        <ReportMetric label="Налоговые (мес)" value={data.monthlyTaxReports} />
        <ReportMetric label="Налоговые (кв)" value={data.quarterlyTaxReports} />
        <ReportMetric
          label="Налоговые (полугод)"
          value={data.semiAnnualTaxReports}
        />
        <ReportMetric label="Налоговые (год)" value={data.annualTaxReports} />
        <ReportMetric
          label="Сотрудники"
          value={data.personnelCount}
          icon={<HiUsers />}
        />
      </div>
    </div>
  );
}

function ReportMetric({
  label,
  value,
  icon,
  highlight = false,
}: {
  label: string;
  value: number;
  icon?: React.ReactNode;
  highlight?: boolean;
}) {
  return (
    <Card
      className={`flex flex-col items-center justify-center p-2 text-center ${highlight ? "bg-orange-50 border-orange-100" : ""}`}
    >
      <div className="text-gray-400 text-[10px] mb-1 uppercase tracking-tighter font-black">
        {label}
      </div>
      <div className="text-2xl font-black text-gray-800 dark:text-white flex items-center gap-2">
        {icon} {value}
      </div>
    </Card>
  );
}
