// useDashboard.ts

import { fetchWrapper } from "@/lib/fetchWrapper";
import { DashboardData } from "@/Types";
import { useEffect } from "react";
import { useDashboardStore } from "./useDashboardStore";

export const useDashboard = (clientId: string) => {
  const { setDashboardData, setLoading } = useDashboardStore();

  useEffect(() => {
    const fetchDashboard = async () => {
      if (!clientId) return;

      setLoading(true);
      try {
        // Явно указываем ожидаемый тип <DashboardData>
        // Если fetchWrapper не поддерживает дженерики, используем "as"
        const response = await fetchWrapper.get<DashboardData>(
          `dashboard/${clientId}`,
        );

        // Проверяем, что response — это именно данные, а не объект с ошибкой
        if (response && !("error" in response)) {
          setDashboardData(response as DashboardData);
        } else {
          console.error("API Error:", response);
        }
      } catch (error) {
        console.error("Fetch error:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchDashboard();
  }, [clientId, setDashboardData, setLoading]);
};
