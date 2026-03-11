import { DashboardData } from "@/Types";
import { create } from "zustand";

type DashboardState = {
  data: DashboardData | null;
  isLoading: boolean;
};

type Actions = {
  setDashboardData: (data: DashboardData) => void;
  setLoading: (status: boolean) => void;
};

const initialState: DashboardState = {
  data: null,
  isLoading: false,
};

export const useDashboardStore = create<DashboardState & Actions>((set) => ({
  ...initialState,
  setDashboardData: (data: DashboardData) =>
    set({ data: data, isLoading: false }),
  setLoading: (status: boolean) => set({ isLoading: status }),
}));
