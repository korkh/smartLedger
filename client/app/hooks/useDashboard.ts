import { fetchWrapper } from "@/lib/fetchWrapper";
import { DashboardData } from "@/Types";
import { useEffect } from "react";
import { useDashboardStore } from "./useDashboardStore";

// Создаем интерфейс для фильтров
export interface DashboardFilters {
  month?: number;
  year?: number;
  fromDate?: string;
  toDate?: string;
  periodType: "today" | "month" | "custom" | "all";
}

export const useDashboard = (clientId: string, filters: DashboardFilters) => {
  const { setDashboardData, setLoading } = useDashboardStore();

  useEffect(() => {
    const fetchDashboard = async () => {
      if (!clientId) return;

      setLoading(true);
      try {
        // Формируем query-строку на основе фильтров
        const params = new URLSearchParams();

        if (filters.periodType === "month") {
          params.append("month", filters.month?.toString() || "");
          params.append("year", filters.year?.toString() || "");
        } else if (filters.periodType === "custom") {
          params.append("fromDate", filters.fromDate || "");
          params.append("toDate", filters.toDate || "");
        } else if (filters.periodType === "today") {
          const today = new Date().toISOString().split("T")[0];
          params.append("fromDate", today);
          params.append("toDate", today);
        } else if (filters.periodType === "all") {
          params.append("all", "true");
        }

        const queryString = params.toString();
        const url = `dashboard/${clientId}${queryString ? `?${queryString}` : ""}`;

        const response = await fetchWrapper.get<DashboardData>(url);

        if (response && !("error" in response)) {
          setDashboardData(response as DashboardData);
        }
      } catch (error) {
        console.error("Fetch error:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchDashboard();
    // Хук перезапустится, если изменится ID клиента или любой из фильтров
  }, [
    clientId,
    filters.periodType,
    filters.month,
    filters.year,
    filters.fromDate,
    filters.toDate,
    setDashboardData,
    setLoading,
  ]);
};
