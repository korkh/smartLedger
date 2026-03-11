import { DashboardData } from "@/Types";
import { create } from "zustand";

type DashboardState = {
  data: DashboardData | null;
  isLoading: boolean;
  isAiLoading: boolean; // Состояние загрузки ИИ
  aiInsight: string | null; // Результат анализа
};

type Actions = {
  setDashboardData: (data: DashboardData) => void;
  setLoading: (status: boolean) => void;
  setAiLoading: (status: boolean) => void;
  setAiInsight: (insight: string) => void;
};

const initialState: DashboardState = {
  data: null,
  isLoading: false,
  isAiLoading: false,
  aiInsight: null,
};

export const useDashboardStore = create<DashboardState & Actions>((set) => ({
  ...initialState,
  setDashboardData: (data: DashboardData) =>
    set({
      data: data,
      isLoading: false,
      aiInsight: data.aiInsight || null, // Инициализируем, если пришло с бэкенда
    }),
  setLoading: (status: boolean) => set({ isLoading: status }),
  setAiLoading: (status: boolean) => set({ isAiLoading: status }),
  setAiInsight: (insight: string) =>
    set({ aiInsight: insight, isAiLoading: false }),
}));
