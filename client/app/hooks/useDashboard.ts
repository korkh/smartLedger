import { fetchWrapper } from "@/lib/fetchWrapper";
import { DashboardData } from "@/Types";
import { format, parseISO } from "date-fns";
import { useEffect } from "react";
import { useDashboardStore } from "./useDashboardStore";

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
        const params = new URLSearchParams();
        const DATE_FORMAT = "yyyy-MM-dd";

        switch (filters.periodType) {
          case "month":
            // Убеждаемся, что не передаем undefined в строку
            if (filters.month) params.append("month", filters.month.toString());
            if (filters.year) params.append("year", filters.year.toString());
            break;

          case "today":
            const today = new Date();
            const todayStr = format(today, DATE_FORMAT);
            params.append("fromDate", todayStr);
            params.append("toDate", todayStr);
            break;

          case "custom":
            // Используем parseISO, чтобы избежать смещения часовых поясов
            if (filters.fromDate) {
              const dFrom = parseISO(filters.fromDate);
              params.append("fromDate", format(dFrom, DATE_FORMAT));
            }
            if (filters.toDate) {
              const dTo = parseISO(filters.toDate);
              params.append("toDate", format(dTo, DATE_FORMAT));
            }
            break;

          case "all":
            params.append("all", "true");
            break;
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
