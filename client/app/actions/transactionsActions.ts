import { fetchWrapper } from "@/lib/fetchWrapper";
import { Client, PagedResult, Transaction } from "@/Types";

// Получение списка транзакций
export async function getTransactions(
  params: string, // передаем уже готовую строку параметров
): Promise<PagedResult<Transaction>> {
  return fetchWrapper.get(`transactions?${params}`);
}

export async function getTransactionById(id: string): Promise<Transaction> {
  return fetchWrapper.get(`transactions/${id}`);
}

// НОВОЕ: Получение списка клиентов для выпадающего списка
export async function getClientsLookup(): Promise<Client[]> {
  // Предполагаем, что на бэкенде есть эндпоинт для краткого списка
  // Если его нет, используем основной эндпоинт клиентов
  return fetchWrapper.get("clients");
}

// НОВОЕ: Создание транзакции
export async function createTransactionApi(
  transaction: any,
): Promise<Transaction> {
  return fetchWrapper.post("transactions", transaction);
}
