import { PagedResult, Transaction } from "@/Types";
import { fetchWrapper } from "@/lib/fetchWrapper";
import { create } from "zustand";

interface TransactionState {
  transactions: Transaction[];
  totalCount: number;
  pageNumber: number;
  totalPages: number;
  loading: boolean;
  params: {
    pageNumber: number;
    pageSize: number;
    searchTerm?: string;
  };
  // Actions
  setParams: (newParams: Partial<TransactionState["params"]>) => void;
  loadTransactions: () => Promise<void>;
  softDeleteTransaction: (id: string) => Promise<void>;
  hardDeleteTransaction: (id: string) => Promise<void>;
  createTransaction: (transaction: Partial<Transaction>) => Promise<boolean>;
}

export const useTransactionStore = create<TransactionState>((set, get) => ({
  transactions: [],
  totalCount: 0,
  pageNumber: 1,
  totalPages: 0,
  loading: false,
  params: {
    pageNumber: 1,
    pageSize: 8,
    searchTerm: "",
  },

  setParams: (newParams) => {
    set((state) => ({ params: { ...state.params, ...newParams } }));
  },

  loadTransactions: async () => {
    set({ loading: true });
    const { params } = get();
    const searchParams = new URLSearchParams();
    searchParams.append("pageNumber", params.pageNumber.toString());
    searchParams.append("pageSize", params.pageSize.toString());
    if (params.searchTerm) searchParams.append("searchTerm", params.searchTerm);

    const result: PagedResult<Transaction> = await fetchWrapper.get(
      `transactions?${searchParams.toString()}`,
    );

    if (result && "items" in result) {
      set({
        transactions: result.items,
        totalCount: result.totalCount,
        totalPages: result.totalPages,
        pageNumber: result.pageNumber,
        loading: false,
      });
    } else {
      set({ loading: false });
    }
  },
  createTransaction: async (transaction: Partial<Transaction>) => {
    set({ loading: true });
    try {
      const result = await fetchWrapper.post("transactions", transaction);
      if (result && !result.error) {
        // Обновляем список после создания
        const { loadTransactions } = get();
        await loadTransactions();
        return true;
      }
      return false;
    } finally {
      set({ loading: false });
    }
  },

  // Мягкое удаление (Senior+)
  softDeleteTransaction: async (id: string) => {
    const result = await fetchWrapper.post(
      `transactions/${id}/soft-delete`,
      {},
    );
    if (!result.error) {
      set((state) => ({
        transactions: state.transactions.filter((t) => t.id !== id),
        totalCount: state.totalCount - 1,
      }));
    }
  },

  // Полное удаление (Admin только)
  hardDeleteTransaction: async (id: string) => {
    const result = await fetchWrapper.del(`transactions/${id}`);
    if (!result.error) {
      set((state) => ({
        transactions: state.transactions.filter((t) => t.id !== id),
        totalCount: state.totalCount - 1,
      }));
    }
  },
}));
