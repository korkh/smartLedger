"use client";

import { getAiInsightDashboard } from "@/app/actions/dashboardActions";
import { useDashboardStore } from "@/app/hooks/useDashboardStore";
import { Button, Card, Spinner } from "flowbite-react";
import { useEffect, useState } from "react";
import { HiRefresh, HiSparkles } from "react-icons/hi";
import { getAiInsightForClient } from "../actions/clientsActions";

interface AiInsightCardProps {
  mode?: "dashboard" | "client";
  externalData?: any;
}

export default function AiInsightCard({
  mode = "dashboard",
  externalData,
}: AiInsightCardProps) {
  // Подключаемся к стору
  const {
    data: storeData,
    aiInsight: storeInsight,
    isAiLoading: storeLoading,
    setAiLoading,
    setAiInsight,
  } = useDashboardStore();

  // Локальное состояние для режима "client", чтобы не засорять глобальный стор дашборда
  const [localInsight, setLocalInsight] = useState<string>("");
  const [localLoading, setLocalLoading] = useState(false);
  const [displayText, setDisplayText] = useState("");

  // Определяем, какие данные и состояния использовать
  const isDashboard = mode === "dashboard";
  const currentData = isDashboard ? storeData : externalData;
  const currentInsight = isDashboard ? storeInsight : localInsight;
  const isLoading = isDashboard ? storeLoading : localLoading;

  // Эффект печатающегося текста
  useEffect(() => {
    if (currentInsight) {
      let i = 0;
      setDisplayText("");
      const interval = setInterval(() => {
        setDisplayText((prev) => prev + currentInsight.charAt(i));
        i++;
        if (i >= currentInsight.length) clearInterval(interval);
      }, 15); // Чуть ускорила печать для динамичности
      return () => clearInterval(interval);
    }
  }, [currentInsight]);

  const handleFetchInsight = async () => {
    if (!currentData || !currentData.id) return;

    // Установка состояния загрузки
    if (isDashboard) setAiLoading(true);
    else setLocalLoading(true);

    try {
      if (isDashboard) {
        const response = await getAiInsightDashboard(
          currentData.id,
          currentData,
        );

        setAiInsight(response.insight);
      } else {
        const response = await getAiInsightForClient(
          currentData.id,
          currentData,
        );
        setLocalInsight(response.insight);
      }
    } catch (error) {
      const errorMsg = "Ошибка анализа. Попробуйте еще раз.";
      if (isDashboard) setAiInsight(errorMsg);
      else setLocalInsight(errorMsg);
    } finally {
      if (isDashboard) setAiLoading(false);
      else setLocalLoading(false);
    }
  };

  if (!currentData) return null;

  return (
    <Card className="bg-linear-to-br from-indigo-50 to-blue-50 dark:from-gray-800 dark:to-gray-900 border-l-4 border-l-indigo-500 shadow-sm transition-all duration-300">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div className="flex items-start gap-4">
          <div className="p-2 bg-white dark:bg-gray-700 rounded-lg shadow-sm shrink-0">
            <HiSparkles
              className={`w-6 h-6 ${isLoading ? "text-purple-500 animate-pulse" : "text-indigo-600"}`}
            />
          </div>
          <div className="flex-1">
            <h5 className="text-xs font-bold tracking-wider text-indigo-800 dark:text-indigo-300 uppercase">
              AI Финансовый анализ{" "}
              {!isDashboard && `— ${currentData.firstName}`}
            </h5>

            {isLoading ? (
              <div className="flex items-center gap-2 mt-2 text-gray-500">
                <Spinner size="sm" color="info" />
                <span className="text-sm italic">
                  Ollama изучает данные {isDashboard ? "дашборда" : "клиента"}
                  ...
                </span>
              </div>
            ) : (
              <div className="mt-1 text-sm text-gray-700 dark:text-gray-300 italic min-h-5">
                {currentInsight ? (
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
            disabled={isLoading}
            className="shadow-md"
          >
            {currentInsight ? (
              <HiRefresh className="mr-2 h-4 w-4" />
            ) : (
              <HiSparkles className="mr-2 h-4 w-4" />
            )}
            {currentInsight ? "Пересчитать" : "Анализ ИИ"}
          </Button>
        </div>
      </div>
    </Card>
  );
}
