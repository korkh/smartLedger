import { create } from "zustand";

type State = {
  pageNumber: number; // Соответствует PagingParams.PageNumber
  pageSize: number; // Соответствует PagingParams.PageSize
  totalCount: number;
  totalPages: number;
  search: string; // Соответствует ClientParams.Search
  sortField: string; // Соответствует ClientParams.SortField
  order: string; // Соответствует ClientParams.Order
  taxRiskLevel: string; // Пример фильтра из ClientParams
  ecpWarningOnly: boolean; // Твой новый фильтр для ЭЦП
};

type Actions = {
  setParams: (params: Partial<State>) => void;
  resetParams: () => void;
};

const initialState: State = {
  pageNumber: 1,
  pageSize: 10,
  totalCount: 0,
  totalPages: 1,
  search: "",
  sortField: "firstname",
  order: "asc",
  taxRiskLevel: "",
  ecpWarningOnly: false,
};

export const useParamsStore = create<State & Actions>((set) => ({
  ...initialState,
  setParams: (newParams: Partial<State>) => {
    set((state) => {
      // Если меняется только страница, сохраняем остальные параметры
      if (newParams.pageNumber) {
        return { ...state, ...newParams };
      }
      // Если меняется фильтр или поиск, всегда сбрасываем на 1-ю страницу
      return { ...state, ...newParams, pageNumber: 1 };
    });
  },
  resetParams: () => set(initialState),
}));
