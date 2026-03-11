"use client";

import { getAiInsight } from "@/app/actions/dashboardActions"; // Импортируем новый экшен
import { useDashboardStore } from "@/app/hooks/useDashboardStore";
import { Button, Card, Spinner } from "flowbite-react";
import { useEffect, useState } from "react";
import { HiRefresh, HiSparkles } from "react-icons/hi";

export default function AiInsightCard() {
  const { data, aiInsight, isAiLoading, setAiLoading, setAiInsight } =
    useDashboardStore();
  const [displayText, setDisplayText] = useState("");

  // Эффект печатающегося текста
  useEffect(() => {
    if (aiInsight) {
      let i = 0;
      setDisplayText("");
      const interval = setInterval(() => {
        setDisplayText((prev) => prev + aiInsight.charAt(i));
        i++;
        if (i >= aiInsight.length) clearInterval(interval);
      }, 20); // Скорость печати
      return () => clearInterval(interval);
    }
  }, [aiInsight]);

  const handleFetchInsight = async () => {
    if (!data || !data.id) return; // Убеждаемся, что данные загружены

    setAiLoading(true);
    try {
      const response = await getAiInsight(data.id, data);
      setAiInsight(response.insight);
    } catch (error) {
      setAiInsight("Ошибка анализа.");
    } finally {
      setAiLoading(false);
    }
  };

  if (!data) return null;

  return (
    <Card className="bg-linear-to-br from-indigo-50 to-blue-50 dark:from-gray-800 dark:to-gray-900 border-l-4 border-l-indigo-500 shadow-sm transition-all duration-300">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div className="flex items-start gap-4">
          <div className="p-2 bg-white dark:bg-gray-700 rounded-lg shadow-sm shrink-0">
            <HiSparkles
              className={`w-6 h-6 ${isAiLoading ? "text-purple-500 animate-pulse" : "text-indigo-600"}`}
            />
          </div>
          <div className="flex-1">
            <h5 className="text-xs font-bold tracking-wider text-indigo-800 dark:text-indigo-300 uppercase">
              AI Финансовый Прогноз
            </h5>

            {isAiLoading ? (
              <div className="flex items-center gap-2 mt-2 text-gray-500">
                <Spinner size="sm" color="info" />
                <span className="text-sm italic">
                  Ollama изучает транзакции клиента...
                </span>
              </div>
            ) : (
              <div className="mt-1 text-sm text-gray-700 dark:text-gray-300 italic min-h-5">
                {aiInsight ? (
                  <span>"{displayText}"</span>
                ) : (
                  <span className="opacity-60">
                    Нажмите для генерации анализа текущих лимитов и рисков.
                  </span>
                )}
              </div>
            )}
          </div>
        </div>

        <div className="flex shrink-0">
          <Button
            size="sm"
            color="indigo"
            onClick={handleFetchInsight}
            disabled={isAiLoading}
            className="shadow-md"
          >
            {aiInsight ? (
              <HiRefresh className="mr-2 h-4 w-4" />
            ) : (
              <HiSparkles className="mr-2 h-4 w-4" />
            )}
            {aiInsight ? "Пересчитать" : "Анализ ИИ"}
          </Button>
        </div>
      </div>
    </Card>
  );
}
