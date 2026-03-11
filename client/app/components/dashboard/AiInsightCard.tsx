"use client";

import { useDashboardStore } from "@/app/hooks/useDashboardStore";
import { Card, Spinner } from "flowbite-react";
import { HiSparkles } from "react-icons/hi";
export default function AiInsightCard() {
  const { data, isLoading } = useDashboardStore();

  if (isLoading) return <Spinner aria-label="Анализ данных ИИ..." />;
  if (!data?.aiInsight) return null;

  return (
    <Card className="bg-linear-to-br from-blue-50 to-indigo-50 dark:from-gray-800 dark:to-gray-700 border-l-4 border-l-blue-500">
      <div className="flex items-start gap-4">
        <div className="p-2 bg-blue-100 dark:bg-blue-900 rounded-lg">
          <HiSparkles className="w-6 h-6 text-blue-600 dark:text-blue-300" />
        </div>
        <div>
          <h5 className="text-sm font-bold tracking-tight text-gray-900 dark:text-white uppercase">
            Умный помощник SmartLedger
          </h5>
          <p className="mt-1 text-sm text-gray-700 dark:text-gray-300 italic">
            "{data.aiInsight}"
          </p>
        </div>
      </div>
    </Card>
  );
}
